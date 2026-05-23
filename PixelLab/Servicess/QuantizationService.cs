using System;

namespace PixelLab.Services
{
  
    public static class QuantizationService
    {
        public static void RunOptimizedKMeans(
            byte[] pixels, int width, int height, int k, out byte[] palette)
        {
            int totalPixels = width * height;
            int channels = 3; 

            palette = new byte[k * channels];
            int[] labels = new int[totalPixels];
            bool[] isStable = new bool[totalPixels];       
            int[] stabilityCount = new int[totalPixels];   

            // 1. K-Means++ Initialization
            InitializeKMeansPlusPlus(pixels, totalPixels, k, palette, channels);

            int maxIter = 12;
            double epsilon = 0.5;

            for (int iter = 0; iter < maxIter; iter++)
            {
                bool converged = true;
                double maxShift = 0;

                // 2. Squared Euclidean + 3. Stable Items Removal
                for (int i = 0; i < totalPixels; i++)
                {
                    if (isStable[i]) continue;

                    int idx = i * channels;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];

                    double minDist = double.MaxValue;
                    int bestCluster = 0;

                    for (int c = 0; c < k; c++)
                    {
                        int cIdx = c * channels;
                        double db = b - palette[cIdx];
                        double dg = g - palette[cIdx + 1];
                        double dr = r - palette[cIdx + 2];
                        double dist = db * db + dg * dg + dr * dr; 

                        if (dist < minDist) { minDist = dist; bestCluster = c; }
                    }

                    if (labels[i] != bestCluster)
                    {
                        labels[i] = bestCluster;
                        isStable[i] = false;
                        stabilityCount[i] = 0; 
                        converged = false;
                    }
                    else
                    {
                        stabilityCount[i]++; 
                        if (stabilityCount[i] >= 3)
                            isStable[i] = true;
                    }
                }

                // Update Centroids
                byte[] newCentroids = UpdateCentroids(pixels, labels, totalPixels, k, channels);

                // 4. Adaptive Convergence
                for (int c = 0; c < k; c++)
                {
                    int cIdx = c * channels;
                    double db = newCentroids[cIdx] - palette[cIdx];
                    double dg = newCentroids[cIdx + 1] - palette[cIdx + 1];
                    double dr = newCentroids[cIdx + 2] - palette[cIdx + 2];
                    double shift = Math.Sqrt(db * db + dg * dg + dr * dr);
                    if (shift > maxShift) maxShift = shift;
                }

                Buffer.BlockCopy(newCentroids, 0, palette, 0, palette.Length);
                if (converged || maxShift < epsilon) break;
            }
        }

        private static void InitializeKMeansPlusPlus(byte[] data, int count, int k, byte[] centers, int channels)
        {
            Random rand = new Random(42);
            int first = rand.Next(count) * channels;
            Buffer.BlockCopy(data, first, centers, 0, channels);

            for (int c = 1; c < k; c++)
            {
                double[] distances = new double[count];
                double sum = 0;

                for (int i = 0; i < count; i++)
                {
                    int idx = i * channels;
                    double minD = double.MaxValue;
                    for (int j = 0; j < c; j++)
                    {
                        int cIdx = j * channels;
                        double db = data[idx] - centers[cIdx];
                        double dg = data[idx + 1] - centers[cIdx + 1];
                        double dr = data[idx + 2] - centers[cIdx + 2];
                        double d = db * db + dg * dg + dr * dr;
                        if (d < minD) minD = d;
                    }
                    distances[i] = minD;
                    sum += minD;
                }

                double r = rand.NextDouble() * sum;
                double acc = 0;
                for (int i = 0; i < count; i++)
                {
                    acc += distances[i];
                    if (acc >= r)
                    {
                        int idx = i * channels;
                        Buffer.BlockCopy(data, idx, centers, c * channels, channels);
                        break;
                    }
                }
            }
        }

        private static byte[] UpdateCentroids(byte[] data, int[] labels, int count, int k, int channels)
        {
            byte[] newCenters = new byte[k * channels];
            int[] clusterCount = new int[k];
            double[] sumR = new double[k], sumG = new double[k], sumB = new double[k];

            for (int i = 0; i < count; i++)
            {
                int label = labels[i];
                int idx = i * channels;
                clusterCount[label]++;
                sumB[label] += data[idx];
                sumG[label] += data[idx + 1];
                sumR[label] += data[idx + 2];
            }

            for (int c = 0; c < k; c++)
            {
                if (clusterCount[c] > 0)
                {
                    newCenters[c * channels] = (byte)(sumB[c] / clusterCount[c]);
                    newCenters[c * channels + 1] = (byte)(sumG[c] / clusterCount[c]);
                    newCenters[c * channels + 2] = (byte)(sumR[c] / clusterCount[c]);
                }
                else
                {
                    Random rand = new Random();
                    int idx = rand.Next(count) * channels;
                    Buffer.BlockCopy(data, idx, newCenters, c * channels, channels);
                }
            }
            return newCenters;
        }

        
        public static Func<byte, byte, byte, int> BuildLookup(byte[] palette)
        {
            int k = palette.Length / 3;
            return (r, g, b) => {
                double minDist = double.MaxValue;
                int best = 0;
                for (int i = 0; i < k; i++)
                {
                    int idx = i * 3;
                    double dr = r - palette[idx + 2], dg = g - palette[idx + 1], db = b - palette[idx];
                    double dist = dr * dr + dg * dg + db * db;
                    if (dist < minDist) { minDist = dist; best = i; }
                }
                return best;
            };
        }
    }
}
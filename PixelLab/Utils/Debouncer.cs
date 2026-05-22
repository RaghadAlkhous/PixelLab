using System;
using System.Windows.Forms;

namespace PixelLab.Utils
{
    public sealed class Debouncer : IDisposable
    {
        private readonly Timer _timer;
        private Action _action;

        public Debouncer(int intervalMilliseconds)
        {
            if (intervalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalMilliseconds));

            _timer = new Timer();
            _timer.Interval = intervalMilliseconds;
            _timer.Tick += Timer_Tick;
        }

        public void Debounce(Action action)
        {
            _action = action;

            _timer.Stop();
            _timer.Start();
        }

        public void Cancel()
        {
            _timer.Stop();
            _action = null;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            Action action = _action;
            _action = null;

            if (action != null)
                action();
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _timer.Dispose();
        }
    }
}

using System;
using System.Threading;
using Digger.Interface;

namespace Digger.Win32
{
    class Timer : ITimer
    {
        private int time;

        public void Start()
        {
            time = Environment.TickCount;
        }

        public int Time => Environment.TickCount;

        public void SyncFrame(int fps)
        {
            int delta = Time - time;
            int delay = 1000 / fps - delta;
#if DEBUG
            if (delay < 0)
                delay = 80;
#endif
            Thread.Sleep(delay);
            Start();
        }
    }
}

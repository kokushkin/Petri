using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetriLib
{
    public class Timer
    {
        DateTime LastTime;
        TimeSpan SumTime;
        TimeSpan Interval;
        public bool IsRun { get { return run; } }
        bool run;

        public Timer(TimeSpan _Interval)
        {
            run = false;
            Interval = _Interval;
            SumTime = new TimeSpan();
        }

        public void Start()
        {
            if (!IsRun)
            {
                LastTime = DateTime.Now;
                run = true;
            }
        }

        public void Stop()
        {
            if (IsRun)
            {
                SumTime += (DateTime.Now - LastTime);
                run = false;
            }
        }

        public void Reset()
        {
            LastTime = DateTime.Now;
            SumTime = new TimeSpan();
        }

        public bool IsElapsed()
        {
            DateTime now_time = DateTime.Now;
            if ((now_time - LastTime) + SumTime > Interval)
                return true;
            else
                return false;
        }

        public int ElapsedTime
        {
            get
            {
                DateTime now_time = DateTime.Now;
                return Convert.ToInt32((now_time - LastTime + SumTime).TotalMilliseconds);
            }
        }
    }
}

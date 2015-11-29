using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetriLib
{
    public interface IBlocked
    {
        object GetBlock();

        ConcurrentDictionary<int, bool> LockDic
        {
            get;
        }

        ConcurrentDictionary<int, Timer> TimerDic
        {
            get;
        }

        bool WasChanged
        {
            get;

            set;
        }
    }




    public enum ExecutionMode
    {
        Synch = 0,
        Asynch = 1
    }


    public class GeneralLocker
    {
        public bool WasChanged = false;

        ConcurrentDictionary<int, bool> lockDic = new ConcurrentDictionary<int, bool>();

        public ConcurrentDictionary<int, bool> LockDic
        {
            get
            {
                return lockDic;
            }
        }

        ConcurrentDictionary<int, Timer> timerDic = new ConcurrentDictionary<int, Timer>();

        public ConcurrentDictionary<int, Timer> TimerDic
        {
            get
            {
                return timerDic;
            }
        }

    }



}

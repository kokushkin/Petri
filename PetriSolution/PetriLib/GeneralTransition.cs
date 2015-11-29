using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetriLib
{

    public class GeneralTransition<T> where T : IBlocked
    {
        Predicate<IEnumerable<T>> Function;
        Predicate<T> SolvePredicate;
        Action<T> DecreaseAction;
        Action<T> IncreaseAction;
        ExecutionMode mode;
        int key;
        public int Key { get { return key; } }


        GeneralLocker gLocker;


        List<T> Objects;

        public GeneralTransition(Predicate<IEnumerable<T>> _Function, Predicate<T> _SolvePredicate, Action<T> _DecreaseAction, Action<T> _IncreaseAction,
            ExecutionMode _mode, int _key, GeneralLocker _gLocker, int _TimePeriod, List<T> _Objects)
        {
            Function = _Function;
            SolvePredicate = _SolvePredicate;
            DecreaseAction = _DecreaseAction;
            IncreaseAction = _IncreaseAction;
            mode = _mode;
            key = _key;

            gLocker = _gLocker;

            gLocker.LockDic[key] = false;
            if (_TimePeriod > 0)
                gLocker.TimerDic[key] = new Timer(new TimeSpan(0, 0, _TimePeriod));
            Objects = _Objects;
        }

        //public bool IsReady(T Object)
        //{
        //    return SolvePredicate(Object);
        //}

        bool DoAction(IEnumerable<T> Objects)
        {
            if (Function(Objects))
            {
                StaticExit(Objects, DecreaseAction, IncreaseAction, ref gLocker.WasChanged, gLocker.LockDic, key);
                return true;
            }
            else
                return false;
        }

        public void Do()
        {

            List<T> readyObjects = new List<T>();

            foreach (var objct in Objects)
            {
                bool result = false;
                lock (objct.GetBlock())
                    result = SolvePredicate(objct);
                if (result)
                    readyObjects.Add(objct);
            }

            if (readyObjects.Count == 0)
                return;

            if (StaticEntrance(readyObjects, SolvePredicate, gLocker.LockDic, gLocker.TimerDic, key))
            {
                if (mode == ExecutionMode.Synch)
                    DoAction(readyObjects);

                else if (mode == ExecutionMode.Asynch)
                    try
                    {
                        ThreadPool.QueueUserWorkItem(o1 => DoAction(readyObjects));
                    }
                    catch(Exception ex)
                    {
                        Log.WriteException(ex, "Petri_Proc");
                        Log.WriteMessage("Error into GeneralTransiton", MessageLevel.Error, "Petri_Proc");
                    }


            }
        }

        public static bool StaticEntrance(IEnumerable<T> Objects, Predicate<T> SolvePredicate, ConcurrentDictionary<int, bool> LockDic, ConcurrentDictionary<int, Timer> TimerDic, int key)
        {
            if (!LockDic[key] && (!TimerDic.ContainsKey(key) || TimerDic[key].IsElapsed()))
            {
                bool solveAnsw = true;

                foreach (var objct in Objects)
                {
                    lock (objct.GetBlock())
                    {
                        solveAnsw = SolvePredicate(objct);
                        if (!solveAnsw)
                            break;
                    }
                }

                if (solveAnsw)
                {
                    if (TimerDic.ContainsKey(key)) //if first time we must start
                    {
                        TimerDic[key].Start(); 
                        TimerDic[key].Reset();
                    }

                    LockDic[key] = true;

                    return true;

                }
                else
                    return false;


            }

            else
                return false;
        }

        public static void StaticExit(IEnumerable<T> Objects, Action<T> DecreaseAction, Action<T> IncreaseAction, ref bool WasChanged, ConcurrentDictionary<int, bool> LockDic, int key)
        {
            foreach (var obj in Objects)
            {
                lock (obj.GetBlock())
                {
                    DecreaseAction(obj);
                    IncreaseAction(obj);
                }
            }

            WasChanged = true; //no thread safe operation
            LockDic[key] = false;
        }

    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetriLib
{
    public class Transition<T> where T : IBlocked
    {

        Predicate<T> Function;
        Predicate<T> SolvePredicate;
        Action<T> DecreaseAction;
        Action<T> IncreaseAction;
        ExecutionMode mode;
        int key;
        int TimePeriod;



        public Transition(Predicate<T> _Function, Predicate<T> _SolvePredicate, Action<T> _DecreaseAction, Action<T> _IncreaseAction, ExecutionMode _mode, int _key, int _TimePeriod = 0)
        {
            Function = _Function;
            SolvePredicate = _SolvePredicate;
            DecreaseAction = _DecreaseAction;
            IncreaseAction = _IncreaseAction;
            mode = _mode;
            key = _key;

            TimePeriod = _TimePeriod;

        }


        bool DoAction(T obj)
        {
            if (Function(obj))
            {
                StaticExit(obj, DecreaseAction, IncreaseAction, obj.LockDic, key);
                return true;
            }
            else
            {
                obj.LockDic[key] = false;
                return false;
            }

        }


        public void Do(T Obj)
        {
            if (TimePeriod > 0 && !Obj.TimerDic.ContainsKey(key))
                Obj.TimerDic[key] = new Timer(new TimeSpan(0, 0, TimePeriod));

            if (StaticEntrance(Obj, SolvePredicate, Obj.LockDic, Obj.TimerDic, key))
            {
                if (mode == ExecutionMode.Synch)
                    DoAction(Obj);

                else if (mode == ExecutionMode.Asynch)
                    try
                    {
                        ThreadPool.QueueUserWorkItem(o1 => DoAction(Obj));
                    }
                    catch(Exception ex)
                    {
                        Log.WriteException(ex, "Petri_Proc");
                        Log.WriteMessage("Ошибка в Transiton", MessageLevel.Error, "Petri_Proc");
                    }

            }
        }



        public static bool StaticEntrance(T Obj, Predicate<T> SolvePredicate, ConcurrentDictionary<int, bool> LockDic, ConcurrentDictionary<int, Timer> TimerDic, int key)
        {

            if ((!LockDic.ContainsKey(key) || !LockDic[key]) && (!TimerDic.ContainsKey(key) || TimerDic[key].IsElapsed()))
            {
                bool solveAnsw = false;
                lock (Obj.GetBlock())
                {
                    solveAnsw = SolvePredicate(Obj);
                }
                if (solveAnsw)
                {
                    if (TimerDic.ContainsKey(key))
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

        public static void StaticExit(T obj, Action<T> DecreaseAction, Action<T> IncreaseAction, ConcurrentDictionary<int, bool> LockDic, int key)
        {

            lock (obj.GetBlock())
            {
                DecreaseAction(obj);
                IncreaseAction(obj);
                obj.WasChanged = true;
                LockDic[key] = false;
            }

        }


    }
}
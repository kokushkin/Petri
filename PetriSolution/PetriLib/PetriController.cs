using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetriLib
{

    public class PetriController<T> where T : IBlocked
    {

        List<Transition<T>> Transitions;
        List<GeneralTransition<T>> GeneralTransitions;

        List<T> Objects;


        public GeneralLocker gLocker;


        //Predicate<T> IsEndState;
        Action<T> ResetStates;


        public PetriController()
        {
            Transitions = new List<Transition<T>>();
            GeneralTransitions = new List<GeneralTransition<T>>();
            gLocker = new GeneralLocker();

            Objects = new List<T>();
        }


        //public void AddObjects(IEnumerable<T> _Objects)
        //{
        //    Objcts.AddRange(_Objects);
        //}

        public void SetObjects(List<T> _Objects)
        {
            //Objects = _Objects;
            Objects.Clear();
            Objects.AddRange(_Objects);
        }

        public void AddTransition(Predicate<T> Function, Predicate<T> SolvePredicate, Action<T> DecreaseAction, Action<T> IncreaseAction, ExecutionMode mode, int TimePeriod = 0)
        {
            Transitions.Add(new Transition<T>(Function, SolvePredicate, DecreaseAction, IncreaseAction, mode, Transitions.Count(), TimePeriod));
        }

        public void AddGeneralTransition(Predicate<IEnumerable<T>> Function, Predicate<T> SolvePredicate, Action<T> DecreaseAction, Action<T> IncreaseAction, ExecutionMode mode, int TimePeriod = 0)
        {
            GeneralTransitions.Add(new GeneralTransition<T>(Function, SolvePredicate, DecreaseAction, IncreaseAction, mode, GeneralTransitions.Count(), gLocker, TimePeriod, Objects));
        }



        //public void SetCheckEndState(Predicate<T> CheckEndState)
        //{
        //    IsEndState = CheckEndState;
        //}

        public void SetReset(Action<T> Reset)
        {
            ResetStates = Reset;
        }

        public void Run()
        {
            foreach (var obj in Objects)
            {
                ResetStates(obj);
            }

            do
            {
                gLocker.WasChanged = false;

                Parallel.ForEach(Objects, o => WorkWhileCan(o, Transitions));

                foreach (var gentr in GeneralTransitions)
                    gentr.Do();

            }
            while (!Objects.All(o => o.LockDic.ToArray().All(o1 => o1.Value == false) && o.WasChanged == false) ||
                gLocker.WasChanged != false || !gLocker.LockDic.ToArray().All(o1 => o1.Value == false));
        }

        public static void WorkWhileCan(T obj, List<Transition<T>> Transitions)
        {
            while (true)
            {
                obj.WasChanged = false;
                foreach (var trns in Transitions)
                    trns.Do(obj);
                //exit condition
                if (obj.LockDic.ToArray().All(o1 => o1.Value == false) && obj.WasChanged == false)
                    break;
            };
        }

    }
}

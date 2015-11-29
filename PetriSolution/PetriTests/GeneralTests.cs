using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using PetriLib;


namespace PetriTests
{

    public class TestClass : IBlocked
    {
        public ConcurrentDictionary<int, bool> lock_dick = new ConcurrentDictionary<int, bool>();

        public object BlockState = new object();

        public ConcurrentDictionary<int, bool> LockDic
        {
            get
            {
                return lock_dick;
            }
        }

        public ConcurrentDictionary<int, Timer> timer_dic = new ConcurrentDictionary<int, Timer>();

        public ConcurrentDictionary<int, Timer> TimerDic
        {
            get
            {
                return timer_dic;
            }
        }

        public object GetBlock()
        {
            return BlockState;
        }

        public bool WasChanged
        {
            get;
            set;
        }


        public int Number;
        public string Word;
    }



    [TestClass]
    public class PetriTests
    {

        [TestMethod]
        public void GeneralStaticEntranceTest()
        {

            int key = 5;
            var LockDic = new ConcurrentDictionary<int, bool>();
            var TimerDic = new ConcurrentDictionary<int, Timer>();
            Predicate<TestClass> SolvePredicate = o => false;
            IEnumerable<TestClass> Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 10);

            //check without locking
            //(without timer)

            LockDic[key] = false;

            SolvePredicate = o => false;
            bool answerFalse = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);
            Assert.IsFalse(LockDic[key]);

            SolvePredicate = o => true;
            bool answerTrue = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(LockDic[key]);


            //(with timer)
            LockDic[key] = false;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0));

            SolvePredicate = o => false;
            answerFalse = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            SolvePredicate = o => true;
            answerTrue = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(LockDic[key]);


            //check for locking
            LockDic[key] = true;
            SolvePredicate = o => true;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0));
            answerFalse = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            //check for timer
            LockDic[key] = false;
            SolvePredicate = o => true;

            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0, 1000));
            TimerDic[key].Start();
            System.Threading.Thread.Sleep(500);
            answerFalse = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            TimerDic[key].Reset();
            int count_before = TimerDic[key].ElapsedTime;
            System.Threading.Thread.Sleep(1500);
            answerTrue = GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(TimerDic[key].ElapsedTime < 500);

            //check for start timer
            LockDic[key] = false;
            SolvePredicate = o => true;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0, 1000));
            GeneralTransition<TestClass>.StaticEntrance(Objects, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(TimerDic[key].IsRun);


        }

        [TestMethod]
        public void GeneralStaticExit()
        {
            int key = 5;
            var LockDic = new ConcurrentDictionary<int, bool>();
            IEnumerable<TestClass> Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 10);
            bool WasChanged = false;

            Action<TestClass> DecreaseAction = new Action<TestClass>(o => { });
            Action<TestClass> IncreaseAction = new Action<TestClass>(o => { });


            //check for release lock
            LockDic[key] = true;
            GeneralTransition<TestClass>.StaticExit(Objects, DecreaseAction, IncreaseAction, ref WasChanged, LockDic, key);
            Assert.IsFalse(LockDic[key]);
       
            //check for set "WasChanged"
            WasChanged = false;
            GeneralTransition<TestClass>.StaticExit(Objects, DecreaseAction, IncreaseAction, ref WasChanged, LockDic, key);
            Assert.IsTrue(WasChanged);

            
            //check for do action
            Objects.AsParallel().ForAll(o => { o.Number = 1; o.Word = "Hello"; });
            DecreaseAction = o => o.Number = 2;
            IncreaseAction = o => o.Word = "Word";

            GeneralTransition<TestClass>.StaticExit(Objects, DecreaseAction, IncreaseAction, ref WasChanged, LockDic, key);
            Assert.IsTrue(Objects.All(o => o.Word == "Word" && o.Number == 2));
        }



        [TestMethod]
        public void StaticEntranceTest()
        {

            int key = 5;
            var LockDic = new ConcurrentDictionary<int, bool>();
            var TimerDic = new ConcurrentDictionary<int, Timer>();
            Predicate<TestClass> SolvePredicate = o => false;
            TestClass Object = new TestClass();

            //check without locking
            //(without timer)

            LockDic[key] = false;

            SolvePredicate = o => false;
            bool answerFalse = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);
            Assert.IsFalse(LockDic[key]);

            SolvePredicate = o => true;
            bool answerTrue = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(LockDic[key]);


            //(with timer)
            LockDic[key] = false;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0));

            SolvePredicate = o => false;
            answerFalse = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            SolvePredicate = o => true;
            answerTrue = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(LockDic[key]);


            //check for locking
            LockDic[key] = true;
            SolvePredicate = o => true;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0));
            answerFalse = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            //check for timer
            LockDic[key] = false;
            SolvePredicate = o => true;

            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0, 1000));
            TimerDic[key].Start();
            System.Threading.Thread.Sleep(500);
            answerFalse = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsFalse(answerFalse);

            TimerDic[key].Reset();
            int count_before = TimerDic[key].ElapsedTime;
            System.Threading.Thread.Sleep(1500);
            answerTrue = Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(answerTrue);
            Assert.IsTrue(TimerDic[key].ElapsedTime < 500);

            //check for start timer
            LockDic[key] = false;
            SolvePredicate = o => true;
            TimerDic[key] = new Timer(new TimeSpan(0, 0, 0, 0, 1000));
            Transition<TestClass>.StaticEntrance(Object, SolvePredicate, LockDic, TimerDic, key);
            Assert.IsTrue(TimerDic[key].IsRun);


        }



        [TestMethod]
        public void StaticExit()
        {
            int key = 5;
            var LockDic = new ConcurrentDictionary<int, bool>();
            TestClass Object = new TestClass();
            Object.WasChanged = false;

            Action<TestClass> DecreaseAction = new Action<TestClass>(o => { });
            Action<TestClass> IncreaseAction = new Action<TestClass>(o => { });


            //check for release locking
            LockDic[key] = true;
            Transition<TestClass>.StaticExit(Object, DecreaseAction, IncreaseAction,  LockDic, key);
            Assert.IsFalse(LockDic[key]);

            //check for set "WasChanged"
            Object.WasChanged = false;
            Transition<TestClass>.StaticExit(Object, DecreaseAction, IncreaseAction,  LockDic, key);
            Assert.IsTrue(Object.WasChanged);


            //check for do actions
            Object.Number = 1; Object.Word = "Hello";
            DecreaseAction = o => o.Number = 2;
            IncreaseAction = o => o.Word = "Word";

            Transition<TestClass>.StaticExit(Object, DecreaseAction, IncreaseAction,  LockDic, key);
            Assert.IsTrue(Object.Word == "Word" && Object.Number == 2);
        }



        public delegate void WorkWhileDelegate(TestClass obj, List<Transition<TestClass>> Transitions);

        [TestMethod]
        public void WorkWhileCanTest()
        {

            //check for finit
            TestClass obj = new TestClass();
            obj.Number = 0;
            obj.Word = "aaa";

            List<Transition<TestClass>> Transitions = new List<Transition<TestClass>>();

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 0 && obj.Word == "aaa",
                o => { }, o => { obj.Number = 1; obj.Word = "bbb"; }, ExecutionMode.Synch, Transitions.Count()));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 1 && obj.Word == "bbb",
                o => { }, o => { obj.Number = 2; obj.Word = "ccc"; }, ExecutionMode.Synch, Transitions.Count()));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 2 && obj.Word == "ccc", 
                o => { }, o => { obj.Number = 3; obj.Word = "ddd"; }, ExecutionMode.Synch, Transitions.Count()));

            WorkWhileDelegate del = new WorkWhileDelegate( PetriController<TestClass>.WorkWhileCan);

            var result = del.BeginInvoke(obj, Transitions, null, null);

            System.Threading.Thread.Sleep(1000);

            Assert.IsTrue(result.IsCompleted);



            //check for infinite cycle
            obj = new TestClass();
            obj.Number = 0;
            obj.Word = "aaa";

            Transitions = new List<Transition<TestClass>>();

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 0 && obj.Word == "aaa",
                o => { }, o => { obj.Number = 1; obj.Word = "bbb"; }, ExecutionMode.Synch, Transitions.Count()));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 1 && obj.Word == "bbb",
                o => { }, o => { obj.Number = 2; obj.Word = "ccc"; }, ExecutionMode.Synch, Transitions.Count()));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 2 && obj.Word == "ccc",
                o => { }, o => { obj.Number = 0; obj.Word = "aaa"; }, ExecutionMode.Synch, Transitions.Count()));

            //del = new WorkWhileDelegate(PetriController<TestClass>.WorkWhileCan);

            System.Threading.Thread checkThread = new System.Threading.Thread(() => PetriController<TestClass>.WorkWhileCan(obj, Transitions));

            //var result1 = del.BeginInvoke(obj, Transitions, null, null);

            checkThread.Start();

            System.Threading.Thread.Sleep(1000);


            //Assert.IsFalse(result1.IsCompleted);
            Assert.IsTrue(checkThread.IsAlive);
            checkThread.Abort();



            //check for do delay
            obj = new TestClass();
            obj.Number = 0;
            obj.Word = "aaa";

            Transitions = new List<Transition<TestClass>>();

            int i = 0;

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 0 && obj.Word == "aaa",
                o => { }, 
                o => { obj.Number = 1; obj.Word = "bbb"; }, ExecutionMode.Synch, Transitions.Count(), 100));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 1 && obj.Word == "bbb",
                o => { }, 
                o => { obj.Number = 2; obj.Word = "ccc"; }, ExecutionMode.Synch, Transitions.Count(), 100));

            Transitions.Add(new Transition<TestClass>(o => true, o => obj.Number == 2 && obj.Word == "ccc",
                o => { },
                o => { obj.Number = 0; obj.Word = "aaa"; i++; }, ExecutionMode.Synch, Transitions.Count(), 100));

            del = new WorkWhileDelegate(PetriController<TestClass>.WorkWhileCan); //won't cycling


            var result2 = del.BeginInvoke(obj, Transitions, null, null);

            System.Threading.Thread.Sleep(500);

            Assert.IsTrue(result2.IsCompleted); 
            Assert.AreEqual(1, i);

            DateTime start = DateTime.Now;

            result2 = del.BeginInvoke(obj, Transitions, null, null); //will get immediately, because timer not elapsed

            del.EndInvoke(result2);

            DateTime finish = DateTime.Now;

            Assert.IsTrue(result2.IsCompleted);
            Assert.IsTrue((finish - start).TotalMilliseconds < 100);

            Assert.AreEqual(1, i); //"i" won't have changed

        }


        [TestMethod]
        public void GeneralTests()
        {
            //test1
            IEnumerable<TestClass> Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 2);

            PetriController<TestClass> pn = new PetriController<TestClass>();

            pn.SetReset(o => { o.Number = 0; o.Word = "aaa"; });

            pn.SetObjects(Objects.ToList());

            pn.AddTransition(o => true, o => o.Number == 0 && o.Word == "aaa", o => { }, o => { o.Number = 1; o.Word = "bbb"; }, ExecutionMode.Synch, 0);
            pn.AddTransition(o => true, o => o.Number == 1 && o.Word == "bbb", o => { }, o => { o.Number = 2; o.Word = "ccc"; }, ExecutionMode.Synch, 0);
            pn.AddGeneralTransition(o => true, o => o.Number == 2 && o.Word == "ccc", o => { }, o => { o.Number = 3; o.Word = "ddd"; }, ExecutionMode.Synch, 0);

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 3 && o.Word == "ddd"));

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 3 && o.Word == "ddd"));

            //test2
            Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 2);

            pn = new PetriController<TestClass>();

            pn.SetReset(o => { o.Number = 0; o.Word = "aaa"; });

            pn.SetObjects(Objects.ToList());

            pn.AddTransition(o => true, o => o.Number == 0 && o.Word == "aaa", o => { }, o => { o.Number = 1; o.Word = "bbb"; }, ExecutionMode.Asynch, 0);
            pn.AddTransition(o => true, o => o.Number == 1 && o.Word == "bbb", o => { }, o => { o.Number = 2; o.Word = "ccc"; }, ExecutionMode.Asynch, 0);
            pn.AddGeneralTransition(o => true, o => o.Number == 2 && o.Word == "ccc", o => { }, o => { o.Number = 3; o.Word = "ddd"; }, ExecutionMode.Asynch, 0);

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 3 && o.Word == "ddd"));

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 3 && o.Word == "ddd"));

            //test3
            Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 2);

            pn = new PetriController<TestClass>();

            pn.SetReset(o => { o.Number = 0; o.Word = "aaa"; });

            pn.SetObjects(Objects.ToList());

            pn.AddTransition(o => true, o => o.Number == 0 && o.Word == "aaa", o => { }, o => { o.Number = 1; o.Word = "bbb"; }, ExecutionMode.Asynch, 0);
            pn.AddTransition(o => true, o => o.Number == 1 && o.Word == "bbb", o => { }, o => { o.Number = 2; o.Word = "ccc"; }, ExecutionMode.Synch, 0);
            pn.AddGeneralTransition(o => true, o => o.Number == 2 && o.Word == "ccc", o => { }, o => { o.Number = 3; o.Word = "ddd"; }, ExecutionMode.Asynch, 0);
            pn.AddTransition(o => true, o => o.Number == 3 && o.Word == "ddd", o => { }, o => { o.Number = 4; o.Word = "eee"; }, ExecutionMode.Synch, 0);

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 4 && o.Word == "eee"));

            pn.Run();

            Assert.IsTrue(Objects.All(o => o.Number == 4 && o.Word == "eee"));

        }



        [TestMethod]
        public void WasChangedGeneralTransitionTest()
        {
            //test1
            IEnumerable<TestClass> Objects = ParallelEnumerable.Repeat<TestClass>(new TestClass(), 2);

            Objects.AsParallel().ForAll(o => { o.Number = 2; o.Word = "ccc"; });

            GeneralLocker gLocker = new GeneralLocker();

            GeneralTransition<TestClass> genTrans = new GeneralTransition<TestClass>(o => true, o => o.Number == 2 && o.Word == "ccc", o => { }, o => { o.Number = 3; o.Word = "ddd"; },
                ExecutionMode.Synch, 0, gLocker, 0, Objects.ToList());

            genTrans.Do();


            Assert.IsTrue(gLocker.WasChanged);

        }

        public void testFunc(ref int value, int u)
        {
            u++;
            value++;
            if (u == 2)
                return;

            testFunc(ref value, u);
        }

        public int testFunc2(ref int value)
        {
            int local_value = value;

            local_value++;

            return local_value;
        }

        [TestMethod]
        public void EasyRefTest()
        {
            int i = 0;
            int u = 0;

            testFunc(ref i, u);

            Assert.AreEqual(2, i);

            int j = 0;
            int answ = testFunc2(ref j);

            //Assert.AreEqual(j, answ);


        }


    }
}

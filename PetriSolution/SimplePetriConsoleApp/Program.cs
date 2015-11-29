using PetriLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePetriConsoleApp
{
    public class MyClass : IBlocked
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

    public class MyLog : ILog
    {
        public void WriteException(Exception ex, string procName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(procName + ":" + ex.ToString());
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void WriteMessage(string msg, MessageLevel level, string procName)
        {
            if (level == MessageLevel.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (level == MessageLevel.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (level == MessageLevel.Notify)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (level == MessageLevel.Information)
                Console.ForegroundColor = ConsoleColor.White;
            else
                Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine(procName + ":" + msg);

            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<MyClass> Objects = ParallelEnumerable.Repeat<MyClass>(new MyClass(), 2);

            PetriController<MyClass> pn = new PetriController<MyClass>();

            pn.SetReset(o => { o.Number = 0; o.Word = "aaa"; });

            pn.SetObjects(Objects.ToList());

            pn.AddTransition(o => true, o => o.Number == 0 && o.Word == "aaa", o => { }, o => { o.Number = 1; o.Word = "bbb"; }, ExecutionMode.Synch, 0);
            pn.AddTransition(o => true, o => o.Number == 1 && o.Word == "bbb", o => { }, o => { o.Number = 2; o.Word = "ccc"; }, ExecutionMode.Synch, 0);
            pn.AddGeneralTransition(o => true, o => o.Number == 2 && o.Word == "ccc", o => { }, o => { o.Number = 3; o.Word = "ddd"; }, ExecutionMode.Synch, 0);

            pn.Run();
        }
    }
}

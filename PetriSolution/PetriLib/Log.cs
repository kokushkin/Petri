using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PetriLib
{
    public enum MessageLevel
    {
        Error = 1,
        Warning = 2,
        Notify = 3,
        Information = 4,
        Debug = 5
    }


    public interface ILog
    {
        void WriteException(Exception ex, string procName);

        void WriteMessage(string msg, MessageLevel level, string procName);

    }


    public static class Log
    {
        public static ILog Logger { get; set; }

        static Log()
        {
            Logger = new SimpleLog();
        }

        public static void WriteException(Exception ex, string procName)
        {
            Logger.WriteException(ex, procName);
        }

        public static void WriteMessage(string msg, MessageLevel level, string procName)
        {
            Logger.WriteMessage(msg, level, procName);
        }
    }

    public class SimpleLog : ILog
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
}

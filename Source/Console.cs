using System;
using System.Diagnostics;

namespace Holo
{
    /// <summary>
    /// Provides interface output related functions, such as logging activities.
    /// </summary>
    public static class Out
    {
        /// <summary>
        /// Enum with flags for log importancies. If 'minimumImportance' flag is higher than the action to be logged, then the action won't be logged.
        /// </summary>
        public enum logFlags{ImportantAction = 3, StandardAction = 2, BelowStandardAction = 1, MehAction = 0}
        /// <summary>
        /// Flag for minimum importance in logs. Adjust this to don't print less important logs.
        /// </summary>
        public static logFlags minimumImportance;
        private static bool bwait = false;
        /// <summary>
        /// Prints a green line of log, together with timestamp and method name.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        public static void wait()
    {
        while(bwait) {}

    }
        public static void WriteLine(string logText)
        {
            wait();
            bwait = true;
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);
            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
            bwait = false;
        }
        /// <summary>
        /// Prints a green line of log, together with timestamp and method name.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        /// <param name="logFlag">The importance flag of this log line.</param>
        public static void WriteLine(string logText, logFlags logFlag)
        {
            
            if ((int)logFlag < (int)minimumImportance)
                return;
            wait();
            bwait = true;
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);
            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
            bwait = false;
        }
        /// <summary>
        /// Prints a customizeable line of log, together with timestamp and method name.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        /// <param name="logFlag">The importance flag of this log line.</param>
        /// <param name="colorOne">The color to use on the left.</param>
        /// <param name="colorTwo">The color to use on the right.</param>
        public static void WriteLine(string logText, logFlags logFlag, ConsoleColor colorOne, ConsoleColor colorTwo)
        {
            
            if ((int)logFlag < (int)minimumImportance)
                return;
            wait();
            bwait = true;
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);
            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = colorOne;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = colorTwo;
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
            bwait = false;
        }
        public static void WriteTrace(string logText)
        {
            return;
        }
        /// <summary>
        /// Prints a red, error line of log, together with timestamp and method name.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        public static void WriteError(string logText)
        {
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);

            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// Prints a red,error line of log, together with timestamp and method name.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        /// <param name="logFlag">The importance flag of this error.</param>
        public static void WriteError(string logText, logFlags logFlag)
        {
            if ((int)logFlag < (int)minimumImportance)
                return;

            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);

            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// Writes a plain text line.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        public static void WritePlain(string logText)
        {
            wait();
            bwait = true;
            Console.WriteLine(logText);
            bwait = false;
        }
        /// <summary>
        /// Writes a blank line.
        /// </summary>
        public static void WriteBlank()
        {
            wait();
            bwait = true;
            Console.WriteLine();
            bwait = false;
        }
        /// <summary>
        /// Writes a special line of log, with customizeable colors and header coloring of logText.
        /// </summary>
        /// <param name="logText">The log line to be printed.</param>
        /// <param name="logFlag">The importance flag of this log line.</param>
        /// <param name="colorOne">The color to use on the left.</param>
        /// <param name="colorTwo">The color to use on the right.</param>
        /// <param name="headerHead">The string to use infront of logText.</param>
        /// <param name="headerLength">The length of the header to color.</param>
        /// <param name="headerColor">The color for the header in the logText.</param>
        public static void WriteSpecialLine(string logText, logFlags logFlag, ConsoleColor colorOne, ConsoleColor colorTwo, string headerHead, int headerLength, ConsoleColor headerColor)
        {
            wait();
            bwait = true;
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);
            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = colorOne;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] " + headerHead + " ");
            Console.ForegroundColor = headerColor;
            try { Console.Write(logText.Substring(0, headerLength)); }
            catch { Console.Write(logText); }
            Console.ForegroundColor = colorTwo;
            Console.WriteLine(logText.Substring(headerLength));
            Console.ForegroundColor = ConsoleColor.Gray;
            bwait = false;
        }

        public static void WriteDCError(string logText)
        {

            try
            {
                System.IO.FileStream Writer = new System.IO.FileStream("DC.err", System.IO.FileMode.Append, System.IO.FileAccess.Write);
                byte[] Msg = System.Text.ASCIIEncoding.ASCII.GetBytes(logText + "\r\n\r\n");
                Writer.Write(Msg, 0, Msg.Length);
            }
            catch (Exception eX) { Out.WritePlain(eX.Message); }
            wait();
            bwait = true;
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(1);

            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Look in DC.err for the disconnection error.");
            Console.ForegroundColor = ConsoleColor.Gray;
            bwait = false;
        }
     }
}

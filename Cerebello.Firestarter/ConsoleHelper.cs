using System;
using System.Text.RegularExpressions;

namespace Cerebello.Firestarter
{
    class ConsoleHelper
    {
        public static void PressAnyKeyToContinue()
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to continue.");
            Console.ForegroundColor = Console.BackgroundColor;
            Console.ReadKey(true);
            Console.WriteLine();
            Console.ForegroundColor = prevColor;
        }

        public static bool YesNo(string question, bool isNoSafe = true)
        {
            var color = Console.ForegroundColor;
            try
            {
                for (int tries = 0; tries < 10; tries++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(question);
                    Console.Write(" (y/n): ");
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.ForegroundColor = isNoSafe ? ConsoleColor.Red : ConsoleColor.Green;
                        Console.WriteLine(isNoSafe ? "n (esc)" : "y (esc)");
                        return !isNoSafe;
                    }
                    var ch = key.KeyChar;
                    if (ch == 'y' || ch == 'Y')
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("y");
                        return true;
                    }
                    if (ch == 'n' || ch == 'N')
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("n");
                        return false;
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("invalid");
                }

                Console.ForegroundColor = isNoSafe ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine(isNoSafe ? "n (invalid)" : "y (invalid)");
                return !isNoSafe;
            }
            finally
            {
                Console.ForegroundColor = color;
            }
        }

        public static void ConsoleWriteProgressIntInt(int x, int y)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (x == 0 || x == y || x * 10 / y != (x - 1) * 10 / y)
                Console.WriteLine("    {0} of {1} - {2}%", x, y, x * 100 / y);
            Console.ForegroundColor = oldColor;
        }

        public static void ConsoleWriteException(Exception ex)
        {
            var color = Console.ForegroundColor;
            var backColor = Console.BackgroundColor;

            try
            {
                var ex1 = ex;

                while (ex1 != null)
                {
                    // exception type
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Exception: {0}", ex1.GetType().Name);
                    Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));

                    // message
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.Red;
                    var lines = Regex.Split(ex1.Message, @"\r\n|\r|\n");
                    foreach (var eachLine in lines)
                    {
                        Console.Write(eachLine);
                        if (Console.CursorLeft > 0)
                            Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                    }

                    // stack-trace
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    int fncLevel = 0;
                    if (ex1.StackTrace != null)
                    {
                        var linesStackTrace = Regex.Split(ex1.StackTrace, @"\r\n|\r|\n");
                        foreach (var eachLine in linesStackTrace)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("{0:0000}:", fncLevel++);
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(eachLine);
                            if (Console.CursorLeft > 0)
                                Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                        }
                    }

                    // inner exception
                    ex1 = ex1.InnerException;
                    if (ex1 != null)
                    {
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write("==== inner exception ====");
                        if (Console.CursorLeft > 0)
                            Console.Write(new string(' ', Console.BufferWidth - Console.CursorLeft));
                    }
                }
            }
            finally
            {
                Console.BackgroundColor = backColor;
                Console.ForegroundColor = color;
            }
        }
    }
}

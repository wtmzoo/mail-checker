using System;

namespace MailChecker.Resources
{
    public class ColoredWrite
    {
        public static void Green(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(text);
            Console.ResetColor();
        }
        public static void Red(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void DarkYellow(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(text);
            Console.ResetColor();
        }
    }
    
    public class ColoredWriteLine
    {
        public static void Green(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        public static void Red(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void DarkYellow(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
using System;

namespace MAD.Project
{
    public static class ColorConsole
    {
        public static void WriteLine(ConsoleColor color, string s)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ForegroundColor = original;
        }
    }
}

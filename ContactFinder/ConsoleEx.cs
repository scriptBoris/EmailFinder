using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactFinderLib
{
    public static class ConsoleEx
    {
        /// <summary>
        /// Пишет в консоль строку, с простым форматированием
        /// </summary>
        /// <param name="head">белый цвет</param>
        /// <param name="value">красный</param>
        public static void WriteError(string head, string value)
        {
            Console.ResetColor();
            Console.Write(head + " ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        /// <summary>
        /// Пишет в консоль строку, с простым форматированием
        /// </summary>
        /// <param name="head">белый цвет</param>
        /// <param name="value">красный</param>
        public static void WriteEmail(string head, string value)
        {
            Console.ResetColor();
            Console.Write(head + " ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(value);
            Console.ResetColor();
        }
    }
}

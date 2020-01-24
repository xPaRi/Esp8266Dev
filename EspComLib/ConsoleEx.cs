using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspComLib
{
    /// <summary>
    /// Wrapper na konzoli.
    /// </summary>
    public static class ConsoleEx
    {

        /// <summary>
        /// Vypíše text.
        /// </summary>
        /// <param name="text">Text</param>
        public static void WriteLine(string text)
        {
            Console.WriteLine(text);    
        }

        /// <summary>
        /// Zobrazí text s možností zadat jeho barvu.
        /// </summary>
        /// <param name="foreColor">Brava textu.</param>
        /// <param name="text">Text.</param>
        public static void WriteLine(ConsoleColor foreColor, string text)
        {
            Console.ForegroundColor = foreColor;
            Console.WriteLine(text);

            Console.ResetColor();
        }

        /// <summary>
        /// Zobrazí text chyby v příslušné barvě.
        /// </summary>
        /// <param name="text"></param>
        public static void WriteError(string text)
        {
            WriteLine(ConsoleColor.Red, "ERROR: " + text);
        }


        /// <summary>
        /// Zobrazí text chyby v příslušné barvě.
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteError(Exception ex)
        {
            WriteError(ex.Messages());
        }

        /// <summary>
        /// Zobrazí text s možností zadat jeho barvu a barvu jeho pozadí.
        /// </summary>
        /// <param name="foreColor">Brava textu.</param>
        /// <param name="backgroundColor">Barva pozadí.</param>
        /// <param name="text">Text.</param>
        public static void WriteLine(ConsoleColor foreColor, ConsoleColor backgroundColor, string text)
        {
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backgroundColor;

            Console.WriteLine(text);

            Console.ResetColor();
        }

        /// <summary>
        /// Zobrazí záhlaví programu a nastaví záhlaví okna.
        /// </summary>
        public static void WriteAppCaption()
        {
            Console.Title = Helpers.GetAssemblyName();

            ConsoleEx.WriteLine( Console.BackgroundColor, Console.ForegroundColor
                , $" {Helpers.GetAssemblyName()} - version {Helpers.GetAssemblyVersion()}".PadRight(Console.BufferWidth, ' '));
        }

        /// <summary>
        /// Vypíše nový řádek.
        /// </summary>
        public static void NewLine()
        {
            Console.WriteLine();
        }

        public static void WriteLineAt(int left, int top, ConsoleColor foreColor, ConsoleColor backgroundColor, string text)
        {
            //--- Positioning
            var bakLeft = Console.CursorLeft;
            var bakTop = Console.CursorTop;
            Console.SetCursorPosition(left, top);
            //---

            WriteLine(foreColor, backgroundColor, text);

            Console.SetCursorPosition(bakLeft, bakTop);
        }

        public static void WriteLineAt(int left, int top, string text)
        {
            //--- Positioning
            var bakLeft = Console.CursorLeft;
            var bakTop = Console.CursorTop;
            Console.SetCursorPosition(left, top);
            //---

            WriteLine(text);

            Console.SetCursorPosition(bakLeft, bakTop);
        }

    }
}

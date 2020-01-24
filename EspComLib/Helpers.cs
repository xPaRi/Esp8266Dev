using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EspComLib
{
    public static class Helpers
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        int uFlags);

        private const int HWND_TOPMOST = -1;
        private const int HWND_NOTOPMOST = -2;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;

        /// <summary>
        /// Nastaví pro konzolu příznak OnTop
        /// </summary>
        public static void ConsoleOnTop()
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;

            SetWindowPos(hWnd, new IntPtr(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        /// <summary>
        /// Nastaví pro konzolu příznak OnTop
        /// </summary>
        public static void ConsoleOnNormal()
        {
            var hWnd = Process.GetCurrentProcess().MainWindowHandle;

            SetWindowPos(hWnd, new IntPtr(HWND_NOTOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        /// <summary>
        /// Objekt pro blokování čtecího threadu
        /// </summary>
        public static object Locker = new object();

        /// <summary>
        /// Název pipe založený na názvu komunikačního portu.
        /// </summary>
        /// <param name="communicationPort"></param>
        /// <returns></returns>
        public static string GetPipeName(string communicationPort) => $"ESP_PIPE_{communicationPort.ToUpper()}";

        /// <summary>
        /// Projde strom (innerMessage) popisu vyjímek a sestaví z nich řetězec vhodný 
        /// k zobrazení v MessageBoxu.
        /// </summary>
        /// <param name="ex">Kořenová vyjímka.</param>
        /// <returns>Popis vyjímek.</returns>
        public static string Messages(this Exception ex)
        {
            return Messages(ex, null);
        }

        /// <summary>
        /// Projde strom (innerMessage) popisu vyjímek a sestaví z nich řetězec vhodný 
        /// k zobrazení v MessageBoxu.
        /// </summary>
        /// <param name="ex">Kořenová vyjímka.</param>
        /// <param name="text">Úvodní text.</param>
        /// <returns>Popis vyjímek.</returns>
        public static string Messages(this Exception ex, string text)
        {
            while (ex != null)
            {
                text += (String.IsNullOrEmpty(text) ? String.Empty : "\n- ") + ex.Message;
                ex = ex.InnerException;
            }

            return text;
        }

        /// <summary>
        /// rozseká bytové pole na části velikosti bufferLength
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public static IEnumerable<byte[]> Split(this byte[] value, int bufferLength)
        {
            var countOfArray = value.Length / bufferLength;

            if (value.Length % bufferLength > 0)
                countOfArray++;

            for (var i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength).ToArray();
            }
        }

        /// <summary>
        /// Zobrací instalované komunikační porty.
        /// </summary>
        public static string GetValidPorts()
        {
            return string.Join(", ", SerialPort.GetPortNames().OrderBy(it=>it));
        }

        /// <summary>
        /// Formátovaná informace o verzi assembly spustitelného souboru.
        /// </summary>
        /// <returns>EntryAssembly.GetName().Version</returns>
        public static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        /// <summary>
        /// Název spuštěného assembly.
        /// </summary>
        /// <remarks>
        /// Používá se například pro konstrukci názvu helpu nebo
        /// určení umístění informací o programu na internetu.
        /// </remarks>
        /// <returns>EntryAssembly.AssemblyTitleAttribute</returns>
        public static string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        public static void SendCmd(SerialPort serialPort, int sendedLines, string text, bool showRecText = true)
        {
            SendToEsp(serialPort, sendedLines, text, showRecText, false);
        }

        public static void SendLua(SerialPort serialPort, int sendedLines, string text)
        {
            SendToEsp(serialPort, sendedLines, $"w([==[{text}]==]);", false, true);
        }

        private static void SendToEsp(SerialPort serialPort, int sendedLines, string sndText, bool showRecText, bool showDot)
        {
            serialPort.WriteLine(sndText);

            //odpověď z ESP
            var recText = serialPort.ReadLine().TrimStart('>', ' ').TrimEnd('\r');

            //--- Kontrola, zda to, co jsme poslali odpovídá tomu, co jsme přijali
            if (!sndText.Equals(recText))
            {
                var colorBak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine();
                Console.WriteLine($"---[ ERROR in line {sendedLines} ]---");
                Console.WriteLine($"   sended> {sndText}");
                Console.WriteLine($" received> {recText}");

                Console.ForegroundColor = colorBak;
            }
            else if (showRecText)
            {
                Console.WriteLine(recText);
            }
            else if (showDot)
            {
                Console.Write(".");
            }
            //---
        }

        public static List<string> SplitQuotationParameters(this string value)
        {
            const string RE = @"\G(""((""""|[^""])+)""|(\S+)) *";

            return Regex.Matches(value, RE).Cast<Match>().Select(m => Regex.Replace(m.Groups[2].Success? m.Groups[2].Value: m.Groups[4].Value, @"""""", @"""")).ToList();
        }

        /// <summary>
        /// Očekává seznam portů oddělených oddělovačem.
        /// Oddělovač je primárně mezera, čárka, středník.
        /// Postupně je vyzkouší a název prvního validního vrátí.
        /// </summary>
        /// <param name="value">Seznam portů z příkazové řádky nebo z konfiguračního souboru</param>
        /// <returns></returns>
        public static string GetValidPortName(string value)
        {
            var portList = (value.ToUpper()).Split(new[] {' ', ';', ','}, StringSplitOptions.RemoveEmptyEntries); //seznam portů z příkazové řádky nebo z konfiguračního souboru
            var validPortList = SerialPort.GetPortNames(); //seznam instalovaných portů

            foreach (var portName in portList.Where(it => validPortList.Contains(it)).OrderBy(it=>it))
            {
                try
                {
                    using (var serialPort = new SerialPort(portName))
                    {
                        serialPort.Open();
                        serialPort.Close();
                    }

                    return portName;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Messages());
                }
            }

            return null;
        }
    }
}

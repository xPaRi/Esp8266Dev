using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EspComDirect
{
    internal static class Helpers
    {
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
                text += (string.IsNullOrEmpty(text) ? string.Empty : "\n- ") + ex.Message;
                ex = ex.InnerException;
            }

            return text;
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Invalid parameters!");
            Console.WriteLine();
            Console.WriteLine("Usage: EspComDirect.exe [port] <file>");
            Console.WriteLine();
            Console.WriteLine("       [port]  Communication port identifier (see config file)");
            Console.WriteLine("       <file>  Filename to upload");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Example: EspComDirect COM9 \"c:\\temp\\test.lua\"");
            Console.WriteLine();

            var serialPort = new SerialPort();

            try
            {
                serialPort = GetSerialPort(Properties.Settings.Default.PortName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Messages());
                Console.WriteLine("Correct or delete configuration file.");
            }

            Console.WriteLine("Serial port settings");
            Console.WriteLine();
            Console.WriteLine("    Parameters     Current       Default        .NET type (see msdn)");
            Console.WriteLine("    ------------  -------------  -------------  -----------------------------");
            Console.WriteLine($"    PortName      {serialPort.PortName}\t\t COM1");
            Console.WriteLine($"    BaudRate      {serialPort.BaudRate}\t 115200");
            Console.WriteLine($"    Parity        {serialPort.Parity}\t\t None           System.IO.Ports.Parity");
            Console.WriteLine($"    DataBits      {serialPort.DataBits}\t\t 8");
            Console.WriteLine($"    StopBits      {serialPort.StopBits}\t\t One            System.IO.Ports.StopBits");
            Console.WriteLine($"    Handshake     {serialPort.Handshake}\t\t None           System.IO.Ports.Handshake");
            Console.WriteLine($"    ReadTimeout   {serialPort.ReadTimeout}\t\t 1000");
            Console.WriteLine($"    WriteTimeout  {serialPort.WriteTimeout}\t\t 1000");
            Console.WriteLine();

            Console.WriteLine($"Valid ports: {Helpers.GetValidPorts()}");
            Console.WriteLine("Configuration file: EspComDirect.exe.config");
        }


        public static SerialPort GetSerialPort(string portName)
        {
            return new SerialPort
            {
                PortName = portName,
                BaudRate = Properties.Settings.Default.BaudRate,
                Parity = Properties.Settings.Default.Parity,
                DataBits = Properties.Settings.Default.DataBits,
                StopBits = Properties.Settings.Default.StopBits,
                Handshake = Properties.Settings.Default.Handshake,
                ReadTimeout = Properties.Settings.Default.ReadTimeout,
                WriteTimeout = Properties.Settings.Default.WriteTimeout
            };
        }

        /// <summary>
        /// Zobrací instalované komunikační porty.
        /// </summary>
        public static string GetValidPorts()
        {
            return string.Join(", ", SerialPort.GetPortNames());
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
    }
}

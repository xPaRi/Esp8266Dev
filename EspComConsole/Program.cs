using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EspComLib;

namespace EspComConsole
{
    /// <summary>
    /// Program konzole pro Esp8266.
    /// </summary>
    /// <remarks>
    /// Parametry:
    ///     /pipe:FIRST (default)
    ///     /ports:COM1,COM3,COM8 (defaultně vezme první v řadě)
    /// 
    /// Spuštění bez parametrů: dá vybrat z volných portů
    /// </remarks>
    internal class Program
    {
        private const string PAR_PIPE_NAME = "/PIPE";
        private const string PAR_PORT_LIST = "/PORTS";
        private const string PAR_HELP_1 = "/?";
        private const string PAR_HELP_2 = "/h";
        private const string PAR_HELP_3 = "/help";

        private static void Main(string[] args)
        {

            ConsoleEx.WriteAppCaption();
            ConsoleEx.WriteLine($"Valid port(s): {Helpers.GetValidPorts()}");

            var cmdLine = new MyCommandLine(args);

            if (cmdLine.ExistsAny(new[] {PAR_HELP_1, PAR_HELP_2, PAR_HELP_3}))
            {
                ShowHelp();
                return;
            }

            try
            {
                var comCom = new ComComunication(cmdLine.Value(PAR_PORT_LIST));

                Console.WriteLine($"Selected port: {comCom.PortName}");

                var comComThread = comCom.StartCommunication();
                var pipeName = cmdLine.Exists(PAR_PIPE_NAME) ? cmdLine.Value(PAR_PIPE_NAME) : Helpers.GetPipeName(comCom.PortName);

                var pipeServer = new PipeServer(pipeName, comCom.SerialPort);
                var pipeServerThread = pipeServer.Start();

                comComThread?.Join();
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteError(ex);
            }
        }

        private static void Test()
        {
            Show("a b c d".SplitQuotationParameters());
            Show("\"a\" b \"c\" d".SplitQuotationParameters());
        }

        private static void Show(IEnumerable<string> value)
        {
            foreach (var item in value)
            {
                Console.WriteLine(item);
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine($"EspComConsole [/PIPE:<pipeName>] [/PORTS:<port list>] [/?|/h|/help]");
            Console.WriteLine();
            Console.WriteLine($" Parameters");
            Console.WriteLine($"    {PAR_PIPE_NAME}   name of communication pipe (FIRST or SECOND or COM_PIPE_1 etc...)");
            Console.WriteLine($"    {PAR_PORT_LIST}  list of possible communication port, order is important");
            Console.WriteLine();
            Console.WriteLine($" Switch");
            Console.WriteLine($"    {PAR_HELP_1}      this help");
            Console.WriteLine($"    {PAR_HELP_2}      this help");
            Console.WriteLine($"    {PAR_HELP_3}   this help");
            Console.WriteLine();
            Console.WriteLine($" Examples");
            Console.WriteLine($"    EspComConsole {PAR_PIPE_NAME}:FIRST {PAR_PORT_LIST}:COM1");
            Console.WriteLine($"    EspComConsole {PAR_PIPE_NAME}:SECOND {PAR_PORT_LIST}:COM3,COM2,COM8");
            Console.WriteLine();
            
        }
    }
}

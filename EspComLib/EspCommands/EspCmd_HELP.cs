using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_HELP : EspCommand
    {
        public override string Code => "help";
        private readonly EspCommandList _EspCommandList;

        public EspCmd_HELP(EspCommandList espCommandList)
        {
            _EspCommandList = espCommandList;
        }

        public override void Execute(SerialPort serialPort, string argument)
        {
            ConsoleEx.WriteLine(ConsoleColor.White, " Help of EspComConsole\n");

            foreach (var espCommand in _EspCommandList.Values.OrderBy(it=>it.Code))
            {
                ConsoleEx.WriteLine(ConsoleColor.White, $"{espCommand.Code.PadRight(10)} - {espCommand.Description}");
            }
        }


        public override string Description => "This help.";

        public override bool IsMustBeLockReadThread => false;
    }
}
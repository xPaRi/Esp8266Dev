using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_LSCOM : EspCommand
    {
        public override string Code => "lscom";

        public override void Execute(SerialPort serialPort, string argument)
        {
            ConsoleEx.WriteLine(ConsoleColor.White, " List of COM ports\n");

            foreach (var portName in SerialPort.GetPortNames())
            {
                ConsoleEx.WriteLine(ConsoleColor.White, portName);
            }
        }

        public override string Description => "List of communication ports.";

        public override bool IsMustBeLockReadThread => true;
    }
}
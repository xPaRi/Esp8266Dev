using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_CLEAR : EspCommand
    {
        public override string Code => "clear";

        public override void Execute(SerialPort serialPort, string argument)
        {
            Console.Clear();
        }

        public override string Description => "Clears the screen.";

        public override bool IsMustBeLockReadThread => false;
    }
}
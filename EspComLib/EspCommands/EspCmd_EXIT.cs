using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_EXIT : EspCommand
    {
        public override string Code => "exit";

        public override void Execute(SerialPort serialPort, string argument)
        {
            //o tohle se postará ComComunication.ConsoleThread()
        }

        public override string Description => "Quits the EspComConsole.EXE program.";

        public override bool IsMustBeLockReadThread => false;
    }
}
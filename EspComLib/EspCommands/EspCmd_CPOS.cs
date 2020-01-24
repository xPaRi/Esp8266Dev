using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_CPOS : EspCommand
    {
        public override string Code => "cpos";

        public override void Execute(SerialPort serialPort, string argument)
        {
            switch (argument)
            {
                case "0":
                    ConsoleEx.WriteLine("Ok. Console on normal position.");
                    Helpers.ConsoleOnNormal();
                    break;
                case "1":
                    ConsoleEx.WriteLine("Ok. Console is on top position.");
                    Helpers.ConsoleOnTop();
                    break;
                default:
                    ConsoleEx.WriteError("Invalid parameter");
                    break;
            }
        }

        public override string Description => "Console position. (0=normal; 1=top.)";

        public override bool IsMustBeLockReadThread => false;
    }
}
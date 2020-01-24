using System;
using System.IO.Ports;
using System.Linq;

namespace EspComLib
{
    internal class EspCmd_RM : EspCommand
    {
        public override string Code => "rm";

        public override void Execute(SerialPort serialPort, string argument)
        {
            var fileList = argument.SplitQuotationParameters();

            if (!fileList.Any())
            {
                ConsoleEx.WriteLine("No file(s) specified.");
                return;
            }

            foreach (var file in fileList)
            {
                Helpers.SendCmd(serialPort, 0, $"file.remove('{file}')");
            }
        }

        public override string Description => "Remove file. Parameter <filename>";

        public override bool IsMustBeLockReadThread => true;
    }
}
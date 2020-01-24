using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_TYPE : EspCommand
    {
        public override string Code => "type";

        public override void Execute(SerialPort serialPort, string argument)
        {
            var command1 = $"_view = function() local _line if file.open('{argument}','r') then repeat _line = file.readline() if (_line~=nil) then print(string.sub(_line,1,-2)) end until _line==nil file.close() print('<EOF>') else";
            var command2 = "print(\"\\r--FileView error: can't open file\") end end _view() _view = nil";

            serialPort.WriteLine(command1);
            serialPort.ReadLine();

            serialPort.WriteLine(command2);
            serialPort.ReadLine();
        }

        public override string Description => "Displays file on screen. Parameter <filename>";

        public override bool IsMustBeLockReadThread => true;
    }
}
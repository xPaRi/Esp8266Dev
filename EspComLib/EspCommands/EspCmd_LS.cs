using System;
using System.IO.Ports;
using System.Threading;

namespace EspComLib
{
    internal class EspCmd_LS : EspCommand
    {
        public override string Code => "ls";

        public override void Execute(SerialPort serialPort, string argument)
        {
            Helpers.SendCmd(
                serialPort
                , 0
                , "_dir=function() local c,r,u,t,k,v,l=0,file.fsinfo() print(' Directory of ESP\\n') for k,v in pairs(file.list()) do print(string.format('%-17s %6d bytes',k,v)) c=c+1 end print(string.format('\\n%4d File(s) %11d bytes',c,u)) end _dir() _dir=nil"
                , false
                );
        }

        public override string Description => "List of file.";

        public override bool IsMustBeLockReadThread => true;
    }
}
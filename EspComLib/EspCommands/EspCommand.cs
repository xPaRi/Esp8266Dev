using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EspComLib
{
    /// <summary>
    /// Předek všech tříd 'zadrátovaných' příkazů.
    /// </summary>
    public abstract class EspCommand
    {
        /// <summary>
        /// Kód příkazu.
        /// </summary>
        public abstract string Code { get; }

        /// <summary>
        /// Metoda spouštějící příkaz.
        /// </summary>
        /// <param name="serialPort">Sériový port, na který se příkaz pošle.</param>
        /// <param name="argument"></param>
        public abstract void Execute(SerialPort serialPort, string argument);

        /// <summary>
        /// Zobrazí nápovědu k příkazu.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Indikuje, zda je před spuštění příkazu nutné zablokovat čtecí thread.
        /// </summary>
        public abstract bool IsMustBeLockReadThread { get; }
    }

    /// <summary>
    /// Seznam příkazů.
    /// </summary>
    public class EspCommandList : Dictionary<string, EspCommand>
    {
        private readonly SerialPort _SerialPort;

        public EspCommandList(SerialPort serialPort)
        {
            Add(new EspCmd_LS());
            Add(new EspCmd_LSCOM());
            Add(new EspCmd_RM());
            Add(new EspCmd_UPLOAD());
            Add(new EspCmd_HELP(this));
            Add(new EspCmd_CLEAR());
            Add(new EspCmd_EXIT());
            Add(new EspCmd_TYPE());
            Add(new EspCmd_CPOS());

            _SerialPort = serialPort;
        }

        private void Add(EspCommand espCommand)
        {
            Add(espCommand.Code, espCommand);
        }

        /// <summary>
        /// Na základě příkazové řádky provede text příslušného příkazu.
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <returns></returns>
        public void Execute(string cmdLine)
        {
            var aCmdLine = cmdLine.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var command = aCmdLine.Length >= 1 ? aCmdLine[0] : string.Empty;
            var argument = aCmdLine.Length > 1 ? aCmdLine[1] : string.Empty;

            EspCommand espCommand;

            //--- Pokud nalezneme interní příkaz, tak jej analyzujeme a provedeme
            if (TryGetValue(command, out espCommand))
            {
                if (espCommand.IsMustBeLockReadThread)
                {
                    lock (Helpers.Locker)
                    {
                        espCommand.Execute(_SerialPort, argument);
                    }
                }
                else
                {
                    espCommand.Execute(_SerialPort, argument);
                }

                return;
            }
            //---

            //--- Pokud je potvrzen jen prázdý řádek, pošleme <CR>
            //Umožňuje se tím přerušit běh některých 'init.lua'
            if (string.IsNullOrEmpty(command))
            {
                cmdLine = "\r";
            }
            //---

            _SerialPort.WriteLine(cmdLine); //v opačném případě jej pošleme rovnou do Esp
        }
    }
}

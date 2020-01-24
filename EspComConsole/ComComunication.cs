using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EspComLib;

namespace EspComConsole
{
    internal class ComComunication
    {
        private readonly SerialPort _SerialPort;
        private readonly StringComparer _StringComparer = StringComparer.OrdinalIgnoreCase;

        public ComComunication(string portName = null)
        {
            var validPortName = Helpers.GetValidPortName(string.IsNullOrEmpty(portName) ? Properties.Settings.Default.PortName : portName);

            if (validPortName == null)
                throw(new Exception("Any COM Device does not exists."));

            _SerialPort = new SerialPort
            {
                PortName = validPortName,
                BaudRate = Properties.Settings.Default.BaudRate,
                Parity = Properties.Settings.Default.Parity,
                DataBits = Properties.Settings.Default.DataBits,
                StopBits = Properties.Settings.Default.StopBits,
                Handshake = Properties.Settings.Default.Handshake,
                ReadTimeout = Properties.Settings.Default.ReadTimeout,
                WriteTimeout = Properties.Settings.Default.WriteTimeout,
                Encoding = Encoding.GetEncoding(Properties.Settings.Default.SerialPortEncoding)
            };
        }

        public SerialPort SerialPort => _SerialPort;

        public string PortName => _SerialPort.PortName;

        public Thread StartCommunication()
        {
            lock (Helpers.Locker)
            {
                TryOpenPort();
            }

            var readThread = new Thread(ReadThread) {IsBackground = true};
            readThread.Start();

            var consoleThread = new Thread(ConsoleThread) { IsBackground = true };
            consoleThread.SetApartmentState(ApartmentState.STA);
            consoleThread.Start();

            return consoleThread;
        }

        /// <summary>
        /// Obstarává čtení ze sériového portu.
        /// </summary>
        private void ReadThread()
        {
            while (true)
            {
                try
                {
                    lock (Helpers.Locker)
                    {
                        var text = _SerialPort.ReadLine();
                        ConsoleEx.WriteLine(ConsoleColor.White, text);
                    }
                }
                catch (TimeoutException)
                {
                }
                catch (InvalidOperationException)
                {
                    TryOpenPort();
                }
                catch (IOException)
                {
                    TryOpenPort();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Messages());
                }
            }
        }

        /// <summary>
        /// Pokouší se cyklicky připojit k zadanému portu.
        /// Pokud uspěje, tak return.
        /// </summary>
        private void TryOpenPort()
        {
            ConsoleEx.NewLine();

            var left = 0;
            var top = Console.CursorTop;
            var text = $" Open {_SerialPort.PortName} ...";
            var space = string.Empty.PadRight(text.Length);
            var flag = true;

            while (true)
            {
                var message = flag ? text : space;
                ConsoleEx.WriteLineAt(left, top, ConsoleColor.Yellow, ConsoleColor.Red, $"{message}");

                try
                {
                    lock (Helpers.Locker)
                    {
                        _SerialPort.Open();
                        ConsoleEx.WriteLineAt(left, top, $"{space}");
                        return;
                    }
                }
                catch (Exception)
                {
                    //jen odchyt chyby
                    Thread.Sleep(200);
                }

                flag = !flag;
            }
        }

        /// <summary>
        /// Obstarává zápis z konzole do sériového portu.
        /// </summary>
        private void ConsoleThread()
        {
            var espCommandList = new EspCommandList(_SerialPort);

            while (true)
            {
                var message = Console.ReadLine();

                if (_StringComparer.Equals("exit", message))
                    break;

                try
                {
                    espCommandList.Execute(message);
                }
                catch (Exception ex)
                {
                    ConsoleEx.WriteError(ex);
                }
            }
        }
    }
}

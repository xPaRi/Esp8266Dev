using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EspComLib;
using Newtonsoft.Json;

namespace EspComConsole
{
    public class PipeServer
    {
        private readonly string _PipeName;
        private readonly SerialPort _SerialPort;

        private int _SendedLines = 0;

        public PipeServer(string pipeName, SerialPort serialPort)
        {
            _PipeName = pipeName;
            _SerialPort = serialPort;
        }

        public Thread Start()
        {
            var pipeServerThread = new Thread(StartPipeServer)
            {
                IsBackground = true 
                , Name = _PipeName + " server"
            
            };
            pipeServerThread.Start();

            return pipeServerThread;
        }

        private void StartPipeServer()
        {
            while (true)
            {
                try
                {
                    using (var pipeServer = new NamedPipeServerStream(_PipeName, PipeDirection.InOut, 1))
                    {
                        ConsoleEx.WriteLine($"Pipe server '{_PipeName}' waiting for connection.");

                        pipeServer.WaitForConnection(); // Wait for a client to connect

                        Console.WriteLine("Pipe client connected.");

                        lock (Helpers.Locker)
                        {
                            try
                            {
                                var streamString = new StreamString(pipeServer);

                                var msg = JsonConvert.DeserializeObject<MessageFromClient>(streamString.ReadString());

                                streamString.WriteString("Ok");

                                Console.WriteLine($"Command: {msg.Command}");

                                switch (msg.Command)
                                {
                                    case "UPLOAD":
                                        foreach (var parameter in msg.Parameters)
                                        {
                                            Upload(streamString, _SerialPort, parameter);
                                        }
                                        break;
                                    case "DOFILE":
                                        foreach (var parameter in msg.Parameters)
                                        {
                                            var file = Path.GetFileName(parameter);
                                            SendCmd(_SerialPort, $"dofile('{file}')");
                                        }
                                        break;
                                    case "CMD":
                                        foreach (var parameter in msg.Parameters)
                                        {
                                            SendCmd(_SerialPort, parameter);
                                        }
                                        break;
                                }


                            }
                            catch (IOException ex)
                            {
                                ConsoleEx.WriteError(ex);
                            }
                            catch(Exception ex)
                            {
                                ConsoleEx.WriteError(ex);
                            }
                        }

                        pipeServer.Close();
                    }
                }
                catch (IOException ex)
                {
                    ConsoleEx.WriteError(ex);
                }
            }
        }

        private void Upload(StreamString streamString, SerialPort serialPort, string fileName)
        {
            ConsoleEx.WriteLine(ConsoleColor.Yellow, $"UPLOAD ¨\"{fileName}\"");

            try
            {
                //--- Načteme řádky programu
                var lines = File.ReadAllLines(fileName);
                var file = Path.GetFileName(fileName);
                //---

                ConsoleEx.WriteLine("---[ START ]---");
                SendCmd(serialPort, $"file.remove('{file}')");
                SendCmd(serialPort, $"file.open('{file}', 'w+')");
                SendCmd(serialPort, "w = file.writeline");

                foreach (var line in lines)
                {
                    streamString.WriteString(line);

                    SendLua(serialPort, line);
                }

                ConsoleEx.NewLine();
                SendCmd(serialPort, "file.close()");
                ConsoleEx.WriteLine("---[ END ]---");
                ConsoleEx.WriteLine($"{_SendedLines} line(s) sended");
            }
            catch (DirectoryNotFoundException ex)
            {
                ConsoleEx.NewLine();
                ConsoleEx.WriteError(ex);
            }
            catch (Exception ex)
            {
                ConsoleEx.NewLine();
                ConsoleEx.WriteError(_SendedLines <= 0 ? $"---[ ERROR ]---" : $"---[ ERROR in line {_SendedLines} ]---");
                ConsoleEx.WriteError(ex);

                ConsoleEx.WriteLine("Do you have correct the PortName?");
            }
            finally
            {
                ConsoleEx.NewLine();
            }
        }


        private void SendCmd(SerialPort serialPort, string text)
        {
            SendToEsp(serialPort, text, true);
        }

        private void SendLua(SerialPort serialPort, string text)
        {
            _SendedLines++;
            SendToEsp(serialPort, $"w([==[{text}]==]);", false);
        }

        private void SendToEsp(SerialPort serialPort, string sndText, bool showRecText)
        {
            serialPort.WriteLine(sndText);

            //odpověď z ESP
            var recText = serialPort.ReadLine().TrimStart('>', ' ').TrimEnd('\r');

            //--- Kontrola, zda to, co jsme poslali odpovídá tomu, co jsme přijali
            if (!sndText.Equals(recText))
            {
                var colorBak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                ConsoleEx.NewLine();
                ConsoleEx.WriteError($"---[ ERROR in line {_SendedLines} ]---");
                ConsoleEx.WriteError($"   sended> {sndText}");
                ConsoleEx.WriteError($" received> {recText}");

                Console.ForegroundColor = colorBak;
            }
            else if (showRecText)
            {
                ConsoleEx.WriteLine(recText);
            }
            else
            {
                Console.Write(".");
            }
            //---
        }


    }
}

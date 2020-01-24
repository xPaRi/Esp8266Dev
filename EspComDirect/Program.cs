using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

namespace EspComDirect
{
    internal class Program
    {
        //Počet odeslaných řádků
        private static int _SendedLines = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine(Helpers.GetAssemblyName() + " " + Helpers.GetAssemblyVersion());

            //--- Příprava
            string portName;
            string fileName;

            switch (args.Length)
            {
                case 1:
                    portName = Properties.Settings.Default.PortName;
                    fileName = args[0];
                    break;
                case 2:
                    portName = args[0];
                    fileName = args[1];
                    break;
                default:
                    Helpers.ShowHelp();
                    return;
            }
            //---

            //--- Připojíme COM port a pošleme na něj data
            using (var serialPort = Helpers.GetSerialPort(portName))
            {
                try
                {
                    //--- Načteme řádky programu
                    var lines = File.ReadAllLines(fileName);
                    var file = Path.GetFileName(fileName);
                    //---

                    serialPort.Open();
                    Console.WriteLine($"Port {portName} opened.");
                    Console.WriteLine();

                    Console.WriteLine("---[ START ]---");
                    SendCmd(serialPort, $"file.remove('{file}')");
                    SendCmd(serialPort, $"file.open('{file}', 'w+')");
                    SendCmd(serialPort, "w = file.writeline");

                    foreach (var line in lines)
                    {
                        SendLua(serialPort, line);
                    }

                    Console.WriteLine();
                    SendCmd(serialPort, "file.close()");
                }
                catch (Exception ex)
                {
                    var colorBak = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine();
                    Console.WriteLine(_SendedLines <= 0 ? $"---[ ERROR ]---" : $"---[ ERROR in line {_SendedLines} ]---");
                    Console.WriteLine(ex.Messages());

                    if (ex is TimeoutException || ex is IOException)
                    {
                        Console.WriteLine("Do you have correct the PortName?");
                        Console.WriteLine($"Valid ports: {Helpers.GetValidPorts()}");
                    }

                    Console.ForegroundColor = colorBak;
                }
                finally
                {
                    Console.WriteLine("---[ END ]---");
                    Console.WriteLine($"{_SendedLines} line(s) sended");
                }
            }
            //---
        }

        private static void SendCmd(SerialPort serialPort, string text)
        {
            SendToEsp(serialPort, text, true);
        }

        private static void SendLua(SerialPort serialPort, string text)
        {
            _SendedLines++;
            SendToEsp(serialPort, $"w([==[{text}]==]);", false);
        }

        private static void SendToEsp(SerialPort serialPort, string sndText, bool showRecText)
        {
            serialPort.WriteLine(sndText);

            //odpověď z ESP
            var recText = serialPort.ReadLine().TrimStart('>', ' ').TrimEnd('\r'); 

            //--- Kontrola, zda to, co jsme poslali odpovídá tomu, co jsme přijali
            if (!sndText.Equals(recText))
            {
                var colorBak = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine();
                Console.WriteLine($"---[ ERROR in line {_SendedLines} ]---");
                Console.WriteLine($"   sended> {sndText}");
                Console.WriteLine($" received> {recText}");

                Console.ForegroundColor = colorBak;
            }
            else if (showRecText)
            {
                Console.WriteLine(recText);
            }
            else
            {
                Console.Write(".");
            }
            //---
        }

    }
}

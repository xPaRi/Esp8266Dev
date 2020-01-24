using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EspComLib;
using Newtonsoft.Json;

namespace EspComCom
{
    internal class Program
    {
        private const string PAR_PIPE_NAME = "/pipe";
        private const string PAR_HELP_1 = "/?";
        private const string PAR_HELP_2 = "/h";
        private const string PAR_HELP_3 = "/help";
        private const string PAR_CMD_DOFILE = "/dofile";
        private const string PAR_CMD_UPLOAD = "/upload";
        private const string PAR_CMD_COMMAND = "/cmd";
        private const string PAR_CMD_LINE = "/line";

        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            ConsoleEx.WriteAppCaption();

            //--- Analyzujeme příkazovou řádku
            var cmdLine = new MyCommandLine(args);

            //je help?
            if (cmdLine.ExistsAny(new[] { PAR_HELP_1, PAR_HELP_2, PAR_HELP_3 }))
            {
                ShowHelp();
                return;
            }

            var pipeName = cmdLine.Value(PAR_PIPE_NAME);
            if (string.IsNullOrEmpty(pipeName))
            {
                ConsoleEx.WriteLine("Missing name of pipe.");
                ShowHelp();
                return;
            }

            MessageFromClient message = null;

            try
            {
                message = GetMessage(cmdLine);
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteError(ex);
                ShowHelp();
                return;
            }

            if (message == null)
            {
                ConsoleEx.WriteLine("Missing command.");
                ShowHelp();
                return;
            }

            try
            {
                using (var pipeClient = new NamedPipeClientStream(
                    "."
                    , pipeName
                    , PipeDirection.InOut
                    , PipeOptions.None
                    , TokenImpersonationLevel.Impersonation))
                {
                    pipeClient.Connect(500);

                    ConsoleEx.WriteLine($"Connect to pipe '{pipeName}' successed.");

                    var streamString = new StreamString(pipeClient);
                    streamString.WriteString(JsonConvert.SerializeObject(message));

                    do
                    {
                        ConsoleEx.WriteLine(ConsoleColor.White, streamString.ReadString());
                    } while (pipeClient.IsConnected);

                    pipeClient.Close();

                    return;
                }
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteError(ex);
                ConsoleEx.WriteError($"Connect to {pipeName} failed.");
            }
            //---

            Debug.WriteLine($"Connect to {pipeName} failed.");
        }

        /// <summary>
        /// Vrací command na základě příkazové řádky.
        /// </summary>
        /// <param name="cmdLine"></param>
        /// <returns></returns>
        private static MessageFromClient GetMessage(MyCommandLine cmdLine)
        {
            var result = GetCmdByLineMessage(cmdLine); //musí být před GetDoFileMessage(...)
            if (result != null)
                return result;

            result = GetDoFileMessage(cmdLine);
            if (result != null)
                return result;

            result = GetUploadMessage(cmdLine);
            if (result != null)
                return result;

            result = GetCmdMessage(cmdLine);
            if (result != null)
                return result;

            return null;
        }


        private static MessageFromClient GetDoFileMessage(MyCommandLine cmdLine)
        {
            if (!cmdLine.Exists(PAR_CMD_DOFILE))
                return null;

            var fileName = cmdLine.Value(PAR_CMD_DOFILE);

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("File not specified.");
            }

            return new MessageFromClient
            {
                Command = "DOFILE",
                Parameters = new List<string> { fileName }
            };
        }

        private static MessageFromClient GetUploadMessage(MyCommandLine cmdLine)
        {
            if (!cmdLine.Exists(PAR_CMD_UPLOAD))
                return null;

            var fileName = cmdLine.Value(PAR_CMD_UPLOAD);

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("File not specified.");
            }

            return new MessageFromClient
            {
                Command = "UPLOAD",
                Parameters = new List<string> { fileName }
            };
        }

        private static MessageFromClient GetCmdMessage(MyCommandLine cmdLine)
        {
            if (!cmdLine.Exists(PAR_CMD_COMMAND))
                return null;

            var command = cmdLine.Value(PAR_CMD_COMMAND);

            if (string.IsNullOrEmpty(command))
            {
                throw new Exception("Command not specified.");
            }

            return new MessageFromClient
            {
                Command = "CMD",
                Parameters = new List<string> { command }
            };
        }

        private static MessageFromClient GetCmdByLineMessage(MyCommandLine cmdLine)
        {
            if (!cmdLine.ExistsAll(new [] {PAR_CMD_DOFILE, PAR_CMD_LINE}))
                return null;

            var fileName = cmdLine.Value(PAR_CMD_DOFILE);

            if (string.IsNullOrEmpty(fileName))
            {
                throw new Exception("File not specified.");
            }

            var lineIndex = cmdLine.Value(PAR_CMD_LINE, 0) - 1; //řádky jsou v VS Code číslovány od jedničky, převádím je tedy na index

            if (lineIndex < 0)
            {
                throw new Exception("Line not specified.");
            }

            //--- vytáhneme příslušný řádek (očekáváme, že řádky jsou číslovány od jedničky)
            var lines = System.IO.File.ReadAllLines(fileName);

            if (lines.Length < lineIndex)
                throw new Exception($"Line {lineIndex+1} in file {fileName} not exists.");

            var command = lines[lineIndex];
            //---

            return new MessageFromClient
            {
                Command = "CMD",
                Parameters = new List<string> { command }
            };
        }

        private static void ShowHelp()
        {
            ConsoleEx.WriteError("Valid syntax EspComCom.exe </PIPE:pipeName> <[UPLOAD:<fileName>]|[DOFILE:<fileName>]|[CMD:<command>]>");
        }
    }
}

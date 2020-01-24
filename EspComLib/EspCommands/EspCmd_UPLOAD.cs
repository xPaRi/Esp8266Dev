using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace EspComLib
{
    internal class EspCmd_UPLOAD : EspCommand
    {
        public override string Code => "upload";

        private string _LastFileName = string.Empty;

        public override void Execute(SerialPort serialPort, string argument)
        {
            var fileList = argument.SplitQuotationParameters();

            //--- pokud nejsou zadány žádné soubory k uploadu
            if (!fileList.Any())
            {
                var dialog = new OpenFileDialog()
                {
                    FileName = _LastFileName,
                    Filter = "Lua files (*.lua)|*.lua|Any files (*.*)|*.*",
                    FilterIndex = 0,
                    Multiselect = true
                };
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _LastFileName = dialog.FileName;
                    fileList.AddRange(dialog.FileNames);
                };
            }
            //---

            if (!fileList.Any())
            {
                ConsoleEx.WriteLine("No file(s) specified.");
                return;
            }

            foreach (var fileName in fileList)
            {
                Upload(serialPort, fileName);
            }
        }

        private void Upload(SerialPort serialPort, string fileName)
        {
            var sendedLines = 0;
            Console.WriteLine($"Upload: \"{fileName}\"");

            try
            {
                //--- Naèteme øádky programu
                var lines = File.ReadAllLines(fileName);
                var file = Path.GetFileName(fileName);
                //---

                ConsoleEx.WriteLine("---[ START ]---");
                Helpers.SendCmd(serialPort, sendedLines, $"file.remove('{file}')");
                Helpers.SendCmd(serialPort, sendedLines, $"file.open('{file}', 'w+')");
                Helpers.SendCmd(serialPort, sendedLines, "w = file.writeline");

                foreach (var line in lines)
                {
                    sendedLines++;
                    Helpers.SendLua(serialPort, sendedLines, line);
                }

                ConsoleEx.NewLine();
                Helpers.SendCmd(serialPort, sendedLines, "file.close()");
            }
            catch (Exception ex)
            {
                ConsoleEx.NewLine();
                ConsoleEx.WriteLine(sendedLines <= 0 ? $"---[ ERROR ]---" : $"---[ ERROR in line {sendedLines} ]---");
                ConsoleEx.WriteError(ex);
            }
            finally
            {
                ConsoleEx.WriteLine("---[ END ]---");
                ConsoleEx.WriteLine($"{sendedLines} line(s) sended");
            }

        }

        public override string Description => "Upload file(s). Parameter [filename]";

        public override bool IsMustBeLockReadThread => true;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspComLib
{
    public class ReadFileToStream
    {
        private readonly string _FileName;
        private readonly StreamString _StreamString;

        public ReadFileToStream(StreamString streamString, string filename)
        {
            _FileName = filename;
            _StreamString = streamString;
        }

        public void Start()
        {
            var contents = File.ReadAllText(_FileName);
            _StreamString.WriteString(contents);
        }

    }
}

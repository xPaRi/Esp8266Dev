using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspComLib
{
    public class StreamString
    {
        private readonly Stream _IoStream;
        private readonly UnicodeEncoding _StreamEncoding;

        public StreamString(Stream ioStream)
        {
            _IoStream = ioStream;
            _StreamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var len = _IoStream.ReadByte() * 256 + _IoStream.ReadByte();

            if (len < 0)
                return null;

            var inBuffer = new byte[len];

            _IoStream.Read(inBuffer, 0, len);

            return _StreamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            var outBuffer = _StreamEncoding.GetBytes(outString);
            var len = outBuffer.Length;

            if (len > ushort.MaxValue)
            {
                len = (int)ushort.MaxValue;
            }

            _IoStream.WriteByte((byte)(len / 256));
            _IoStream.WriteByte((byte)(len & 255));
            _IoStream.Write(outBuffer, 0, len);
            _IoStream.Flush();

            return outBuffer.Length + 2;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife
{
    public class StreamBody : IDisposable
    {
        public static StreamBody FromLocalFile(string filePath)
        {
            var stream = File.OpenRead(filePath);
            return new StreamBody
            {
                needDisposeStream = true,
                Stream = stream,
                FileName = Path.GetFileName(filePath),
                Length = stream.Length,
                ContentType = ContentTypeMappings.GetContentTypeByFileName(Path.GetFileName(filePath)),
            };
        }
        public static StreamBody FromStream(Stream stream, string contentType, string fileName, long length, bool autoSeekBegin = true)
        {
            if (autoSeekBegin && stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            return new StreamBody
            {
                FileName = fileName,
                Length = length,
                ContentType = contentType,
                Stream = stream,
            };
        }
        public static StreamBody FromBytes(byte[] bytes, string contentType, string fileName)
        {
            return new StreamBody
            {
                FileName = fileName,
                Length = bytes.Length,
                ContentType = contentType,
                Stream = new MemoryStream(bytes),
                needDisposeStream = true
            };
        }
        private bool _disposed = false;
        private bool needDisposeStream = false;
        public Stream Stream { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public string FileName { get; private set; } = null!;
        public long Length { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing && needDisposeStream)
            {
                Stream.Dispose();
            }
            _disposed = true;
        }

        ~StreamBody()
        {
            // 参数为 false，表示非托管资源由终结器线程清理，不要触碰托管对象
            Dispose(false);
        }

    }
}

using System;
using System.IO;
using System.Net.Http;

namespace Moq.Contrib.HttpClient
{
    /// <summary>
    /// Remembers the stream position the first time <see cref="Create"/> is called and seeks back to that position on
    /// each subsequent call or when the <see cref="StreamContent"/> is disposed, if the stream is seekable. Also
    /// prevents a disposing <see cref="StreamContent"/> from closing the underlying stream. This allows the stream
    /// given to ReturnsResponse to be reused across multiple (non-concurrent) requests.
    /// </summary>
    internal class StreamContentFactory
    {
        private readonly UnclosableStream stream;
        private readonly Lazy<long> position;

        public StreamContentFactory(Stream content)
        {
            stream = new UnclosableStream(content);
            position = new Lazy<long>(() => stream.Position);

            // In case other setups use the same stream, try to seek it back when done
            stream.OnClosed += Reset;
        }

        public StreamContent Create()
        {
            Reset();
            return new StreamContent(stream);
        }

        private void Reset()
        {
            if (stream.CanSeek)
            {
                stream.Position = position.Value;
            }
        }

        private class UnclosableStream : Stream
        {
            private readonly Stream inner;

            public UnclosableStream(Stream inner)
            {
                this.inner = inner;
            }

            public override void Close()
            {
                OnClosed?.Invoke();
            }

            public event Action OnClosed;

            public override bool CanRead => inner.CanRead;
            public override bool CanSeek => inner.CanSeek;
            public override bool CanWrite => inner.CanWrite;
            public override long Length => inner.Length;
            public override long Position { get => inner.Position; set => inner.Position = value; }
            public override void Flush() => inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
            public override void SetLength(long value) => inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
        }
    }
}

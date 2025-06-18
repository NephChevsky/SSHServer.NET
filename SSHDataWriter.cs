using System.Text;

namespace SSHServer.NET
{
	public class SSHDataWriter
	{
		private readonly MemoryStream _ms;

		public SSHDataWriter(int expectedCapacity = 4096)
		{
			_ms = new MemoryStream(expectedCapacity);
		}

		public SSHDataWriter Write(byte value)
		{
			_ms.WriteByte(value);
			return this;
		}

		public SSHDataWriter Write(bool value)
		{
			_ms.WriteByte(value ? (byte)1 : (byte)0);
			return this;
		}

		public SSHDataWriter Write(uint value)
		{
			byte[] bytes = new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
			_ms.Write(bytes, 0, 4);
			return this;
		}

		public SSHDataWriter Write(string str, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(str);
			WriteBinary(bytes);
			return this;
		}

		public SSHDataWriter WriteBytes(ReadOnlyMemory<byte> data)
		{
			_ms.Write(data.Span);
			return this;
		}

		public SSHDataWriter WriteBinary(ReadOnlyMemory<byte> data)
		{
			Write((uint)data.Length);
			_ms.Write(data.Span);
			return this;
		}

		public byte[] ToByteArray()
		{
			if (_ms.TryGetBuffer(out ArraySegment<byte> buf))
				return [.. buf];
			else
				return _ms.ToArray();
		}
	}
}

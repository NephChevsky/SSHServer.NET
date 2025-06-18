using System.Text;

namespace SSHServer.NET
{
	public class SSHDataWriter(int expectedCapacity = 4096)
	{
		private readonly MemoryStream _ms = new(expectedCapacity);

		public SSHDataWriter WriteByte(byte value)
		{
			_ms.WriteByte(value);
			return this;
		}

		public SSHDataWriter WriteBool(bool value)
		{
			_ms.WriteByte(value ? (byte)1 : (byte)0);
			return this;
		}

		public SSHDataWriter WriteUInt(uint value)
		{
			byte[] bytes = [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF)];
			_ms.Write(bytes, 0, 4);
			return this;
		}

		public SSHDataWriter WriteString(string str, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(str);
			WriteBinary(bytes);
			return this;
		}

		public SSHDataWriter WriteMpint(ReadOnlyMemory<byte> data)
		{
			if (data.Length == 1 && data.Span[0] == 0)
			{
				WriteBytes(new byte[4]);
			}
			else
			{
				uint length = (uint)data.Length;
				bool high = ((data.Span[0] & 0x80) != 0);
				if (high)
				{
					WriteUInt(length + 1);
					WriteByte(0);
					WriteBytes(data);
				}
				else
				{
					WriteUInt(length);
					WriteBytes(data);
				}
			}
			return this;
		}

		public SSHDataWriter WriteBytes(ReadOnlyMemory<byte> data)
		{
			_ms.Write(data.Span);
			return this;
		}

		public SSHDataWriter WriteBinary(ReadOnlyMemory<byte> data)
		{
			WriteUInt((uint)data.Length);
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

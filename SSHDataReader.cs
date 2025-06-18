using System.Text;

namespace SSHServer.NET
{
	public class SSHDataReader
	{
		private readonly ReadOnlyMemory<byte> _bytes;
		private int _position;

		public SSHDataReader(ReadOnlyMemory<byte> bytes)
		{
			_bytes = bytes;
		}

		public bool ReadBoolean()
		{
			byte num = ReadByte();

			return num != 0;
		}

		public byte ReadByte()
		{
			ReadOnlySpan<byte> span = ReadBytesAsMemory(1).Span;
			return span[0];
		}

		public uint ReadUInt32()
		{
			ReadOnlySpan<byte> span = ReadBytesAsMemory(4).Span;
			return (uint)(span[0] << 24 | span[1] << 16 | span[2] << 8 | span[3]);
		}

		public ReadOnlyMemory<byte> ReadBytesAsMemory(int length)
		{
			if (_position + length > _bytes.Length)
				throw new ArgumentOutOfRangeException(nameof(length));

			ReadOnlyMemory<byte> bytes = _bytes.Slice(_position, length);
			_position += length;
			return bytes;
		}

		public ReadOnlyMemory<byte> ReadBinaryAsMemory()
		{
			uint length = ReadUInt32();
			return ReadBytesAsMemory((int)length);
		}

		public byte[] ReadBytes(int length)
		{
			return ReadBytesAsMemory(length).ToArray();
		}

		public string ReadString(Encoding encoding)
		{
			ReadOnlySpan<byte> span = ReadBinaryAsMemory().Span;
			return encoding.GetString(span);
		}
	}
}

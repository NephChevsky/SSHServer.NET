namespace SSHServer.NET.Messages
{
	public abstract class Message
	{
		public abstract byte MessageType { get; }

		protected ReadOnlyMemory<byte> RawBytes { get; set; }

		public void Load(ReadOnlyMemory<byte> bytes)
		{
			RawBytes = bytes;

			SSHDataReader reader = new(bytes);
			var number = reader.ReadByte();
			if (number != MessageType)
				throw new ArgumentException(string.Format("Message type {0} is not valid.", number));

			OnLoad(reader);
		}

		public byte[] GetPacket()
		{
			SSHDataWriter writer = new();
			writer.Write(MessageType);

			OnGetPacket(writer);

			return writer.ToByteArray();
		}

		public static T LoadFrom<T>(Message message) where T : Message, new()
		{
			var msg = new T();
			msg.Load(message.RawBytes);
			return msg;
		}

		protected virtual void OnLoad(SSHDataReader reader)
		{
			throw new NotSupportedException();
		}

		protected virtual void OnGetPacket(SSHDataWriter writer)
		{
			throw new NotSupportedException();
		}
	}
}

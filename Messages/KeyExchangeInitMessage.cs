using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace SSHServer.NET.Messages
{
	public class KeyExchangeInitMessage : Message
	{
		private const byte MessageNumber = 20;

		public byte[] Cookie { get; private set; }
		public string[] KeyExchangeAlgorithms { get; set; }
		public string[] ServerHostKeyAlgorithms { get; set; }
		public string[] EncryptionAlgorithmsClientToServer { get; set; }
		public string[] EncryptionAlgorithmsServerToClient { get; set; }
		public string[] MacAlgorithmsClientToServer { get; set; }
		public string[] MacAlgorithmsServerToClient { get; set; }
		public string[] CompressionAlgorithmsClientToServer { get; set; }
		public string[] CompressionAlgorithmsServerToClient { get; set; }
		public string[] LanguagesClientToServer { get; set; }
		public string[] LanguagesServerToClient { get; set; }
		public bool FirstKexPacketFollows { get; set; }
		public uint Reserved { get; set; }
		public override byte MessageType { get { return MessageNumber; } }

		public KeyExchangeInitMessage()
		{
			Cookie = RandomNumberGenerator.GetBytes(16);
			KeyExchangeAlgorithms = [ "curve25519-sha256", "diffie-hellman-group14-sha256", "diffie-hellman-group14-sha1" ];
			ServerHostKeyAlgorithms = [ "rsa-sha2-512", "rsa-sha2-256", "ssh-rsa", "ecdsa-sha2-nistp256", "ssh-ed25519" ];
			EncryptionAlgorithmsClientToServer = EncryptionAlgorithmsServerToClient = ["aes128-ctr", "aes192-ctr", "aes256-ctr", "aes128-gcm@openssh.com", "chacha20-poly1305@openssh.com"];
			MacAlgorithmsClientToServer = MacAlgorithmsServerToClient = [ "hmac-sha2-256", "hmac-sha2-512", "hmac-sha1-96", "hmac-sha1" ];
			CompressionAlgorithmsClientToServer = CompressionAlgorithmsServerToClient = [ "none", "zlib@openssh.com"];
			LanguagesClientToServer = LanguagesServerToClient = [];
			FirstKexPacketFollows = false;
			Reserved = 0;
		}

		protected override void OnLoad(SSHDataReader reader)
		{
			Cookie = reader.ReadBytes(16);
			KeyExchangeAlgorithms = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			ServerHostKeyAlgorithms = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			EncryptionAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			EncryptionAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			MacAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			MacAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			CompressionAlgorithmsClientToServer = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			CompressionAlgorithmsServerToClient = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			LanguagesClientToServer = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			LanguagesServerToClient = reader.ReadString(Encoding.ASCII).Split(',', StringSplitOptions.RemoveEmptyEntries);
			FirstKexPacketFollows = reader.ReadBoolean();
			Reserved = reader.ReadUInt32();
		}

		protected override void OnGetPacket(SSHDataWriter writer)
		{
			writer.WriteBytes(Cookie)
				.WriteString(string.Join(",", KeyExchangeAlgorithms), Encoding.ASCII)
				.WriteString(string.Join(",", ServerHostKeyAlgorithms), Encoding.ASCII)
				.WriteString(string.Join(",", EncryptionAlgorithmsClientToServer), Encoding.ASCII)
				.WriteString(string.Join(",", EncryptionAlgorithmsServerToClient), Encoding.ASCII)
				.WriteString(string.Join(",", MacAlgorithmsClientToServer), Encoding.ASCII)
				.WriteString(string.Join(",", MacAlgorithmsServerToClient), Encoding.ASCII)
				.WriteString(string.Join(",", CompressionAlgorithmsClientToServer), Encoding.ASCII)
				.WriteString(string.Join(",", CompressionAlgorithmsServerToClient), Encoding.ASCII)
				.WriteString(string.Join(",", LanguagesClientToServer), Encoding.ASCII)
				.WriteString(string.Join(",", LanguagesServerToClient), Encoding.ASCII)
				.WriteBool(FirstKexPacketFollows)
				.WriteUInt(Reserved);
		}
	}
}

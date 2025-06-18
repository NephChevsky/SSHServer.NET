using System.Net;
using System.Security.Cryptography;

namespace SSHServer.NET
{
	public class SSHPacket
	{
		public byte[] Payload { get; private set; }
		public byte[] RawPacket { get; private set; }

		private SSHPacket(byte[] payload, byte[] rawPacket)
		{
			Payload = payload;
			RawPacket = rawPacket;
		}

		public static byte[] WrapUnencrypted(byte[] payload)
		{
			const int blockSize = 8;
			const int minPadding = 4;

			int payloadLength = payload.Length;
			int paddingLength = blockSize - ((payloadLength + 5) % blockSize);
			if (paddingLength < minPadding)
				paddingLength += blockSize;

			int packetLength = payloadLength + paddingLength + 1;

			using var ms = new MemoryStream();
			ms.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetLength)), 0, 4);
			ms.WriteByte((byte)paddingLength);
			ms.Write(payload, 0, payload.Length);

			byte[] padding = RandomNumberGenerator.GetBytes(paddingLength);
			ms.Write(padding, 0, paddingLength);

			return ms.ToArray();
		}

		public static async Task<SSHPacket> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
		{
			byte[] buffer = new byte[5];

			await ReadExactAsync(stream, buffer, 0, 5, cancellationToken);

			int packetLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
			if (packetLength < 0 || packetLength > 35000)
				throw new InvalidDataException($"Invalid packet length: {packetLength}");

			byte paddingLength = buffer[4];
			int payloadLength = packetLength - paddingLength - 1;

			if (payloadLength < 0)
				throw new InvalidDataException($"Invalid padding length: {paddingLength}");

			byte[] packetBody = new byte[packetLength];
			await ReadExactAsync(stream, packetBody, 0, packetLength, cancellationToken);

			byte[] payload = new byte[payloadLength];
			Buffer.BlockCopy(packetBody, 0, payload, 0, payloadLength);

			byte[] fullPacket = new byte[4 + 1 + packetLength];
			Buffer.BlockCopy(buffer, 0, fullPacket, 0, 5);
			Buffer.BlockCopy(packetBody, 0, fullPacket, 5, packetLength);

			return new SSHPacket(payload, fullPacket);
		}

		private static async Task ReadExactAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			int read = 0;
			while (read < count)
			{
				int bytesRead = await stream.ReadAsync(buffer.AsMemory(offset + read, count - read), cancellationToken);
				if (bytesRead == 0)
					throw new EndOfStreamException("Unexpected end of SSH stream");
				read += bytesRead;
			}
		}
	}
}

using SSHServer.NET.Messages;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SSHServer.NET
{
	public class SSHSession
	{
		private readonly NetworkStream _networkStream;

		private string _clientBanner;

		public SSHSession(TcpClient client)
		{
			_networkStream = client.GetStream();
		}

		public async Task ReceiveConnection(string serverBanner, CancellationToken cancellationToken)
		{
			await ExchangeServerBanner(serverBanner, cancellationToken);

			await ExchangeKeyInit(cancellationToken);

			
		}

		public async Task ExchangeServerBanner(string serverBanner, CancellationToken cancellationToken)
		{
			string formatted = serverBanner.Trim('\r', '\n') + "\r\n";
			byte[] data = Encoding.ASCII.GetBytes(formatted);
			await _networkStream.WriteAsync(data, cancellationToken);
			await _networkStream.FlushAsync(cancellationToken);

			List<byte> buffer = [];
			int prevByte = -1;

			while (true)
			{
				int b = _networkStream.ReadByte();
				if (b == -1)
				{
					// TODO: close connection gracefully
					throw new IOException("Stream closed by client before sending banner");
				}

				if (prevByte == '\r' && b == '\n')
				{
					buffer.RemoveAt(buffer.Count - 1);
					break;
				}

				buffer.Add((byte)b);
				prevByte = b;

				if (buffer.Count > 255)
				{
					// TODO: close connection gracefully
					throw new InvalidOperationException("SSH client banner too long");
				}
			}

			_clientBanner = Encoding.ASCII.GetString(buffer.ToArray());

			if (!_clientBanner.StartsWith("SSH-2.0-"))
			{
				// TODO: close connection gracefully
				throw new InvalidOperationException("Unsupported SSH version: " + _clientBanner);
			}
		}

		public async Task ExchangeKeyInit(CancellationToken cancellationToken)
		{
			KeyExchangeInitMessage kexInit = new();
			byte[] payload = kexInit.GetPacket();
			byte[] packet = SSHPacket.WrapUnencrypted(payload);
			await _networkStream.WriteAsync(packet, cancellationToken);

			SSHPacket receivedPacket = await SSHPacket.ReadAsync(_networkStream, cancellationToken);
			KeyExchangeInitMessage clientKexInit = new();
			clientKexInit.Load(receivedPacket.Payload);
		}
	}
}

using SSHServer.NET.Messages;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SSHServer.NET
{
	public class SSHSession(TcpClient client, string serverBanner)
	{
		private readonly NetworkStream _networkStream = client.GetStream();

		private string _clientBanner;
		private readonly string _serverBanner = serverBanner;

		public async Task ReceiveConnection(CancellationToken cancellationToken)
		{
			await ExchangeBanners(cancellationToken);

			await ExchangeKeys(cancellationToken);

			
		}

		private async Task ExchangeBanners(CancellationToken cancellationToken)
		{
			string formatted = _serverBanner.Trim('\r', '\n') + "\r\n";
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

			_clientBanner = Encoding.ASCII.GetString([.. buffer]);

			if (!_clientBanner.StartsWith("SSH-2.0-"))
			{
				// TODO: close connection gracefully
				throw new InvalidOperationException("Unsupported SSH version: " + _clientBanner);
			}
		}

		private async Task ExchangeKeys(CancellationToken cancellationToken)
		{
			KeyExchangeInitMessage serverKeyExchangeInit = new();
			byte[] payload = serverKeyExchangeInit.GetPacket();
			byte[] packet = SSHPacket.WrapUnencrypted(payload);
			await _networkStream.WriteAsync(packet, cancellationToken);

			SSHPacket receivedPacket = await SSHPacket.ReadAsync(_networkStream, cancellationToken);
			KeyExchangeInitMessage clientKeyExchangeInit = new();
			clientKeyExchangeInit.Load(receivedPacket.Payload);

			string keyExchangeAlgorithm = SelectAlgorithm([.. serverKeyExchangeInit.KeyExchangeAlgorithms], [.. clientKeyExchangeInit.KeyExchangeAlgorithms]);

			if (string.IsNullOrEmpty(keyExchangeAlgorithm))
			{
				throw new Exception("Failed to retrieve a common algorithm between server and client");
			}
			if (keyExchangeAlgorithm == "curve25519-sha256")
			{
				SSHPacket clientECDHInitPacket = await SSHPacket.ReadAsync(_networkStream, cancellationToken);
				KeyExchangeECDhInitMessage clientECDHInit = new();
				clientECDHInit.Load(clientECDHInitPacket.Payload);
			}
		}

		private static string SelectAlgorithm(List<string> serverAlgorithms, List<string> clientAlgorithms)
		{
			foreach (string algorithm in serverAlgorithms)
			{
				if (clientAlgorithms.Contains(algorithm))
				{
					return algorithm;
				}
			}
			return null;
		}
	}
}

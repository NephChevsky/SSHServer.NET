using System.Net.Sockets;
using System.Text;

namespace SSHServer.NET
{
	public class SSHSession
	{
		private readonly StreamWriter _writerStream;
		private readonly StreamReader _readerStream;

		private string _clientBanner;

		public SSHSession(TcpClient client)
		{
			NetworkStream networkStream = client.GetStream();
			_readerStream = new(networkStream, Encoding.ASCII, leaveOpen: true);
			_writerStream = new(networkStream, Encoding.ASCII, leaveOpen: true);
		}

		public async Task WriteServerBanner(string banner, CancellationToken cancellationToken)
		{
			if (banner.EndsWith("\r\n"))
			{
				banner = banner.Trim('\r', '\n');
			}
			await _writerStream.WriteLineAsync(banner);
		}

		public async Task ReadClientBanner()
		{
			_clientBanner = await _readerStream.ReadLineAsync();
		}
	}
}

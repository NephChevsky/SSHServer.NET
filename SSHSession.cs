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
			await _writerStream.WriteLineAsync(banner.Trim('\r', '\n'));
		}

		public async Task ReadClientBanner()
		{
			_clientBanner = await _readerStream.ReadLineAsync();
		}
	}
}

using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
using System.Net;

namespace SSHServer.NET
{
	public class SSHServer : BackgroundService
	{
		private TcpListener _listener;
		private string _serverBanner = "SSH-2.0-SSHServer.NET_0.1";

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_listener = new TcpListener(IPAddress.Any, 2222);
			_listener.Start();
			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
					_ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"Socket exception: {ex.Message}");
				}
			}
		}

		async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
		{
			using (client)
			{
				SSHSession session = new(client);
				await session.WriteServerBanner(_serverBanner, cancellationToken);
				await session.ReadClientBanner();
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_listener.Stop();
			return base.StopAsync(cancellationToken);
		}
	}
}

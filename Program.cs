using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SSHServer.NET
{
    internal class Program
    {
		static async Task Main(string[] args)
		{
			IHost host = Host.CreateDefaultBuilder(args)
			.ConfigureServices(services =>
			{
				services.AddHostedService<SSHServer>();
			})
			.Build();

			await host.RunAsync();
		}
	}
}

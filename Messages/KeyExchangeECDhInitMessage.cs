using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSHServer.NET.Messages
{
	internal class KeyExchangeECDhInitMessage : KeyExchangeXInitMessage
	{
		public byte[] Q { get; private set; }

		protected override void OnLoad(SSHDataReader reader)
		{
			Q = reader.ReadBinary();
		}
	}
}

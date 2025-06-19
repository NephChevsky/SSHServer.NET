using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSHServer.NET.Messages
{
	internal class KeyExchangeXInitMessage : Message
	{
		private const byte MessageNumber = 30;

		public override byte MessageType { get { return MessageNumber; } }

		protected override void OnLoad(SSHDataReader reader)
		{
		}
	}
}

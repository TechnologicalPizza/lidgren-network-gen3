﻿
namespace Lidgren.Network
{
    internal sealed class NetReliableUnorderedReceiver : NetReceiverChannel
	{
		private int _windowStart;
		private int _windowSize;
		private NetBitArray _earlyReceived;

		public NetReliableUnorderedReceiver(NetConnection connection, int windowSize)
			: base(connection)
		{
			_windowSize = windowSize;
			_earlyReceived = new NetBitArray(windowSize);
		}

		private void AdvanceWindow()
		{
			_earlyReceived.Set(_windowStart % _windowSize, false);
			_windowStart = (_windowStart + 1) % NetConstants.SequenceNumbers;
		}

		public override void ReceiveMessage(in NetMessageView message)
		{
			int relate = NetUtility.RelativeSequenceNumber(message.SequenceNumber, _windowStart);

			// ack no matter what
			Connection.QueueAck(message.BaseMessageType, message.SequenceNumber);

			if (relate == 0)
			{
				// Log("Received message #" + message.SequenceNumber + " right on time");

				// excellent, right on time

				AdvanceWindow();
				Peer.ReleaseMessage(message);

				// release withheld messages
				int nextSeqNr = (message.SequenceNumber + 1) % NetConstants.SequenceNumbers;

				while (_earlyReceived[nextSeqNr % _windowSize])
				{
					AdvanceWindow();
					nextSeqNr++;
				}

				return;
			}

			if (relate < 0)
			{
				// duplicate
				Peer.LogVerbose("Received message #" + message.SequenceNumber + " DROPPING DUPLICATE");
				return;
			}

			// relate > 0 = early message
			if (relate > _windowSize)
			{
				// too early message!
				Peer.LogDebug("Received " + message.ToString() + " TOO EARLY! Expected " + _windowStart);
				return;
			}

			_earlyReceived.Set(message.SequenceNumber % _windowSize, true);

			Peer.ReleaseMessage(message);
		}
	}
}

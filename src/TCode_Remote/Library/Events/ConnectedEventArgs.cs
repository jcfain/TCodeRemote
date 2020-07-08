using System;

namespace TCode_Remote.Library.Events
{
	public class ConnectedEventArgs : EventArgs
	{
		public bool IsConnected { get; set; }
		public string Message { get; set; }
	}
}

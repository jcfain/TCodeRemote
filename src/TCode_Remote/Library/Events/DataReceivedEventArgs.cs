using TCode_Remote.Library.Model;
using System;
using System.Collections.Generic;

namespace TCode_Remote.Library.Events
{
	public class DataReceivedEventArgs : EventArgs
	{
		public string Data { get; set; }
		public HashSet<ChannelValueModel> GamepadData { get; set; }
	}
}

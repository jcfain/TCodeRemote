using System;
using TCode_Remote.Library.Model;

namespace TCode_Remote.Library.Events
{
	public class DeviceCollectionChangeEventArgs : EventArgs
	{
		public DeviceModel Device { get; set; }
	}
}

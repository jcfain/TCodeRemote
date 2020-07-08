using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Reference.Enum;

namespace TCode_Remote.Library.Model
{
	public class DeviceInitModel: IDeviceInit
	{
		public OSRRemoteDeviceMode Mode { get; set; }
		public string Address { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCode_Remote.Library.Reference.Enum
{
	public enum OSRRemoteInputMode
	{
		Address,
		Gamepad,
		Serial
	}

	public enum OSRRemoteOutputMode
	{
		Address,
		Serial,
		BLE
	}

	public enum OSRRemoteDeviceMode
	{
		Out,
		In
	}
}

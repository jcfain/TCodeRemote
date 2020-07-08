
using System;
using System.Threading.Tasks;
using TCode_Remote.Library.Events;
using Windows.Networking.Vpn;

namespace TCode_Remote.Library.Interfaces
{
	public interface IDevice
	{
		bool IsRunning { get; }
		bool IsConnected { get; }
		EventHandler<ConnectedEventArgs> ConnectionChange { get; set; }
		EventHandler<DataReceivedEventArgs> DataRecieved { get; set; }
		EventHandler<DataReceivedEventArgs> GamepadDataRecieved { get; set; }
		void Init(IDeviceInit deviceInit);
		Task SetIODeviceAsync(IDevice device);
		void Stop();
		void Dispose();
	}
}

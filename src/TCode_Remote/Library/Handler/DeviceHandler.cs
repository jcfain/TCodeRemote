using TCode_Remote.Library.Events;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Reference.Enum;
using System;
using System.ComponentModel;
using System.Web.Script.Serialization;

namespace TCode_Remote.Library.Handler
{
	public abstract class DeviceHandler : INotifyPropertyChanged
	{
		public bool IsRunning { get { return _isRunning; } }
		public bool IsConnected { get { return _isConnected; } }
		public EventHandler<DataReceivedEventArgs> DataRecieved { get; set; }
		public EventHandler<DataReceivedEventArgs> GamepadDataRecieved { get; set; }
		public EventHandler<ConnectedEventArgs> ConnectionChange { get; set; }
		public event PropertyChangedEventHandler PropertyChanged;

		protected IDevice _device;
		protected OSRRemoteDeviceMode _mode { get; set; }
		protected bool _isConnected = false;
		protected bool _isRunning = false;

		protected JavaScriptSerializer _serializer = new JavaScriptSerializer();

		protected void OnPropertyChanged<T>(object instance, T property)
		{
			PropertyChanged?.Invoke(instance, new PropertyChangedEventArgs(nameof(property)));
		}

		protected void UpdateConnected(bool isConnected, string message = "")
		{
			_isConnected = isConnected;
			EventHandler<ConnectedEventArgs> handler = ConnectionChange;
			if (handler != null)
			{
				ConnectedEventArgs eventArgs = new ConnectedEventArgs();
				eventArgs.IsConnected = isConnected;
				eventArgs.Message = message;
				handler.Invoke(this, eventArgs);
			}
		}
	}
}

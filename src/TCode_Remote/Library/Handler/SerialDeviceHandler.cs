using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading.Tasks;
using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Library.Events;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Tools;
using System.Threading;
using System.Linq;
using System.Management;
using TCode_Remote.Library.Model;

namespace TCode_Remote.Library.Handler
{
	public class SerialDeviceHandler : DeviceHandler, IDevice
	{
		private SerialPort _port;
		private string _portName;

		public SerialDeviceHandler(OSRRemoteDeviceMode mode)
		{
			_mode = mode;
		}

		public void Init(IDeviceInit deviceInit)
		{
			_portName = deviceInit.Address;
			if (string.IsNullOrEmpty(_portName))
			{
				return;
			}
			//if (_portName.Length != 4)
			//{
			//	_portName = "\\\\.\\" + _portName;
			//}
			UpdateConnected(false, SettingsHandler.DEVICE_CONNECT);
			OpenPort();
			if (IsRunning && _mode == OSRRemoteDeviceMode.Out)
			{
				_port.DataReceived -= Port_DataReceived_IsTcode;
				_port.DataReceived += Port_DataReceived_IsTcode;
				LookForTcodeDevice();
			}
			else if (IsRunning && _mode == OSRRemoteDeviceMode.In)
			{
				UpdateConnected(true);
			}
		}

		public async Task SetIODeviceAsync(IDevice device)
		{
			if (device == this)
			{
				throw new InvalidProgramException("IO device cannot be me");
			}
			_device = device;
			OpenPort();
			if (_mode == OSRRemoteDeviceMode.In)
			{
				_port.DataReceived += Port_DataReceived;
			}
			else
			{
				_device.DataRecieved += Device_DataReceived;
			}
		}

		public void OpenPort()
		{
			try
			{
				if (_port != null && _port.IsOpen)
				{
					_port.Close();
				}
				_port = new SerialPort(_portName);
				_port.BaudRate = 115200;
				_port.ReadTimeout = 500;
				_port.WriteTimeout = 500;
				_port.DtrEnable = true;
				_port.Open();
				_isRunning = true;
			}
			catch (Exception e)
			{
				Log.Dialog("Error opening: " + _portName, e.Message);
				Dispose();
				UpdateConnected(false);
				return;
			}
		}

		public void Stop()
		{
			_isRunning = false;
			if (_device != null)
			{
				_device.DataRecieved -= Device_DataReceived;
			}
			if (_port != null)
			{
				_port.DataReceived -= Port_DataReceived;
				_port.DataReceived -= Port_DataReceived_IsTcode;
			}
		}

		public void Dispose()
		{
			if (_port != null)
			{
				_port.DataReceived -= Port_DataReceived_IsTcode;
			}
			if (_port != null && _port.IsOpen)
			{
				_port.Close();
			}
			UpdateConnected(false);
			Stop();
		}

		public ObservableCollection<SerialDeviceModel> GetPorts()
		{
			using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
			{
				var portnames = SerialPort.GetPortNames();
				var ports = searcher.Get().Cast<ManagementBaseObject>().ToList().Select(p => p["Caption"].ToString());

				var portsCollection = new ObservableCollection<SerialDeviceModel>(
					portnames.Select(n => new SerialDeviceModel()
					{
						Name = n,
						FriendlyName = ports.FirstOrDefault(s => s.Contains(n)) ?? n
					})
				);
				portsCollection.Add(new SerialDeviceModel() { Name = null, FriendlyName = "None" });
				return portsCollection;
			}
		}

		private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (_port != null && _port.IsOpen && IsRunning)
			{
				EventHandler<DataReceivedEventArgs> handler = DataRecieved;
				if (handler != null)
				{
					var eventArgs = new DataReceivedEventArgs();
					eventArgs.Data = _port.ReadExisting();
					handler(this, eventArgs);
				}
			}
		}

		private void Port_DataReceived_IsTcode(object sender, SerialDataReceivedEventArgs e)
		{
			if (_port != null && _port.IsOpen)
			{
				try
				{
					var data = _port.ReadLine();
					Log.Debug("Com device response: " + data);
					if (data.Contains(SettingsHandler.TCodeVersion))
					{
						UpdateConnected(true);
						_port.DataReceived -= Port_DataReceived_IsTcode;
					}
					//else
					//{
					//	Dispose();
					//	UpdateConnected(false);
					//	Log.Dialog("Invalid device", $"Device at address ({_portName}) responded with an invalid response.\n{data}");
					//}
				} 
				catch(Exception ex)
				{
					Log.Dialog("Error", $"Device at address ({_portName}) failed to respond: "+ ex.Message);
				}
			}
		}

		private void Device_DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (_port != null)
			{
				try
				{
					if (!_port.IsOpen)
						_port.Open();
					if (!e.Data.Contains("S") && _mode == OSRRemoteDeviceMode.Out)
					{
						e.Data = e.Data.Replace(" ", $"S{SettingsHandler.Speed} ");
						e.Data = e.Data.Replace("\n", $"S{SettingsHandler.Speed}\n");
					}
					_port.WriteLine(e.Data);
					Log.Debug($"Serial write: {e.Data}");
					Thread.Sleep(100);
				} 
				catch (Exception ex)
				{
					Dispose();
					UpdateConnected(false);
					Log.Dialog("Error sending", "Error sending to port: " + _portName  + " " + ex.Message);
				}
			}
			else
			{
				Dispose();
				UpdateConnected(false);
				Log.Dialog("No connection", "Port not open: " + _portName);
			}
		}

		private void LookForTcodeDevice()
		{
			Task.Run(() =>
			{
				while (!_isConnected && IsRunning)
				{
					try
					{
						Thread.Sleep(1000);
						Log.Debug("Sending D1 to " + _portName);
						_port.Write("D1\n");
						Thread.Sleep(5000);
					} 
					catch (Exception e)
					{
						Log.Dialog("Error accessing COM port when looking for TCode device", e.Message);
						_isRunning = false;
					}
				}
			});
		}
	}
}

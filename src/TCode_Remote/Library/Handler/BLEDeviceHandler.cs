using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Library.Events;
using TCode_Remote.Library.Tools;
using System.Collections.ObjectModel;

namespace TCode_Remote.Library.Handler
{
	public class BLEDeviceHandler : DeviceHandler, IDevice
	{

		public EventHandler<DeviceCollectionChangeEventArgs> DeviceAdded { get; set; }
		public EventHandler<DeviceCollectionChangeEventArgs> DeviceRemoved { get; set; }

		private GattCharacteristic _commandCharacteristic;
		private GattCharacteristic _serialCharacteristic;
		private GattCharacteristic _modelNumberStringharacteristic;
		private GattDeviceService _selectedDeviceServices;
		private DeviceInformation _selectedDeviceInformation;
		private const string SerialPortUUID = "0000dfb1-0000-1000-8000-00805f9b34fb";
		private const string CommandUUID = "0000dfb2-0000-1000-8000-00805f9b34fb";
		private const string ServiceUUID = "0000dfb0-0000-1000-8000-00805f9b34fb";
		private readonly object mWriteLock = new object();
		private readonly List<DeviceInformation> _deviceInformationList;

		private DeviceWatcher deviceWatcher;
		private int _mBaudrate = 115200; //set the default baud rate to 115200
		private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

		public BLEDeviceHandler(OSRRemoteDeviceMode mode)
		{
			//_devices = new ObservableCollection<DeviceModel>();
			_deviceInformationList = new List<DeviceInformation>();
			_mode = mode;
		}

		public void Init(IDeviceInit deviceInit)
		{
			if (_mode == OSRRemoteDeviceMode.Out && !_isRunning)
			{
				_isRunning = true;
				StartWatcher();
			}
		}

		public async Task SetIODeviceAsync(IDevice device)
		{
			_device = device;
			if (_mode == OSRRemoteDeviceMode.Out)
			{
				_device.DataRecieved += Device_DataReceived;
			}
		}


		public void Stop()
		{
			_isRunning = false;
			StopWatcher();
			if (_device != null)
			{
				_device.DataRecieved -= Device_DataReceived;
			}
		}

		public void Dispose()
		{
			Stop();
			if (_serialCharacteristic != null && _serialCharacteristic.Service != null)
			{
				_serialCharacteristic.Service.Dispose();
				_serialCharacteristic = null;
			}
			UpdateConnected(false);
		}

		private void StartWatcher()
		{
			UpdateConnected(false, SettingsHandler.DEVICE_CONNECT);
			string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };
			//TODO wwhere does this fit in? I seen it on a microsoft video but they didnt say....
			string aqsAllBlueToothLEDevices = "{System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
			deviceWatcher =
						DeviceInformation.CreateWatcher(
								BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
								requestedProperties,
								DeviceInformationKind.AssociationEndpoint);

			// Register event handlers before starting the watcher.
			// Added, Updated and Removed are required to get all nearby devices
			deviceWatcher.Added += DeviceWatcher_Added; ;
			deviceWatcher.Updated += DeviceWatcher_Updated;
			deviceWatcher.Removed += DeviceWatcher_Removed;

			// EnumerationCompleted and Stopped are optional to implement.
			deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
			deviceWatcher.Stopped += DeviceWatcher_Stopped;

			deviceWatcher.Start();
		}

		private void StopWatcher()
		{
			// Since the device watcher runs in the background, it is possible that
			// a notification is "in flight" at the time we stop the watcher.
			// In other words, it is possible for the watcher to become stopped while a
			// handler is running, or for a handler to run after the watcher has stopped.

			if (IsWatcherStarted(deviceWatcher))
			{
				deviceWatcher.Added -= DeviceWatcher_Added; ;
				deviceWatcher.Updated -= DeviceWatcher_Updated;
				deviceWatcher.Removed -= DeviceWatcher_Removed;

				// EnumerationCompleted and Stopped are optional to implement.
				deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
				deviceWatcher.Stopped -= DeviceWatcher_Stopped;
				// We do not null out the deviceWatcher yet because we want to receive
				// the Stopped event.
				deviceWatcher.Stop();
			}
		}

		public async Task ConnectDevice(string id)
		{
			BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(id);
			if (bluetoothLeDevice != null)
			{
				GattDeviceServicesResult gattDeviceServicesResult = await bluetoothLeDevice.GetGattServicesAsync();
				Log.Debug($"{bluetoothLeDevice.Name} Services: {gattDeviceServicesResult.Services.Count}, {gattDeviceServicesResult.Status}, {gattDeviceServicesResult.ProtocolError}");

				if (gattDeviceServicesResult.Status == GattCommunicationStatus.Success)
				{
					_selectedDeviceServices = gattDeviceServicesResult.Services.FirstOrDefault(x => x.Uuid.ToString() == ServiceUUID);
					foreach(var service in gattDeviceServicesResult.Services)
					{
						Log.Debug("service: " + service.Uuid);
					}
					if (_selectedDeviceServices != null)
					{
						GattCharacteristicsResult gattCharacteristicsResult = await _selectedDeviceServices.GetCharacteristicsAsync();

						if (gattCharacteristicsResult.Status == GattCommunicationStatus.Success)
						{
							var characteristics = gattCharacteristicsResult.Characteristics.Where(x => x.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write));
							_commandCharacteristic = gattCharacteristicsResult.Characteristics.FirstOrDefault(x => x.Uuid.ToString() == CommandUUID);
							_serialCharacteristic = gattCharacteristicsResult.Characteristics.FirstOrDefault(x => x.Uuid.ToString() == SerialPortUUID);
							GattCharacteristicProperties properties = _serialCharacteristic.CharacteristicProperties;
							if (_serialCharacteristic != null)
							{
								if (properties.HasFlag(GattCharacteristicProperties.Read))
								{
									ReadBLESerial();
								}
								if (properties.HasFlag(GattCharacteristicProperties.Write))
								{
									WriteBlunoSettings();
								}
								if (properties.HasFlag(GattCharacteristicProperties.Notify))
								{
									// This characteristic supports subscribing to notifications.
								}
							}
						}
					}
				}
			}
		}


		private async void WriteBlunoSettings()
		{
			//string mPassword = "AT+PASSWOR=DFRobot\r\n";
			//string mBaudrateBuffer = "AT+UART=" + _mBaudrate + "\r\n";

			//await WriteSerial(mPassword);
			//await WriteSerial(mBaudrateBuffer);
			StopWatcher();
			UpdateConnected(true);
		}

		private async void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
		{
			await Task.Run(() => 
			{
				Log.Debug("DeviceWatcher_Stopped");
			});
		}

		private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
		{
			await Task.Run(() => 
			{
				Log.Debug("DeviceWatcher_EnumerationCompleted");
				//StopWatcher();
			});
		}

		private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			await Task.Run(() => 
			{
				Log.Debug("DeviceWatcher_Removed: Key: " + args.Properties.Keys.FirstOrDefault() + " ID: " + args.Id);
				//_devices.Remove(_devices.First(x => x.Id == args.Id));
			});
			var deviceName = _deviceInformationList.FirstOrDefault(x => x.Id == args.Id)?.Name;
			DeviceRemoved?.Invoke(this, new DeviceCollectionChangeEventArgs() { Device = new DeviceModel() { Id = args.Id, Name = deviceName } });
		}

		private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
		{
			await Task.Run(() => 
			{
				Log.Debug("DeviceWatcher_Updated: Key: " + args.Properties.Keys.FirstOrDefault() + " ID: " + args.Id);
			});
		}
		object __lockObj = new object();
		private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
		{
			await Task.Run(() => 
			{
				lock (__lockObj) {
					if (!string.IsNullOrEmpty(args.Name)) {
						var deviceExists = _deviceInformationList.Any(x => x.Name == args.Name);
						if (!deviceExists)
						{
							Log.Debug("New device detected");
							Log.Debug("Name: " + args.Name + " ID: " + args.Id);
							DeviceAdded?.Invoke(this, new DeviceCollectionChangeEventArgs() { Device = new DeviceModel() { Id = args.Id, Name = args.Name } });
							_deviceInformationList.Add(args);
							if (args.Name == "Bluno")
							{
								ConnectDevice(args.Id);
								StopWatcher();
							}
						}
						else
						{
							Log.Debug("Device Updated");
							Log.Debug("Name: " + args.Name + " ID: " + args.Id);
							DeviceRemoved?.Invoke(this, new DeviceCollectionChangeEventArgs() { Device = new DeviceModel() { Id = args.Id, Name = args.Name } });
							DeviceAdded?.Invoke(this, new DeviceCollectionChangeEventArgs() { Device = new DeviceModel() { Id = args.Id, Name = args.Name } });
							_deviceInformationList.Remove(_deviceInformationList.First(x => x.Name == args.Name));
							_deviceInformationList.Add(args);
						}
					}
				}
			});
		}

		private bool IsWatcherRunning()
		{
			if (deviceWatcher == null)
			{
				return false;
			}

			DeviceWatcherStatus status = deviceWatcher.Status;
			return (status == DeviceWatcherStatus.Started) ||
				(status == DeviceWatcherStatus.EnumerationCompleted) ||
				(status == DeviceWatcherStatus.Stopping);
		}

		public bool IsWatcherStarted(DeviceWatcher watcher)
		{
			return (watcher.Status == DeviceWatcherStatus.Started) ||
				(watcher.Status == DeviceWatcherStatus.EnumerationCompleted);
		}


		IEnumerable<string> Split(string str)
		{
			while (str.Length > 0)
			{
				yield return new string(str.Take(19).ToArray());
				str = new string(str.Skip(19).ToArray());
			}
		}

		private async void Device_DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
			{
				await semaphoreSlim.WaitAsync();
				try
				{
					var xAxis = e.Data.IndexOf("L");
					var endOfXAxis = e.Data.IndexOf(" ");
					if (xAxis > -1 && endOfXAxis > -1)
					{
						//var lines = Split(e.Data);
						//foreach (var line in lines)
						//{
						var line = e.Data.Substring(xAxis, endOfXAxis);
						if (!line.Contains("S"))
						{
							line += "S" + SettingsHandler.Speed;
						}
						await WriteTCode(line);
						Log.Debug($"BLE Serial write:{DateTime.Now} {line}");
						//}
						//Thread.Sleep(100);
					}
				}
				finally
				{
					//When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
					//This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
					semaphoreSlim.Release();
				}
			}
		}

		public async Task WriteTCode(string command)
		{
			await WriteSerial(command + "\n");
		}

		private async Task WriteSerial(string data)
		{
			if (_serialCharacteristic != null)
			{
				var writer = new DataWriter();
				writer.WriteString(data);

				try
				{
					var buffer = writer.DetachBuffer();
					var result = await _serialCharacteristic.WriteValueAsync(buffer);
					if (result == GattCommunicationStatus.Success)
					{
						Log.Debug($"Write serial success\n{data}");
					}
					else
					{
						Log.Debug($"Write serial NOT success: {result}");
					}
				}
				catch (Exception ex) when ((uint)ex.HResult == 0x80650003 || (uint)ex.HResult == 0x80070005)
				{
					Log.Dialog("Error writing: " + data, "Message: " + ex.Message);
				}
				catch (Exception ex)
				{
					Log.Debug($"Exception writing: { ex.Message }\ndata:\n{ data }");
				}
				//writer.Dispose();
			}
		}

		public async void ReadBLESerial()
		{
			////SerialPort port = new SerialPort("COM9", 9600, Parity.None, 8, StopBits.One);
			//GattReadResult result = await _serialCharacteristic.ReadValueAsync();
			//if (result.Status == GattCommunicationStatus.Success)
			//{
			//	var reader = DataReader.FromBuffer(result.Value);
			//	byte[] input = new byte[reader.UnconsumedBufferLength];
			//	//var data = reader.ReadString(data);
			//	reader.ReadBytes(input);
			//	// Utilize the data as needed
			//}
		}
	}
}


using System;
using System.Threading.Tasks;
using System.Text;
using System.Net.Sockets;
using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Library.Events;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Model;
using System.Web.Script.Serialization;
using TCode_Remote.Library.Tools;
using System.Collections.Generic;
using System.Threading;
using System.Net;

namespace TCode_Remote.Library.Handler
{
	public class UdpHandler : DeviceHandler, IDevice
	{
		private string _address;
		private int _port;
		private UdpClient _udpClient;
		private TCodeFactory _tcodeFactory;

		public UdpHandler(OSRRemoteDeviceMode mode)
		{
			_mode = mode;
		}

		public void Init(IDeviceInit deviceInit)
		{
			Dispose();
			_address = deviceInit.Address;
			_port = ((NetworkInitModel)deviceInit).Port;
			_tcodeFactory = new TCodeFactory(0.0, 1.0);
			_udpClient = new UdpClient();
			UpdateConnected(false, SettingsHandler.DEVICE_CONNECT);
			switch (_mode)
			{
				case OSRRemoteDeviceMode.In:
					StartInputServer();
					break;
				case OSRRemoteDeviceMode.Out:
					StartOutputClient();
					break;
			}
		}

		public async Task SetIODeviceAsync(IDevice device)
		{
			_device = device;
			_isRunning = true;
			switch (_mode)
			{
				case OSRRemoteDeviceMode.In:
					break;
				case OSRRemoteDeviceMode.Out:
					if (_device.GetType() == typeof(GamepadHandler))
					{
						_device.DataRecieved += GamepadDevice_DataReceived;
					}
					else
					{
						_device.DataRecieved += Device_DataReceived;
					}
					break;
			}
		}

		public void Stop()
		{
			_isRunning = false;
			if (_device != null)
			{
				_device.DataRecieved -= Device_DataReceived;
				_device.DataRecieved -= GamepadDevice_DataReceived;
			}
		}

		public void Dispose()
		{
			Stop();
			UpdateConnected(false);
			if (_udpClient != null)
			{
				_udpClient.Close();
			}
		}

		private async void Device_DataReceived(object sender, DataReceivedEventArgs e)
		{

			if (_udpClient != null && IsRunning)
			{
				try
				{
					Log.Debug("sentData: " + e.Data);
					Byte[] senddata = Encoding.ASCII.GetBytes(e.Data);
					if (IsRunning)
						await _udpClient.SendAsync(senddata, senddata.Length);
				} catch(Exception error)
				{
					Log.Dialog("Error sending to udp client.", error.Message);
					Dispose();
				}
			}
		}

		private async void GamepadDevice_DataReceived(object sender, DataReceivedEventArgs e)
		{

			if (_udpClient != null && IsRunning)
			{
				try
				{
					var serializedstring = new JavaScriptSerializer().Serialize(e.GamepadData);
					Log.Debug("sentData: " + serializedstring);
					Byte[] senddata = Encoding.ASCII.GetBytes(serializedstring);
					if(IsRunning)
						await _udpClient.SendAsync(senddata, senddata.Length);
				}
				catch (Exception error)
				{
					Log.Dialog("Error sending gamepad data to udp client.", error.Message);
					Dispose();
				}
			}
		}

		private async Task StartInputServer()
		{
			_isRunning = true;
			await Task.Run(async () =>
			{
				while (IsRunning)
				{
					try
					{

						var ipEndPoint = new IPEndPoint(IPAddress.Any, _port);
						using (var udpClient = new UdpClient(_port))
						{
							//IPEndPoint object will allow us to read datagrams sent from any source.
							var receivedResults = udpClient.Receive(ref ipEndPoint);
							var returnData = Encoding.ASCII.GetString(receivedResults);
							Log.Debug(DateTime.Now + " returnData: " + returnData);
							if (returnData.Contains(SettingsHandler.HandShakeChannel))
							{
								_udpClient = new UdpClient();
								_udpClient.Connect(ipEndPoint.Address, 54000);
								var connected = Encoding.ASCII.GetBytes(SettingsHandler.TCodeVersion);
								_udpClient.Send(connected, connected.Length);
								UpdateConnected(true);
								continue;
							}
							else if (returnData.Contains("{"))
							{
								// Deserialize cannot tell the differenct between a HashSet and a List :/
								var dataList = new JavaScriptSerializer().Deserialize<List<ChannelValueModel>>(returnData);
								var data = new HashSet<ChannelValueModel>(dataList);
								returnData = _tcodeFactory.FormatTCode(data);
							}
							EventHandler<DataReceivedEventArgs> handler = DataRecieved;
							if (handler != null)
							{
								var eventArgs = new DataReceivedEventArgs();
								eventArgs.Data = returnData;
								handler(this, eventArgs);
							}
						}
					}
					catch (Exception e)
					{
						_isRunning = false;
						Log.Dialog("Error getting udp data", e.ToString());
					}
				}
			});
		}

		private async void StartOutputClient()
		{
			if (!IsRunning)
			{
				await SendClientHandshake();
			}
		}

		private async Task SendClientHandshake()
		{
			if (_udpClient != null)
			{
				_isRunning = true;
				await Task.Run(async () =>
				{
					while (!IsConnected && IsRunning)
					{
						_udpClient.Connect(_address, _port);
						var password = Encoding.ASCII.GetBytes(SettingsHandler.HandShakeChannel);
						Log.Debug(DateTime.Now + " Sending udp connection handshake");
						_udpClient.Send(password, password.Length);
						var receivedResults = await _udpClient.ReceiveAsync();
						var returnData = Encoding.ASCII.GetString(receivedResults.Buffer);
						Log.Debug(DateTime.Now + " udp connection handshake returnData: " + returnData);
						if (returnData.Contains(SettingsHandler.TCodeVersion))
						{
							UpdateConnected(true);
						}
					}
				});
			}
		}
	}
}

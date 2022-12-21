using TCode_Remote.Library.Events;
using TCode_Remote.Library.Interfaces;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Reference.Enum;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCode_Remote.Library.Tools;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;

namespace TCode_Remote.Library.Handler
{
	public class TcpHandler : DeviceHandler, IDevice
	{

		private string _address;
		private int _port;
		private TCodeFactory _tcodeFactory;
		private Socket _sListener;
		IPEndPoint _ipEndPoint;

		public TcpHandler(OSRRemoteDeviceMode mode)
		{
			_mode = mode;
		}

		public void Init(IDeviceInit model)
		{
			_address = model.Address;
			_port = ((NetworkInitModel)model).Port;
			_tcodeFactory = new TCodeFactory(0.0, 1.0);
			_isRunning = true;
			UpdateConnected(false, SettingsHandler.DEVICE_CONNECT);
			if (_mode == OSRRemoteDeviceMode.In)
			{
				InitServer();
			} 
			else
			{
				InitiateClientConnection();
			}
		}

		private void Device_DataReceived(object sender, DataReceivedEventArgs e)
		{
			SendToOutputClient(e.Data);
		}

		private void InitiateClientConnection()
		{
			Task.Run(() =>
			{
				while(!IsConnected && IsRunning)
				{
					try
					{
						SendToOutputClient(SettingsHandler.HandShakeChannel);
						StartHeartBeat();
						return;
					} 
					catch
					{
						Thread.Sleep(TimeSpan.FromSeconds(5));
					}
				}
			});
		}

		private void StartHeartBeat()
		{
			Task.Run(() =>
			{
				while (IsRunning)
				{
					try
					{
						Thread.Sleep(TimeSpan.FromSeconds(10));
						SendToOutputClient(SettingsHandler.HandShakeChannel);
					}
					catch
					{
						UpdateConnected(false);
					}
				}
			});
		}

		private static IPAddress PingAddress(string hostName)
		{
			Ping ping = new Ping();
			var replay = ping.Send(hostName);

			if (replay.Status == IPStatus.Success)
			{
				return replay.Address;
			}
			return null;
		}

		private void GamepadDevice_DataReceived(object sender, DataReceivedEventArgs e)
		{
			if (IsRunning)
			{
				try
				{
					var serializedstring = _serializer.Serialize(e.GamepadData);
					Log.Debug("Trying to send: " + serializedstring);
					if (IsRunning)
						SendToOutputClient(serializedstring);
				}
				catch (Exception error)
				{
					Log.Dialog("Error sending gamepad data to tcp client.", error.Message);
				}
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
			UpdateConnected(false);
			Stop();
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

		private async Task InitServer()
		{
			// establish the local end point for the socket 
			_ipEndPoint = new IPEndPoint(IPAddress.Any, _port);

			// create a Tcp/Ip Socket 
			_sListener = new Socket(AddressFamily.InterNetwork,
										  SocketType.Stream, ProtocolType.Tcp);
			ConfigureTcpSocket(_sListener);
			// bind the socket to the local endpoint and 
			// listen to the incoming sockets 
			_sListener.Bind(_ipEndPoint);
			_sListener.Listen(10);
			await StartInputServer();
		}

		private async Task StartInputServer()
		{
			await Task.Run(() =>
			{

				try
				{
					while (IsRunning)
					{
						var returnData = RecieveData();

						if (returnData.Contains(SettingsHandler.HandShakeChannel))
						{
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
					UpdateConnected(false);
					Log.Dialog("Server error", e.ToString());
				}
			});
		}

		private string RecieveData()
		{
			// Start listening for connections 
			Log.Debug($"Waiting for a connection on port {_ipEndPoint}");
			// program is suspended while waiting for an incoming connection 
			var socketHandler = _sListener.Accept();
			UpdateConnected(true);
			Log.Debug($"Connected to {_ipEndPoint}");
			string returnData = "";

			// we got the client attempting to connect 
			//while (IsRunning && data.Length <= 240)
			//{
			byte[] bytes = new byte[2048];

			int bytesRec = socketHandler.Receive(bytes);

			returnData += Encoding.ASCII.GetString(bytes, 0, bytesRec);

			//}

			// show the data on the console 
			Log.Debug($"{DateTime.Now} Text Received: {returnData}");
			if (returnData.Contains(SettingsHandler.HandShakeChannel))
			{
				byte[] msg = Encoding.ASCII.GetBytes(SettingsHandler.TCodeVersion);

				socketHandler.Send(msg);
			}
			socketHandler.Shutdown(SocketShutdown.Both);
			socketHandler.Close();
			return returnData;
		}

		private void SendToOutputClient(string message)
		{
			try
				{
				TcpClient client = new TcpClient(_address, _port);
				ConfigureTcpSocket(client.Client);
				// Translate the passed message into ASCII and store it as a Byte array.
				var data = Encoding.ASCII.GetBytes(message);

				// Get a client stream for reading and writing.
				NetworkStream stream = client.GetStream();

				// Send the message to the connected TcpServer.
				stream.Write(data, 0, data.Length);

				Log.Debug("Sent: " + message);

				// Buffer to store the response bytes.
				data = new Byte[256];

				// String to store the response ASCII representation.
				String responseData = String.Empty;

				// Read the first batch of the TcpServer response bytes.
				Int32 bytes = stream.Read(data, 0, data.Length);
				responseData = Encoding.ASCII.GetString(data, 0, bytes);
				Log.Debug("Send Reply: " + responseData);

				if (responseData.Contains(SettingsHandler.TCodeVersion)) 
				{
					UpdateConnected(true);
				}

				// Close everything.
				stream.Close();
				client.Close();
			}
			catch (ArgumentNullException e)
			{
				Log.Dialog("ArgumentNullException", e.Message);
				UpdateConnected(false);
				throw;
			}
			catch (SocketException e)
			{
				Log.Debug("SocketException:" + e.Message);
				UpdateConnected(false);
				throw;
			}
		}

		private void ConfigureTcpSocket(Socket tcpSocket)
		{
			// Don't allow another socket to bind to this port.
			//tcpSocket.ExclusiveAddressUse = true;

			// The socket will linger for 10 seconds after
			// Socket.Close is called.
			tcpSocket.LingerState = new LingerOption(true, 10);

			// Disable the Nagle Algorithm for this tcp socket.
			tcpSocket.NoDelay = true;

			// Set the receive buffer size to 8k
			tcpSocket.ReceiveBufferSize = 8192;

			// Set the timeout for synchronous receive methods to
			// 1 second (1000 milliseconds.)
			tcpSocket.ReceiveTimeout = 1000;

			// Set the send buffer size to 8k.
			tcpSocket.SendBufferSize = 8192;

			// Set the timeout for synchronous send methods
			// to 1 second (1000 milliseconds.)
			tcpSocket.SendTimeout = 1000;

			// Set the Time To Live (TTL) to 42 router hops.
			tcpSocket.Ttl = 42;
		}
	}
}

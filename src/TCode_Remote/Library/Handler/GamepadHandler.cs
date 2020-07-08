using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Library.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using TCode_Remote.Library.Events;
using TCode_Remote.Library.Tools;
using System.Collections.Generic;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Reference;
using System.Linq;

namespace TCode_Remote.Library.Handler
{
	public class GamepadHandler : DeviceHandler, IDevice
	{
		private ObservableCollection<Gamepad> GamePads { get { return _gamePads; } }
		private ObservableCollection<Gamepad> _gamePads;
		private Gamepad _selectedGamePad;
		private double _deadzone = 0.2;
		private TCodeFactory _tcodeFactory;

		public GamepadHandler()
		{
			_mode = OSRRemoteDeviceMode.In;
			_tcodeFactory = new TCodeFactory(0.0, 1.0);
			_gamePads = new ObservableCollection<Gamepad>();
			Gamepad.GamepadAdded += Gamepad_GamepadAdded;
			Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
			UpdateConnected(_isConnected);
		}

		public void Init(IDeviceInit deviceInit)
		{
		}

		public async Task SetIODeviceAsync(IDevice device)
		{
			_device = device;
			_isRunning = true;
			if (_mode == OSRRemoteDeviceMode.In)
			{
				await GetTCodeFromGamepadreading();
			}
		}

		public void Stop()
		{
			_isRunning = false;
		}

		public void Dispose()
		{
			Stop();
		}

		private async void Gamepad_GamepadRemoved(object sender, Gamepad e)
		{
			await Task.Run(() =>
			{
				_gamePads.Remove(e);
				if (_selectedGamePad == e)
				{
					_selectedGamePad = null;
					UpdateConnected(false);
				}
			});
			OnPropertyChanged(this, GamePads);
		}

		private async void Gamepad_GamepadAdded(object sender, Gamepad e)
		{
			await Task.Run(() =>
			{
				_gamePads.Add(e);
				if (_selectedGamePad == null)
				{
					_selectedGamePad = e;
					UpdateConnected(true);
				}
			});
			OnPropertyChanged(this, GamePads);
		}
	
		private async Task GetTCodeFromGamepadreading()
		{
			await Task.Run(() =>
			{
				while (IsRunning)
				{
					_tcodeFactory.Init();
					GamepadReading reading = _selectedGamePad.GetCurrentReading();
					var axisValues = new HashSet<ChannelValueModel>();
					_tcodeFactory.Calculate(GamepadAxisNames.LeftXAxis, CalculateDeadZone(reading.LeftThumbstickX), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.LeftYAxis, CalculateDeadZone(reading.LeftThumbstickY), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.RightXAxis, CalculateDeadZone(reading.RightThumbstickX), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.RightYAxis, CalculateDeadZone(reading.RightThumbstickY), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.RightTrigger, reading.RightTrigger, axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.LeftTrigger, reading.LeftTrigger, axisValues);
					// Binary inputs
					_tcodeFactory.Calculate(GamepadAxisNames.A, GamepadButtons.A == (reading.Buttons & GamepadButtons.A), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.B, GamepadButtons.B == (reading.Buttons & GamepadButtons.B), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.X, GamepadButtons.X == (reading.Buttons & GamepadButtons.X), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.Y, GamepadButtons.Y == (reading.Buttons & GamepadButtons.Y), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.RightBumper, GamepadButtons.RightShoulder == (reading.Buttons & GamepadButtons.RightShoulder), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.LeftBumper, GamepadButtons.LeftShoulder == (reading.Buttons & GamepadButtons.LeftShoulder), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.Start, GamepadButtons.View == (reading.Buttons & GamepadButtons.View), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.Select, GamepadButtons.Menu == (reading.Buttons & GamepadButtons.Menu), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.DPadUp, GamepadButtons.DPadUp == (reading.Buttons & GamepadButtons.DPadUp), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.DPadDown, GamepadButtons.DPadDown == (reading.Buttons & GamepadButtons.DPadDown), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.DPadLeft, GamepadButtons.DPadLeft == (reading.Buttons & GamepadButtons.DPadLeft), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.DPadRight, GamepadButtons.DPadRight == (reading.Buttons & GamepadButtons.DPadRight), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.RightAxisButton, GamepadButtons.RightThumbstick == (reading.Buttons & GamepadButtons.RightThumbstick), axisValues);
					_tcodeFactory.Calculate(GamepadAxisNames.LeftAxisButton, GamepadButtons.LeftThumbstick == (reading.Buttons & GamepadButtons.LeftThumbstick), axisValues);

					EventHandler<DataReceivedEventArgs> handler = DataRecieved;
					if (handler != null)
					{
						var eventArgs = new DataReceivedEventArgs();
						if (_device.GetType() == typeof(TcpHandler) || _device.GetType() == typeof(UdpHandler))
						{
							eventArgs.GamepadData = axisValues;
						} 
						else
						{
							eventArgs.Data = _tcodeFactory.FormatTCode(axisValues);
						}
						handler(this, eventArgs);
					}
				}
			});
		}

		private double CalculateDeadZone(double gpIn)
		{
			if (gpIn < _deadzone && gpIn > 0 || gpIn > -_deadzone && gpIn < 0)
			{
				return 0;
			}
			return gpIn;
		}
	}
}

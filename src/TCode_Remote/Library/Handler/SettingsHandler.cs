using TCode_Remote.Library.Model;
using TCode_Remote.Library.Reference;
using TCode_Remote.Library.Reference.Constants;
using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Script.Serialization;
using TCode_Remote.Library.Tools;
using System.Reflection;

namespace TCode_Remote.Library.Handler
{
	public static class SettingsHandler
	{
		public const string DEVICE_CONNECT = "Searching";
		public static event EventHandler PropertyChanged;

		private static bool _isDebugMode;
		public static bool IsDebugMode
		{
			get { return _isDebugMode; }
			set
			{
				_isDebugMode = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(IsDebugMode)));
			}
		}
		private static OSRRemoteInputMode _inputDevice;
		public static OSRRemoteInputMode InputDevice
		{
			get { return _inputDevice; }
			set
			{
				Settings.Default.InputDevice = value.ToString();
				_inputDevice = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InputDevice)));
			}
		}

		private static OSRRemoteOutputMode _outputDevice;
		public static OSRRemoteOutputMode OutputDevice
		{
			get { return _outputDevice; }
			set
			{
				Settings.Default.OutputDevice = value.ToString();
				_outputDevice = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(OutputDevice)));
			}
		}

		private static Dictionary<string, int> _tCodeOutputRanges;
		public static Dictionary<string, int> TCodeOutputRanges
		{
			get { return _tCodeOutputRanges; } 
			private set
			{
				_tCodeOutputRanges = value;
				SerializeMinMaxSettings();
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(TCodeOutputRanges)));
			}
		}

		private static Dictionary<string, string> _gamepadButtonMap;
		public static Dictionary<string, string> GamepadButtonMap
		{
			get { return _gamepadButtonMap; }
			private set
			{
				_gamepadButtonMap = value;
				SerializeGamepadMap();
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(GamepadButtonMap)));
			}
		}
		
		private static string _inputAddress;
		public static string InputAddress
		{
			get { return _inputAddress; }
			set
			{
				Settings.Default.InputAddress = value;
				_inputAddress = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InputAddress)));
			}
		}

		private static string _inputAddressPort;
		public static string InputAddressPort
		{
			get { return _inputAddressPort; }
			set
			{
				Settings.Default.InputAddressPort = value;
				_inputAddressPort = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InputAddressPort)));
			}
		}

		private static string _outputAddress;
		public static string OutputAddress
		{
			get { return _outputAddress; }
			set
			{
				Settings.Default.OutputAddress = value;
				_outputAddress = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(OutputAddress)));
			}
		}

		private static string _outputAddressPort;
		public static string OutputAddressPort
		{
			get { return _outputAddressPort; }
			set
			{
				Settings.Default.OutputAddressPort = value;
				_outputAddressPort = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(OutputAddressPort)));
			}
		}

		private static string _inputSerialPort;
		public static string InputSerialPort
		{
			get { return _inputSerialPort; }
			set
			{
				Settings.Default.InputSerialPort = value;
				_inputSerialPort = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InputSerialPort)));
			}
		}

		private static string _outputSerialPort;
		public static string OutputSerialPort
		{
			get { return _outputSerialPort; }
			set
			{
				Settings.Default.OutputSerialPort = value;
				_outputSerialPort = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(OutputSerialPort)));
			}
		}

		private static bool _inverseTcYRollR1;
		public static bool InverseTcYRollR1
		{
			get { return _inverseTcYRollR1; }
			set
			{
				Settings.Default.InverseTcYRollR1 = value;
				_inverseTcYRollR1 = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InverseTcYRollR1)));
			}
		}

		private static bool _inverseTcXL0;
		public static bool InverseTcXL0
		{
			get { return _inverseTcXL0; }
			set
			{
				Settings.Default.InverseTcXL0 = value;
				_inverseTcXL0 = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InverseTcXL0)));
			}
		}

		private static bool _inverseTcXRollR2;
		public static bool InverseTcXRollR2
		{
			get { return _inverseTcXRollR2; }
			set
			{
				Settings.Default.InverseTcXRollR2 = value;
				_inverseTcXRollR2 = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InverseTcXRollR2)));
			}
		}

		private static bool _inverseTcTwistR0;
		public static bool InverseTcTwistR0
		{
			get { return _inverseTcTwistR0; }
			set
			{
				Settings.Default.InverseTcTwistR0 = value;
				_inverseTcTwistR0 = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(InverseTcTwistR0)));
			}
		}

		private static int _speed;
		public static int Speed
		{
			get { return _speed; }
			set
			{
				Settings.Default.Speed = value;
				_speed = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Speed)));
			}
		}

		private static bool _useUDP;
		public static bool UseUDP
		{
			get { return _useUDP; }
			set
			{
				Settings.Default.UseUDP = value;
				_useUDP = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(UseUDP)));
			}
		}

		private static Dictionary<string, ChannelNameModel> _availableAxis;
		public static Dictionary<string, ChannelNameModel> AvailableAxis
		{
			get { return _availableAxis; }
			set
			{
				_availableAxis = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(AvailableAxis)));
			}
		}

		private static string _tCodeVersion;
		public static string TCodeVersion
		{
			get { return _tCodeVersion; }
			set
			{
				_tCodeVersion = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(TCodeVersion)));
			}
		}


		private static bool _showBLEMessage;
		public static bool ShowBLEMessage
		{
			get { return _showBLEMessage; }
			set
			{
				Settings.Default.ShowBLEMessage = value;
				_showBLEMessage = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(ShowBLEMessage)));
			}
		}


		private static bool _dataResetRequired = false;

		static SettingsHandler()
		{
			try
			{
				var versionFull = GetVersion();
				var versionDouble = versionFull.Major + (versionFull.Minor / 100.00);
				if (Settings.Default.Version == 0)
				{
					ResetSettings();
					Settings.Default.Version = versionDouble;
				} 
				else if (Settings.Default.Version < versionDouble && _dataResetRequired)
				{
					ResetSettings();
					Log.Dialog("New version.", " New version detected and a data migration is required.\n Resetting user settings.");
					Settings.Default.Version = versionDouble;
				}

				_tCodeOutputRanges = new JavaScriptSerializer().Deserialize<Dictionary<string, int>>(Settings.Default.TCodeOutputRanges);
				_availableAxis = new JavaScriptSerializer().Deserialize<Dictionary<string, ChannelNameModel>>(Settings.Default.AvailableAxis);
				_gamepadButtonMap = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(Settings.Default.GamepadButtonMap);
				_inputAddress = Settings.Default.InputAddress;
				_inputAddressPort = Settings.Default.InputAddressPort;
				_outputAddress = Settings.Default.OutputAddress;
				_outputAddressPort = Settings.Default.OutputAddressPort;
				_inputSerialPort = Settings.Default.InputSerialPort;
				_outputSerialPort = Settings.Default.OutputSerialPort;
				_inverseTcTwistR0 = Settings.Default.InverseTcTwistR0;
				_inverseTcXRollR2 = Settings.Default.InverseTcXRollR2;
				_inverseTcXL0 = Settings.Default.InverseTcXL0;
				_inverseTcYRollR1 = Settings.Default.InverseTcYRollR1;
				_speed = Settings.Default.Speed;
				_useUDP = Settings.Default.UseUDP;
				_tCodeVersion = Settings.Default.TCodeVersion;
				_inputDevice = string.IsNullOrEmpty(Settings.Default.InputDevice) ? OSRRemoteInputMode.Gamepad :
					(OSRRemoteInputMode)Enum.Parse(typeof(OSRRemoteInputMode), Settings.Default.InputDevice);
				_outputDevice = string.IsNullOrEmpty(Settings.Default.OutputDevice) ? OSRRemoteOutputMode.Serial :
					(OSRRemoteOutputMode)Enum.Parse(typeof(OSRRemoteOutputMode), Settings.Default.OutputDevice);
				_showBLEMessage = Settings.Default.ShowBLEMessage;
			} 
			catch(Exception e)
			{
				ResetSettings();
				Log.Dialog("Error loading preferences.", " Sorry but your settings have been reset, please restart the app");
				Log.Error("Error loading preferences: "+ e.Message);
			}
		}

		public static Version GetVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version;
		}

		public static void Save()
		{
			SerializeMinMaxSettings();
			SerializeAvailableAxis();
			SerializeGamepadMap();
			Settings.Default.Save();
			Settings.Default.Reload();
		}


		private static void SerializeMinMaxSettings()
		{
			Settings.Default.TCodeOutputRanges = new JavaScriptSerializer().Serialize(_tCodeOutputRanges);
		}

		private static void SerializeGamepadMap()
		{
			Settings.Default.GamepadButtonMap = new JavaScriptSerializer().Serialize(_gamepadButtonMap);
		}

		private static void SerializeAvailableAxis()
		{
			Settings.Default.AvailableAxis = new JavaScriptSerializer().Serialize(_availableAxis);
		}

		private static void SetupNewRangesDictionary()
		{
			_tCodeOutputRanges = new Dictionary<string, int>
			{
				{ $"{AxisNames.TcXUpDownL0}Mid", 500 },
				{ $"{AxisNames.TcXUpDownL0}Max", 999 },
				{ $"{AxisNames.TcXUpDownL0}Min", 1 },
				{ $"{AxisNames.TcYRollR1}Mid", 500 },
				{ $"{AxisNames.TcYRollR1}Max", 999 },
				{ $"{AxisNames.TcYRollR1}Min", 1 },
				{ $"{AxisNames.TcXRollR2}Mid", 500 },
				{ $"{AxisNames.TcXRollR2}Max", 999 },
				{ $"{AxisNames.TcXRollR2}Min", 1 },
				{ $"{AxisNames.TcTwistR0}Mid", 500 },
				{ $"{AxisNames.TcTwistR0}Max", 520 },
				{ $"{AxisNames.TcTwistR0}Min", 480 },
				{ $"{AxisNames.TcVibV0}Mid", 500 },
				{ $"{AxisNames.TcVibV0}Max", 999 },
				{ $"{AxisNames.TcVibV0}Min", 1 }
			};
		}

		private static void SetupAvailableAxis()
		{
			_availableAxis = new Dictionary<string, ChannelNameModel>()
			{
				{AxisNames.None, new ChannelNameModel() { FriendlyName = AxisNames.None, AxisName = AxisNames.None, Channel = AxisNames.None, Start = 500, End = 999 } },
				{AxisNames.TcXUpDownL0, new ChannelNameModel() { FriendlyName = "X (Up/down L0)", AxisName = AxisNames.TcXUpDownL0, Channel = "L0", Start = 500, End = 999 } },
				{AxisNames.TcXDownL0, new ChannelNameModel() { FriendlyName = "X (Down)", AxisName = AxisNames.TcXDownL0, Channel = "L0", Start = 500, End = 999} },
				{AxisNames.TcXUpL0, new ChannelNameModel() { FriendlyName = "X (Up)", AxisName = AxisNames.TcXUpL0, Channel = "L0", Start = 500, End = 999 } },
				{AxisNames.TcXRollR2, new ChannelNameModel() { FriendlyName = "X (Roll R2)", AxisName = AxisNames.TcXRollR2, Channel = "R2", Start = 500, End = 999 } },
				{AxisNames.TcXRollForwardR2, new ChannelNameModel() { FriendlyName = "X (Roll Forward)", AxisName = AxisNames.TcXRollForwardR2, Channel = "R2", Start = 500, End = 999 } },
				{AxisNames.TcXRollBackR2, new ChannelNameModel() { FriendlyName = "X (Roll Back)", AxisName = AxisNames.TcXRollBackR2, Channel = "R2", Start = 500, End = 999 } },
				{AxisNames.TcYRollR1, new ChannelNameModel() { FriendlyName = "Y (Roll R1)", AxisName = AxisNames.TcYRollR1, Channel = "R1", Start = 500, End = 999 } },
				{AxisNames.TcYRollLeftR1, new ChannelNameModel() { FriendlyName = "Y (Roll Left)", AxisName = AxisNames.TcYRollLeftR1, Channel = "R1", Start = 500, End = 999} },
				{AxisNames.TcYRollRightR1, new ChannelNameModel() { FriendlyName = "Y (Roll Right)", AxisName = AxisNames.TcYRollRightR1, Channel = "R1", Start = 500, End = 999 } },
				{AxisNames.TcTwistR0, new ChannelNameModel() { FriendlyName = "Twist R0", AxisName = AxisNames.TcTwistR0, Channel = "R0", Start = 500, End = 520 } },
				{AxisNames.TcTwistCWR0, new ChannelNameModel() { FriendlyName = "Twist (CW)", AxisName = AxisNames.TcTwistCWR0, Channel = "R0", Start = 500, End = 520 } },
				{AxisNames.TcTwistCCWR0, new ChannelNameModel() { FriendlyName = "Twist (CCW)", AxisName = AxisNames.TcTwistCCWR0, Channel = "R0", Start = 500, End = 520 } },
				{AxisNames.TcVibV0, new ChannelNameModel() { FriendlyName = "Vib V0", AxisName = AxisNames.TcVibV0, Channel = "V0", Start = 500, End = 999 } },
				{AxisNames.TcPumpV2, new ChannelNameModel() { FriendlyName = "Pump V2", AxisName = AxisNames.TcPumpV2, Channel = "V2", Start = 500, End = 999 } }
			};
		}

		private static void SetupGamepadButtonMapDictionary()
		{
			_gamepadButtonMap = new Dictionary<string, string>
			{
				{ "None", AxisNames.None },
				{ GamepadAxisNames.LeftXAxis, AxisNames.TcVibV0 },
				{ GamepadAxisNames.LeftYAxis,  AxisNames.TcXUpDownL0 },
				{ GamepadAxisNames.RightYAxis ,  AxisNames.TcXRollR2  },
				{ GamepadAxisNames.RightXAxis, AxisNames.TcYRollR1  },
				{ GamepadAxisNames.RightTrigger, AxisNames.TcTwistCWR0 },
				{ GamepadAxisNames.LeftTrigger, AxisNames.TcTwistCCWR0 },
				{ GamepadAxisNames.RightBumper, AxisNames.None },
				{ GamepadAxisNames.LeftBumper, AxisNames.None },
				{ GamepadAxisNames.Select, AxisNames.None },
				{ GamepadAxisNames.Start, AxisNames.None },
				{ GamepadAxisNames.X, AxisNames.None },
				{ GamepadAxisNames.Y, AxisNames.None },
				{ GamepadAxisNames.B, AxisNames.None },
				{ GamepadAxisNames.A, AxisNames.None },
				{ GamepadAxisNames.DPadUp, AxisNames.None },
				{ GamepadAxisNames.DPadDown, AxisNames.None },
				{ GamepadAxisNames.DPadLeft, AxisNames.None },
				{ GamepadAxisNames.DPadRight, AxisNames.None },
				{ GamepadAxisNames.RightAxisButton, AxisNames.None },
				{ GamepadAxisNames.LeftAxisButton, AxisNames.None }
			};
		}
		public static void ResetSettings()
		{
			Settings.Default.Reset();
			SetupNewRangesDictionary();
			SetupAvailableAxis();
			SetupGamepadButtonMapDictionary();
			Save();
		}
	}
}

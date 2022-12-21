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

		public static readonly int TCODE_MAX = 9999;
		public static readonly int TCODE_MID = 5000;
		public static readonly int TCODE_MIN = 0;

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
		public static string AppVersion
		{
			get { return $"App Version: {Settings.Default.Version}"; }
		}

		public static string HandShakeChannel
		{
			get { return "D1\n"; }
		}

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

		private static bool _useUDPOutput;
		public static bool UseUDPOutput
		{
			get { return _useUDPOutput; }
			set
			{
				Settings.Default.UseUDPOutput = value;
				_useUDPOutput = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(UseUDPOutput)));
			}
		}

		private static bool _useUDPInput;
		public static bool UseUDPInput
		{
			get { return _useUDPInput; }
			set
			{
				Settings.Default.UseUDPInput = value;
				_useUDPInput = value;
				PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(UseUDPInput)));
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
				else if(Settings.Default.Version < 0.16)
				{
					ResetSettings();
					Log.Dialog("New version.", "Resetting user settings for Tcode V0.3");
					Settings.Default.Version = versionDouble;
				}
				

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
				_useUDPOutput = Settings.Default.UseUDPOutput;
				_useUDPInput = Settings.Default.UseUDPInput;
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
			SerializeAvailableAxis();
			SerializeGamepadMap();
			Settings.Default.Save();
			Settings.Default.Reload();
		}

		private static void SerializeGamepadMap()
		{
			Settings.Default.GamepadButtonMap = new JavaScriptSerializer().Serialize(_gamepadButtonMap);
		}

		private static void SerializeAvailableAxis()
		{
			Settings.Default.AvailableAxis = new JavaScriptSerializer().Serialize(_availableAxis);
		}

		private static void SetupAvailableAxis()
		{
			_availableAxis = new Dictionary<string, ChannelNameModel>()
			{
				{AxisNames.None, new ChannelNameModel() { FriendlyName = AxisNames.None, AxisName = AxisNames.None, Channel = AxisNames.None, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Stroke, new ChannelNameModel() { FriendlyName = "Stroke", AxisName = AxisNames.Stroke, Channel = AxisNames.Stroke, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.StrokeDown, new ChannelNameModel() { FriendlyName = "Stroke (Down)", AxisName = AxisNames.StrokeDown, Channel = AxisNames.Stroke, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.StrokeUp, new ChannelNameModel() { FriendlyName = "Stroke (Up)", AxisName = AxisNames.StrokeUp, Channel = AxisNames.Stroke, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Sway, new ChannelNameModel() { FriendlyName = "Sway", AxisName = AxisNames.Sway, Channel = AxisNames.Sway, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.SwayLeft, new ChannelNameModel() { FriendlyName = "Sway Left", AxisName = AxisNames.SwayLeft, Channel = AxisNames.Sway, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.SwayRight, new ChannelNameModel() { FriendlyName = "Sway Right", AxisName = AxisNames.SwayRight, Channel = AxisNames.Sway, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Surge, new ChannelNameModel() { FriendlyName = "Surge", AxisName = AxisNames.Surge, Channel = AxisNames.Surge, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.SurgeBack, new ChannelNameModel() { FriendlyName = "Surge Back", AxisName = AxisNames.SurgeBack, Channel = AxisNames.Surge, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.SurgeForward, new ChannelNameModel() { FriendlyName = "Surge Forward", AxisName = AxisNames.SurgeForward, Channel = AxisNames.Surge, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Pitch, new ChannelNameModel() { FriendlyName = "Pitch", AxisName = AxisNames.Pitch, Channel = AxisNames.Pitch, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.PitchForward, new ChannelNameModel() { FriendlyName = "Pitch Forward", AxisName = AxisNames.PitchForward, Channel = AxisNames.Pitch, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.PitchBack, new ChannelNameModel() { FriendlyName = "Pitch Back", AxisName = AxisNames.PitchBack, Channel = AxisNames.Pitch, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Roll, new ChannelNameModel() { FriendlyName = "Roll", AxisName = AxisNames.Roll, Channel = AxisNames.Roll, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.RollLeft, new ChannelNameModel() { FriendlyName = "Roll Left", AxisName = AxisNames.RollLeft, Channel = AxisNames.Roll, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.RollRight, new ChannelNameModel() { FriendlyName = "Roll Right", AxisName = AxisNames.RollRight, Channel = AxisNames.Roll, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Twist, new ChannelNameModel() { FriendlyName = "Twist", AxisName = AxisNames.Twist, Channel = AxisNames.Twist, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.TwistCW, new ChannelNameModel() { FriendlyName = "Twist (CW)", AxisName = AxisNames.TwistCW, Channel = AxisNames.Twist, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.TwistCCW, new ChannelNameModel() { FriendlyName = "Twist (CCW)", AxisName = AxisNames.TwistCCW, Channel = AxisNames.Twist, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Vib0, new ChannelNameModel() { FriendlyName = "Vib 0", AxisName = AxisNames.Vib0, Channel = AxisNames.Vib0, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Lube, new ChannelNameModel() { FriendlyName = "Lube", AxisName = AxisNames.Lube, Channel = AxisNames.Lube, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Squeeze, new ChannelNameModel() { FriendlyName = "Squeeze", AxisName = AxisNames.Squeeze, Channel = AxisNames.Squeeze, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.Suck, new ChannelNameModel() { FriendlyName = "Suck", AxisName = AxisNames.Suck, Channel = AxisNames.Suck, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } },
				{AxisNames.SuckLevel, new ChannelNameModel() { FriendlyName = "Suck Level", AxisName = AxisNames.SuckLevel, Channel = AxisNames.SuckLevel, Mid = TCODE_MID, Min = TCODE_MIN, Max = TCODE_MAX } }
			};
		}

		private static void SetupGamepadButtonMapDictionary()
		{
			_gamepadButtonMap = new Dictionary<string, string>
			{
				{ "None", AxisNames.None },
				{ GamepadAxisNames.LeftXAxis, AxisNames.Twist },
				{ GamepadAxisNames.LeftYAxis,  AxisNames.Stroke },
				{ GamepadAxisNames.RightYAxis , AxisNames.Pitch  },
				{ GamepadAxisNames.RightXAxis, AxisNames.Roll  },
				{ GamepadAxisNames.RightTrigger, AxisNames.SurgeForward },
				{ GamepadAxisNames.LeftTrigger, AxisNames.SurgeBack },
				{ GamepadAxisNames.RightBumper, AxisNames.SwayRight },
				{ GamepadAxisNames.LeftBumper, AxisNames.SwayLeft },
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
			SetupAvailableAxis();
			SetupGamepadButtonMapDictionary();
			Save();
		}
	}
}

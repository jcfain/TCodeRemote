﻿using TCode_Remote.Library.Reference.Enum;
using TCode_Remote.Library.Handler;
using TCode_Remote.Library.Interfaces;
using System.Windows;
using System.Windows.Controls;
using TCode_Remote.Library.Reference.Constants;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Tools;
using System;
using TCode_Remote.Library.Events;
using TCode_Remote.Library.Extension;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;
using System.Net.Sockets;

namespace TCode_Remote
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public GamepadHandler GamepadHandler;
		public SerialDeviceHandler InputSerialDeviceHandler { get; set; }
		public SerialDeviceHandler OutputSerialDeviceHandler { get; set; }
		public BLEDeviceHandler BLEDeviceHandler { get; set; }
		public TcpHandler InputTcpAddresshandler { get; set; }
		public TcpHandler OutputTcpAddresshandler { get; set; }
		public UdpHandler InputUdpAddresshandler { get; set; }
		public UdpHandler OutputUdpAddresshandler { get; set; }
		public ObservableCollection<SerialDeviceModel> SerialPorts { get; set; }
		public ObservableCollection<DeviceModel> BLEDevices { get; set; }

		private IDevice _inputDevice;
		private IDevice _outputDevice;
		private Debouncer _outputNetworkAddressTxtDebouncer;
		private Debouncer _inputNetworkPortTxtDebouncer;
		private bool _appRunning = false;

		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			Dispatcher.UnhandledException += Dispatcher_UnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;

			GamepadHandler = new GamepadHandler();
			GamepadHandler.ConnectionChange += GamepadConnected_Changed;
			InputSerialDeviceHandler = new SerialDeviceHandler(OSRRemoteDeviceMode.In);
			InputSerialDeviceHandler.ConnectionChange += SerialConnected_Changed;
			OutputSerialDeviceHandler = new SerialDeviceHandler(OSRRemoteDeviceMode.Out);
			OutputSerialDeviceHandler.ConnectionChange += SerialConnected_Changed;
			SerialPorts = OutputSerialDeviceHandler.GetPorts();
			BLEDevices = new ObservableCollection<DeviceModel>();
			InputTcpAddresshandler = new TcpHandler(OSRRemoteDeviceMode.In);
			InputTcpAddresshandler.ConnectionChange += NetworkConnected_Changed;
			OutputTcpAddresshandler = new TcpHandler(OSRRemoteDeviceMode.Out);
			OutputTcpAddresshandler.ConnectionChange += NetworkConnected_Changed;
			InputUdpAddresshandler = new UdpHandler(OSRRemoteDeviceMode.In);
			InputUdpAddresshandler.ConnectionChange += NetworkConnected_Changed;
			OutputUdpAddresshandler = new UdpHandler(OSRRemoteDeviceMode.Out);
			OutputUdpAddresshandler.ConnectionChange += NetworkConnected_Changed;
			//BLEDeviceHandler = new BLEDeviceHandler(OSRRemoteDeviceMode.Out);
			//BLEDeviceHandler.ConnectionChange += BLEDeviceHandler_ConnectionChanged;
			//BLEDeviceHandler.DeviceAdded += BLEDeviceAdded;
			//BLEDeviceHandler.DeviceRemoved += BLEDeviceRemoved;



			InitializeComponent();

			this.DataContext = this;
			IPAddressLabel.Content = GetLocalIPV4Address();

			RestoreSavedSettings();

			_outputNetworkAddressTxtDebouncer = new Debouncer();
			_outputNetworkAddressTxtDebouncer.Idled += OutputNetworkAddressTxtDebouncer_Idled;
			_inputNetworkPortTxtDebouncer = new Debouncer();
			_inputNetworkPortTxtDebouncer.Idled += InputNetworkPortTxtDebouncer_Idled;

			//BLERdo.Checked += OutRadio_Checked;

		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_appRunning = false;
			Log.Error("Unhandled Domain exception" + ((Exception)e.ExceptionObject).Message);
			Log.Error("Stacktrace" + ((Exception)e.ExceptionObject).StackTrace);
		}

		private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			_appRunning = false;
			Log.Error("Unhandled exception" + e.Exception.Message);
			Log.Error("Stacktrace" + e.Exception.StackTrace);
		}

		public void OnPropertyChanged(object obj)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(obj)));
		} 

		private void RestoreSavedSettings()
		{
			XRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Stroke).Mid;
			XRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Stroke).Min;
			XRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Stroke).Max;
			LoadSliderUI(true, XRangeSlider, XRangeMin, XRangeMax);
			LoadSliderUI(false, XRangeSlider, XRangeMin, XRangeMax);
			XRangeMinLabel.Content = XRangeSlider.SelectionStart;
			XRangeMaxLabel.Content = XRangeSlider.SelectionEnd;

			YRollRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Roll).Mid;
			YRollRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Roll).Min;
			YRollRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Roll).Max;
			LoadSliderUI(true, YRollRangeSlider, YRollRangeMin, YRollRangeMax);
			LoadSliderUI(false, YRollRangeSlider, YRollRangeMin, YRollRangeMax);
			YRollRangeMinLabel.Content = YRollRangeSlider.SelectionStart;
			YRollRangeMaxLabel.Content = YRollRangeSlider.SelectionEnd;

			XRollRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Pitch).Mid;
			XRollRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Pitch).Min;
			XRollRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Pitch).Max;
			LoadSliderUI(true, XRollRangeSlider, XRollRangeMin, XRollRangeMax);
			LoadSliderUI(false, XRollRangeSlider, XRollRangeMin, XRollRangeMax);
			XRollRangeMinLabel.Content = XRollRangeSlider.SelectionStart;
			XRollRangeMaxLabel.Content = XRollRangeSlider.SelectionEnd;

			SwayRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Sway).Mid;
			SwayRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Sway).Min;
			SwayRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Sway).Max;
			LoadSliderUI(true, SwayRangeSlider, SwayRangeMin, SwayRangeMax);
			LoadSliderUI(false, SwayRangeSlider, SwayRangeMin, SwayRangeMax);
			SwayRangeMinLabel.Content = SwayRangeSlider.SelectionStart;
			SwayRangeMaxLabel.Content = SwayRangeSlider.SelectionEnd;

			SurgeRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Surge).Mid;
			SurgeRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Surge).Min;
			SurgeRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Surge).Max;
			LoadSliderUI(true, SurgeRangeSlider, SurgeRangeMin, SurgeRangeMax);
			LoadSliderUI(false, SurgeRangeSlider, SurgeRangeMin, SurgeRangeMax);
			SurgeRangeMinLabel.Content = SurgeRangeSlider.SelectionStart;
			SurgeRangeMaxLabel.Content = SurgeRangeSlider.SelectionEnd;

			TwistRangeSlider.Value = SettingsHandler.AvailableAxis.GetValue(AxisNames.Twist).Mid;
			TwistRangeSlider.SelectionStart = SettingsHandler.AvailableAxis.GetValue(AxisNames.Twist).Min;
			TwistRangeSlider.SelectionEnd = SettingsHandler.AvailableAxis.GetValue(AxisNames.Twist).Max;
			LoadSliderUI(true, TwistRangeSlider, TwistRangeMin, TwistRangeMax);
			LoadSliderUI(false, TwistRangeSlider, TwistRangeMin, TwistRangeMax);
			TwistRangeMinLabel.Content = TwistRangeSlider.SelectionStart;
			TwistRangeMaxLabel.Content = TwistRangeSlider.SelectionEnd;

			SpeedValueLabel.Content = SpeedSlider.Value;
			SetLabelConnected(InputNetworkStatusLabel, false, null);
			SetLabelConnected(OutputNetworkStatusLabel, false, null);
			SetLabelConnected(GamepadStatus, false, null);
			SetLabelConnected(InputSerialStatusLabel, false, null);
			SetLabelConnected(OutputSerialStatusLabel, false, null);
			SetLabelConnected(BLEStatusLbl, false, null);
			//InvertXCheckbox.IsChecked = SettingsHandler.InverseTcXUpDownL0;
			//InvertXRollCheckbox.IsChecked = SettingsHandler.InverseTcXRollR2;
			//InvertYRollCheckbox.IsChecked = SettingsHandler.InverseTcYRollR1;
			//OutputAddressTxt.Text = SettingsHandler.OutputAddress;
			//IntputAddressTxt.Text = SettingsHandler.InputAddress;
			//OutPutSerialCmb.SelectedItem = SettingsHandler.OutputSerialPort;
			//InputSerialCmb.SelectedItem = SettingsHandler.InputSerialPort;

			//SerialOSRSettingsGrid.Visibility = Visibility.Hidden;
			switch (SettingsHandler.InputDevice)
			{
				case OSRRemoteInputMode.Address:
					InputAddressRdo.IsChecked = true;
					break;
				case OSRRemoteInputMode.Serial:
					InputSerialRdo.IsChecked = true;
					break;
				case OSRRemoteInputMode.Gamepad:
					InputGamepadRdo.IsChecked = true;
					break;
			}
			switch (SettingsHandler.OutputDevice)
			{
				case OSRRemoteOutputMode.Address:
					OutputAddressRdo.IsChecked = true;
					break;
				case OSRRemoteOutputMode.Serial:
					OutputSerialRdo.IsChecked = true;
					break;
				case OSRRemoteOutputMode.BLE:
					BLERdo.IsChecked = true;
					break;
			}

			UDPOutputCheckbox.IsChecked = SettingsHandler.UseUDPOutput;
			UDPInputCheckbox.IsChecked = SettingsHandler.UseUDPInput;
			//if (OutputSerialRdo.IsChecked.Value && InputGamepadRdo.IsChecked.Value ||
			//	BLERdo.IsChecked.Value && InputGamepadRdo.IsChecked.Value ||
			//	InputAddressRdo.IsChecked.Value && OutputSerialRdo.IsChecked.Value)
			//{
			//	SerialOSRSettingsGrid.Visibility = Visibility.Visible;
			//}

			if (InputSerialRdo.IsChecked.Value)
			{
				SerialOSRSettingsGrid.Visibility = Visibility.Hidden;
			}
		}

		public static IPAddress GetLocalIPV6Address(string hostName)
		{
			Ping ping = new Ping();
			var replay = ping.Send(hostName);

			if (replay.Status == IPStatus.Success)
			{
				return replay.Address;
			}
			return null;
		}

		public static IPAddress GetLocalIPV4Address()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
		}

		private void InRadio_Checked(object sender, RoutedEventArgs e)
		{
			if(_inputDevice != null)
			{
				_inputDevice.Dispose();
			}
			StartBtn.IsEnabled = true;
			var radioButton = (RadioButton)sender;
			//SerialOSRSettingsGrid.Visibility = Visibility.Hidden;
			SerialOSRSettingsGrid.Visibility = Visibility.Visible;

			if (radioButton == InputAddressRdo)
			{
				SettingsHandler.InputDevice = OSRRemoteInputMode.Address;
				GamepadMap.Visibility = Visibility.Hidden;
				if (SettingsHandler.UseUDPInput)
				{
					_inputDevice = InputUdpAddresshandler;
				}
				else
				{
					_inputDevice = InputTcpAddresshandler;
				}
				InitInputNetworkAddress();
			}
			else if (radioButton == InputGamepadRdo)
			{
				SettingsHandler.InputDevice = OSRRemoteInputMode.Gamepad;
				GamepadMap.Visibility = Visibility.Visible;
				if (!GamepadHandler.IsRunning)
				{
					GamepadHandler.Init(null);
				}
				_inputDevice = GamepadHandler;
			}
			else if (radioButton == InputSerialRdo)
			{
				SettingsHandler.InputDevice = OSRRemoteInputMode.Serial;
				GamepadMap.Visibility = Visibility.Hidden;
				SerialOSRSettingsGrid.Visibility = Visibility.Hidden;
				_inputDevice = InputSerialDeviceHandler;
				var selectedValue = InputSerialCmb.SelectedValue as string;
				if (!string.IsNullOrEmpty(selectedValue) && !InputSerialDeviceHandler.IsRunning)
				{
					InputSerialDeviceHandler.Init(new DeviceInitModel() { Address = selectedValue, Mode = OSRRemoteDeviceMode.In });
				}
			}

			//if (OutputSerialRdo.IsChecked.Value || BLERdo.IsChecked.Value)
			//{
			//	SerialOSRSettingsGrid.Visibility = Visibility.Visible;
			//}
			if (_outputDevice != null)
			{
				StartBtn.IsEnabled = _inputDevice.IsConnected && _outputDevice.IsConnected;
			}
		}

		private void OutRadio_Checked(object sender, RoutedEventArgs e)
		{
			if (_outputDevice != null)
			{
				_outputDevice.Dispose();
			}
			var radioButton = (RadioButton)sender;
			//SerialOSRSettingsGrid.Visibility = Visibility.Hidden;
			SerialOSRSettingsGrid.Visibility = Visibility.Visible;

			if (radioButton == OutputAddressRdo)
			{
				SettingsHandler.OutputDevice = OSRRemoteOutputMode.Address;
				if (SettingsHandler.UseUDPOutput)
				{
					_outputDevice = OutputUdpAddresshandler;
				}
				else
				{
					_outputDevice = OutputTcpAddresshandler;
				}
				InitOutputNetworkAddress();
			}
			else if (radioButton == OutputSerialRdo)
			{
				SettingsHandler.OutputDevice = OSRRemoteOutputMode.Serial;
				//SerialOSRSettingsGrid.Visibility = Visibility.Visible;
				_outputDevice = OutputSerialDeviceHandler;
				var selectedValue = OutPutSerialCmb.SelectedValue as string;
				if (!string.IsNullOrEmpty(selectedValue) && !OutputSerialDeviceHandler.IsRunning)
				{
					OutputSerialDeviceHandler.Init(new DeviceInitModel() { Address = selectedValue, Mode = OSRRemoteDeviceMode.Out });
				}
			}
			else if (radioButton == BLERdo)
			{
				SettingsHandler.OutputDevice = OSRRemoteOutputMode.BLE;
				if (!BLEDeviceHandler.IsRunning)
				{
					BLEDeviceHandler.Init(null);
				}
				//SerialOSRSettingsGrid.Visibility = Visibility.Visible;
				_outputDevice = BLEDeviceHandler;
				if (SettingsHandler.ShowBLEMessage)
				{
					Log.Dialog("Experimental!!", "BLE only works with the Romeo BLE mini (possible other Bluno devices)\nMay experience issues");
					SettingsHandler.ShowBLEMessage = false;
				}
			}
			if (_inputDevice != null)
			{
				StartBtn.IsEnabled = _inputDevice.IsConnected && _outputDevice.IsConnected;
			}
		}

		private void OutPutSerialCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (OutPutSerialCmb.IsLoaded)
			{
				OutputSerialDeviceHandler.Dispose();
				OutputSerialDeviceHandler.Init(new DeviceInitModel() { Address = OutPutSerialCmb.SelectedValue as string, Mode = OSRRemoteDeviceMode.Out });
			}
		}

		private void InputSerialCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (InputSerialCmb.IsLoaded)
			{
				InputSerialDeviceHandler.Dispose();
				InputSerialDeviceHandler.Init(new DeviceInitModel() { Address = InputSerialCmb.SelectedValue as string, Mode = OSRRemoteDeviceMode.Out });
			}
		}

		private void OutputAddressTxt_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (OutputAddressTxt.IsLoaded && OutputNetworkPortTxt.IsLoaded)
			{
				_outputNetworkAddressTxtDebouncer.Change();
			}
		}

		private void OutputNetworkAddressTxtDebouncer_Idled(object sender, EventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				InitOutputNetworkAddress();
			}));
		}

		private void InputNetworkPort_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (InputNetworkPortTxt.IsLoaded)
			{
				_inputNetworkPortTxtDebouncer.Change();
			}
		}

		private void InputNetworkPortTxtDebouncer_Idled(object sender, EventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				InitInputNetworkAddress();
			}));
		}

		private void InitOutputNetworkAddress()
		{
			if (OutputAddressRdo.IsChecked.Value && 
				!OutputUdpAddresshandler.IsRunning && 
				!OutputTcpAddresshandler.IsRunning && 
				!string.IsNullOrEmpty(OutputAddressTxt.Text) && 
				!string.IsNullOrEmpty(OutputNetworkPortTxt.Text))
			{
				if (SettingsHandler.UseUDPOutput)
				{
					OutputUdpAddresshandler.Init(new NetworkInitModel() { Address = OutputAddressTxt.Text, Port = int.Parse(OutputNetworkPortTxt.Text), Mode = OSRRemoteDeviceMode.Out });
				}
				else
				{
					OutputTcpAddresshandler.Init(new NetworkInitModel() { Address = OutputAddressTxt.Text, Port = int.Parse(OutputNetworkPortTxt.Text), Mode = OSRRemoteDeviceMode.Out });
				}
			}
		}

		private void InitInputNetworkAddress()
		{
			if (InputAddressRdo.IsChecked.Value &&
				!InputTcpAddresshandler.IsRunning && 
				!InputUdpAddresshandler.IsRunning && 
				!string.IsNullOrEmpty(InputNetworkPortTxt.Text))
			{
				var port = int.Parse(InputNetworkPortTxt.Text);
				if (SettingsHandler.UseUDPOutput)
				{
					InputUdpAddresshandler.Init(new NetworkInitModel() { Port = port, Mode = OSRRemoteDeviceMode.In });
				}
				else
				{
					InputTcpAddresshandler.Init(new NetworkInitModel() { Port = port, Mode = OSRRemoteDeviceMode.In });
				}
			}
		}

		private void GamepadConnected_Changed(object sender, ConnectedEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				SetLabelConnected(GamepadStatus, e.IsConnected, e.Message);
				ValidateConnections();
			}));
		}

		private void NetworkConnected_Changed(object sender, ConnectedEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				if (sender == InputTcpAddresshandler || sender == InputUdpAddresshandler)
				{
					SetLabelConnected(InputNetworkStatusLabel, e.IsConnected, e.Message);
				} else
				{
					SetLabelConnected(OutputNetworkStatusLabel, e.IsConnected, e.Message);
				}
				ValidateConnections();
			}));
		}

		private void SerialConnected_Changed(object sender, ConnectedEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				if (sender == InputSerialDeviceHandler)
				{
					SetLabelConnected(InputSerialStatusLabel, e.IsConnected, e.Message);
				} 
				else
				{
					SetLabelConnected(OutputSerialStatusLabel, e.IsConnected, e.Message);
				}
				ValidateConnections();
			}));
		}

		private void BLEDeviceHandler_ConnectionChanged(object sender, ConnectedEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				//if(!string.IsNullOrEmpty(e.Message))
				//{
				//	BLEStatusLbl.Content = e.Message;
				//}
				SetLabelConnected(BLEStatusLbl, e.IsConnected, e.Message);
				ValidateConnections();
			}));
		}

		private void BLEDeviceAdded(object sender, DeviceCollectionChangeEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				BLEDevices.Add(e.Device);
			}));
		}

		private void BLEDeviceRemoved(object sender, DeviceCollectionChangeEventArgs e)
		{
			Dispatcher.Invoke(new Action(async () =>
			{
				var device = BLEDevices.FirstOrDefault(x => x.Name == e.Device.Name);
				BLEDevices.Remove(device);
			}));
		}

		private void ValidateConnections()
		{
			if (_inputDevice != null && _outputDevice != null)
				StartBtn.IsEnabled = _inputDevice.IsConnected && _outputDevice.IsConnected && !_appRunning;
		}

		private void StartBtn_Click(object sender, RoutedEventArgs e)
		{
			_appRunning = true;
			InputDeviceSelectionGrid.IsEnabled = false;
			OutputDeviceSelectionGrid.IsEnabled = false;
			_inputDevice.SetIODeviceAsync(_outputDevice);
			_outputDevice.SetIODeviceAsync(_inputDevice);
			StartBtn.IsEnabled = false;
			StopBtn.IsEnabled = true;
		}

		private void StopBtn_Click(object sender, RoutedEventArgs e)
		{
			_appRunning = false;
			if (_inputDevice != null)
			{
				_inputDevice.Stop();
			}
			if (_outputDevice != null)
			{
				_outputDevice.Stop();
			}
			InputDeviceSelectionGrid.IsEnabled = true;
			OutputDeviceSelectionGrid.IsEnabled = true;
			StartBtn.IsEnabled = true;
			StopBtn.IsEnabled = false;
			ValidateConnections();
		}

		private void Range_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			var slider = (sender as Slider);
			if (slider.SelectionEnd != slider.SelectionStart)
			{
				if (e.NewValue > slider.SelectionEnd || e.NewValue < slider.SelectionStart)
				{
					slider.Value = e.OldValue;
					Log.Debug($"Slider change: {slider.Value}");
				}
			}
			if (slider == XRangeSlider)
			{
				SettingsHandler.AvailableAxis[AxisNames.Stroke].Mid = (int)slider.Value;
			} 
			else if (slider == YRollRangeSlider)
			{
				SettingsHandler.AvailableAxis[AxisNames.Roll].Mid = (int)slider.Value;
			}
			else if (slider == XRollRangeSlider)
			{
				SettingsHandler.AvailableAxis[AxisNames.Pitch].Mid = (int)slider.Value;
			}
		}

		private void XRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, XRangeSlider, XRangeMin, XRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Stroke].Min = (int)XRangeSlider.SelectionStart;
			XRangeMinLabel.Content = (int)XRangeSlider.SelectionStart;
		}

		private void XRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, XRangeSlider, XRangeMin, XRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Stroke].Max = (int)XRangeSlider.SelectionEnd;
			XRangeMaxLabel.Content = (int)XRangeSlider.SelectionEnd;
		}

		private void YRollRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, YRollRangeSlider, YRollRangeMin, YRollRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Roll].Min = (int)YRollRangeSlider.SelectionStart;
			YRollRangeMinLabel.Content = (int)YRollRangeSlider.SelectionStart;
		}

		private void YRollRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, YRollRangeSlider, YRollRangeMin, YRollRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Roll].Max = (int)YRollRangeSlider.SelectionEnd;
			YRollRangeMaxLabel.Content = (int)YRollRangeSlider.SelectionEnd;
		}

		private void XRollRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, XRollRangeSlider, XRollRangeMin, XRollRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Pitch].Min = (int)XRollRangeSlider.SelectionStart;
			XRollRangeMinLabel.Content = (int)XRollRangeSlider.SelectionStart;
		}

		private void XRollRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, XRollRangeSlider, XRollRangeMin, XRollRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Pitch].Max = (int)XRollRangeSlider.SelectionEnd;
			XRollRangeMaxLabel.Content = (int)XRollRangeSlider.SelectionEnd;
		}

		private void SwayRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, SwayRangeSlider, SwayRangeMin, SwayRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Sway].Min = (int)SwayRangeSlider.SelectionStart;
			SwayRangeMinLabel.Content = (int)SwayRangeSlider.SelectionStart;
		}
		private void SwayRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, SwayRangeSlider, SwayRangeMin, SwayRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Sway].Max = (int)SwayRangeSlider.SelectionEnd;
			SwayRangeMaxLabel.Content = (int)SwayRangeSlider.SelectionEnd;
		}

		private void SurgeRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, SurgeRangeSlider, SurgeRangeMin, SurgeRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Surge].Min = (int)SurgeRangeSlider.SelectionStart;
			SurgeRangeMinLabel.Content = (int)SurgeRangeSlider.SelectionStart;
		}
		private void SurgeRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, SurgeRangeSlider, SurgeRangeMin, SurgeRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Surge].Max = (int)SurgeRangeSlider.SelectionEnd;
			SurgeRangeMaxLabel.Content = (int)SurgeRangeSlider.SelectionEnd;
		}

		private void TwistRangeMin_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(true, TwistRangeSlider, TwistRangeMin, TwistRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Twist].Min = (int)TwistRangeSlider.SelectionStart;
			TwistRangeMinLabel.Content = (int)TwistRangeSlider.SelectionStart;
		}
		private void TwistRangeMax_DragDelta(object sender, DragDeltaEventArgs e)
		{
			UpdateRangeSliderUI(false, TwistRangeSlider, TwistRangeMin, TwistRangeMax, e.HorizontalChange);
			SettingsHandler.AvailableAxis[AxisNames.Twist].Max = (int)TwistRangeSlider.SelectionEnd;
			TwistRangeMaxLabel.Content = (int)TwistRangeSlider.SelectionEnd;
		}


		private void UpdateRangeSliderUI(bool minMode, Slider slider, Thumb min, Thumb max, double horizontalChange)
		{
			double left = Canvas.GetLeft(min);
			double right = Canvas.GetLeft(max);

			if (minMode)
			{
				if (left + horizontalChange < right && left + horizontalChange > 0)
				{
					Canvas.SetLeft(min, left + horizontalChange);
					slider.SelectionStart = (left + horizontalChange) / slider.Width * slider.Maximum;
					if (slider.Value < slider.SelectionStart)
					{
						slider.Value = slider.SelectionStart;
					}
				}
			}
			else
			{
				if (right + horizontalChange > left && right + horizontalChange < slider.Width)
				{
					Canvas.SetLeft(max, right + horizontalChange); ;
					slider.SelectionEnd = (right + horizontalChange) / slider.Width * slider.Maximum;
					if (slider.Value > slider.SelectionEnd)
					{
						slider.Value = slider.SelectionEnd;
					}
				} 
			}
		}
		private void LoadSliderUI(bool minMode, Slider slider, Thumb min, Thumb max)
		{
			if (minMode)
			{
				var percent = (slider.SelectionStart / slider.Maximum);
				var position = slider.Width * percent;
				Canvas.SetLeft(min, position);
			}
			else
			{
				var percent = (slider.SelectionEnd / slider.Maximum);
				var position = slider.Width * percent;
				Canvas.SetLeft(max, position);
			}
		}

		void DataWindow_Closing(object sender, CancelEventArgs e)
		{
			SettingsHandler.Save();
		}

		private void RefreshCSerialPorts_Click(object sender, RoutedEventArgs e)
		{
			var refreshedPorts = OutputSerialDeviceHandler.GetPorts();
			SerialPorts.Clear();
			foreach(var port in refreshedPorts)
			{
				SerialPorts.Add(port);
			}
		}

		private void Window_Deactivated(object sender, EventArgs e)
		{
			if (InputGamepadRdo.IsChecked.Value && GamepadHandler.IsRunning)
			{
				this.Topmost = true;
				this.Focus();
				Activate();
				foreach (Window window in Application.Current.Windows)
				{
					// Don't change for main window
					if (window.GetType().Name != this.GetType().Name)
					{
						window.Topmost = false;
					}
				}
			}
			else
			{
				this.Topmost = false;
			}
		}

		private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (SpeedValueLabel != null)
				SpeedValueLabel.Content = SettingsHandler.Speed;
		}

		private void SetLabelConnected(Label label, bool connected, string message)
		{
			if (message != null && message.Contains(SettingsHandler.DEVICE_CONNECT))
			{
				label.Content = "\u231B";
				label.Foreground = System.Windows.Media.Brushes.Black;
			} 
			else 
			{
				label.Content = connected ? "\u2714" : "\u274C";
				label.Foreground = connected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
			}
		}

		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var settingsWindow = new SettingsWindow();
			settingsWindow.Owner = this;
			settingsWindow.ShowDialog();
		}

		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var aboutWindow = new AboutWindow();
			aboutWindow.Owner = this;
			aboutWindow.ShowDialog();
		}

		private async void BLECmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (BLECmb.SelectedItem != null)
			{
				var bleDevice = (DeviceModel)BLECmb.SelectedItem;
				await BLEDeviceHandler.ConnectDevice(bleDevice.Id);
			}
		}

		private void UseUDPInput_Clicked(object sender, RoutedEventArgs e)
		{
			//Log.Dialog("Input Protocol changed", "Make sure the other end has the same value.");
			if (SettingsHandler.UseUDPInput)
			{
				//Log.Dialog("Input Protocol changed", "Make sure the source has UDP as output.");
				_inputDevice = InputUdpAddresshandler;
			}
			else
			{
				//Log.Dialog("Input Protocol changed", "Make sure the source has TCP as output. (UDP checkbox unchecked)");
				_inputDevice = InputTcpAddresshandler;
			}
		}

		private void UseUDPOutput_Clicked(object sender, RoutedEventArgs e)
		{
			//Log.Dialog("Output Protocol changed", "Please restart the app.\nMake sure the other end has the same value.");
			if (SettingsHandler.UseUDPOutput)
			{
				//Log.Dialog("Output Protocol changed", "Make sure the destination has UDP as input.");
				_outputDevice = OutputUdpAddresshandler;
			}
			else
			{
				//Log.Dialog("Output Protocol changed", "Make sure the destination has TCP as input. (UDP checkbox unchecked)");
				_outputDevice = OutputTcpAddresshandler;
			}
		}


		private void InvertXCheckbox_Checked(object sender, RoutedEventArgs e)
		{

		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}

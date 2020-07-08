
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using TCode_Remote.Library.Handler;
using TCode_Remote.Library.Model;
using TCode_Remote.Library.Tools;

namespace TCode_Remote
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private ObservablePairCollection<string, ChannelNameModel> _channelData;
		public ObservablePairCollection<string, ChannelNameModel> ChannelData
		{
			get
			{
				return _channelData;
			}
			set
			{
				_channelData = value;
				OnPropertyChanged("ChannelData");
			}
		}
		public SettingsWindow()
		{
			InitializeComponent();
			DataContext = this;
			ChannelData = new ObservablePairCollection<string, ChannelNameModel>(SettingsHandler.AvailableAxis);
			UseUDP.IsChecked = SettingsHandler.UseUDP;
		}

		void OnPropertyChanged(string prop)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		private void UseUDP_Clicked(object sender, RoutedEventArgs e)
		{
			Log.Dialog("Protocol changed", "Please restart the app.\nMake sure the other end has the same value.");
		}

		private void ResetSettingsBtn_Click(object sender, RoutedEventArgs e)
		{
			SettingsHandler.ResetSettings();
			Log.Dialog("Reset to default", "Please restart the app.");
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			SettingsHandler.AvailableAxis = ChannelData.ToDictionary();
			SettingsHandler.Save();
			Close();
		}

		private void AddChannelBtn_Click(object sender, RoutedEventArgs e)
		{
			ChannelData.Add(new Pair<string, ChannelNameModel>("", new ChannelNameModel()));
		}

		private void RemoveChannelBtn_Click(object sender, RoutedEventArgs e)
		{
			if (ChannelGrid.SelectedItems.Count > 0)
			{
				var selectedItem = ChannelGrid.SelectedItems[0];
				_channelData.Remove((Pair<string, ChannelNameModel>)selectedItem);
			}
		}
	}
}

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using TCode_Remote.Library.Handler;

namespace TCode_Remote
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow()
		{
			InitializeComponent();

			var versionFull = SettingsHandler.GetVersion();
			AboutLabel.Content = $"TCode Remote v{versionFull.Major}.{versionFull.Minor}a";
			TCodeVersionLabel.Content = SettingsHandler.TCodeVersion;
		}

		private void PatreonHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}

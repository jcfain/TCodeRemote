using System;
using System.Windows;
using TCode_Remote.Library.Handler;

namespace TCode_Remote.Library.Tools
{
	public static class Log
	{
		private static bool isDebugMode = false;
		private static string startUpDateTime;
		static Log()
		{
			startUpDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
#if DEBUG
			isDebugMode = true;
#endif
		}
		public static void Debug(string message)
		{
			if (isDebugMode || SettingsHandler.IsDebugMode)
			{
				System.Diagnostics.Debug.WriteLine("Debug: " + message);
				if (SettingsHandler.IsDebugMode)
				{
					WriteDebugFile($"Debug: {message}");
				}
			}
		}

		public static void Info(string message)
		{
			System.Diagnostics.Debug.WriteLine("Info: " + message);
			if (SettingsHandler.IsDebugMode)
			{
				WriteDebugFile($"Info: {message}");
			}
		}

		public static void Warning(string message)
		{
			System.Diagnostics.Debug.WriteLine("Warning: " + message);
			if (SettingsHandler.IsDebugMode)
			{
				WriteDebugFile($"Warn: {message}");
			}
		}

		public static void Error(string message)
		{
			System.Diagnostics.Debug.WriteLine("Error: " + message);
			WriteErrorFile("Error: " + message);
			if (SettingsHandler.IsDebugMode)
			{
				WriteDebugFile($"ERROR: {message}");
			}
		}

		public static void Dialog(string caption, string message)
		{
			MessageBoxButton button = MessageBoxButton.OK;
			MessageBoxImage icon = MessageBoxImage.Warning;
			MessageBox.Show(message, caption, button, icon);
			if (SettingsHandler.IsDebugMode)
			{
				WriteDebugFile($"Dialog: {message}");
			}
		}

		private static void WriteDebugFile(string message)
		{
			var path = $"Debug_Output_{startUpDateTime}.txt";
			WriteFile(path, message);
		}

		private static void WriteErrorFile(string message)
		{
			var path = $"Error_Output_{startUpDateTime}.txt";
			WriteFile(path, message);
		}

		private static void WriteFile(string path, string message)
		{
			using (System.IO.StreamWriter file =
			new System.IO.StreamWriter(path, true))
			{
				file.WriteLine($"{DateTime.Now} {message}");
			}
		}
	}
}

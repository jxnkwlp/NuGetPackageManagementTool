using NuGetPackageManagerUI.Utils;
using System;
using System.ComponentModel;
using System.Windows;

namespace NuGetPackageManagerUI.Xaml
{
	/// <summary>
	/// Interaction logic for LogsWindow.xaml
	/// </summary>
	public partial class LogsWindow : Window
	{
		private readonly ILogger _logger = null;

		public LogsWindow()
		{
			InitializeComponent();

			_logger = ServiceLocator.GetService<ILogger>();

			_logger.OnLog += Logger_OnLog;
		}

		private void Logger_OnLog(string messaeg)
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				LogRichTextBox.AppendText(messaeg + Environment.NewLine);
				LogRichTextBox.ScrollToEnd();
			}));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			_logger.OnLog -= Logger_OnLog;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			LogRichTextBox.Document.Blocks.Clear();
		}
	}
}

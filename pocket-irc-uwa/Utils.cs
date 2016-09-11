using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace pocket_irc_uwa
{
	static class Utils
	{
		private static string FormatTime()
		{
			string hh;
			string mm;

			if (DateTime.Now.Hour >= 10)
			{
				hh = DateTime.Now.Hour.ToString();
			}
			else
			{
				hh = "0" + DateTime.Now.Hour;
			}

			if (DateTime.Now.Minute >= 10)
			{
				mm = DateTime.Now.Minute.ToString();
			}
			else
			{
				mm = "0" + DateTime.Now.Minute;
			}

			return "[" + hh + ":" + mm + "] ";
		}

		public async static void AddText(ListBox listBox, String message, int limit)
		{
			if ((App.Current as App).Settings.ShowTime)
			{
				message = Utils.FormatTime() + message;
			}

			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (message.Length > limit)
				{
					listBox.Items.Add(message.Substring(0, limit));

					message = message.Substring(limit);

					while (message.Length > limit)
					{
						listBox.Items.Add("  " + message.Substring(0, limit));

						message = message.Substring(limit);
					}

					if (message.Length > 0)
					{
						listBox.Items.Add("  " + message);
					}
				}
				else
				{
					listBox.Items.Add(message);
				}

				listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
			});
		}

		public async static void AddText(TextBlock listBox, String message, ScrollViewer scrollViewer)
		{
			const int maxLength = 10000;

			if ((App.Current as App).Settings.ShowTime)
			{
				message = Utils.FormatTime() + message;
			}

			message = message.Trim();
			message += "\n";

			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				/*if (listBox.Text.Length == 0)
				{
					String prefix = "";

					for (int i = 0; i < 100; i++)
					{
						prefix += "\n";
					}

					listBox.Text = prefix;
				}*/

				String newText = listBox.Text + message;

				if (newText.Length > maxLength)
				{
					newText = newText.Substring(newText.Length - maxLength);
				}

				listBox.Text = newText;

				scrollViewer.ChangeView(null, double.MaxValue, null);
			});
		}

		public static void LogException(Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}

		public static void SetType(TextBox textBox, InputScopeNameValue type)
		{
			textBox.InputScope = new InputScope();
			InputScopeName inputScopeName = new InputScopeName();
			inputScopeName.NameValue = type;
			textBox.InputScope.Names.Add(inputScopeName);
		}

		public static void SetVisible(this AppBarButton button, bool? visible)
		{
			if ((null == visible) || (false == visible.Value))
			{
				button.Visibility = Visibility.Collapsed;
			}
			else
			{
				button.Visibility = Visibility.Visible;
			}
		}

		public static async void ShowMessage(String message)
		{
			MessageDialog dialog = new MessageDialog(message);

			UICommand okButton = new UICommand("OK");

			dialog.Commands.Add(okButton);

			await dialog.ShowAsync();
		}
	}
}

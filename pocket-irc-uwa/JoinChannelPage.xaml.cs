using MyIRC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace pocket_irc_uwa
{
	public sealed partial class JoinChannelPage : Page
	{
		public JoinChannelPage()
		{
			InitializeComponent();

			var app = App.Current as App;

			this.TextBoxChannel.Text = app.Settings.Channel;
			this.TextBoxKey.Text = app.Settings.ChannelKey;

			this.ListBoxChannels.SelectionChanged += this.ChannelSelectionChanged;

			try
			{
				app.Server.Client.ChannelsRecieving += this.ChannelsRecieving;

				var t = Task.Run(async delegate
				{
					await Task.Delay(5000);

					if (false == this.closed)
					{
						app.Server.Client.ListChannels(null);
					}
				});

				t.Start();
			}
			catch (Exception ex)
			{
				Utils.LogException(ex);
			}
		}

		private void ChannelSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (null != this.ListBoxChannels.SelectedItem)
			{
				this.TextBoxChannel.Text = ((ChannelInfo)this.ListBoxChannels.SelectedItem).Name;
			}
		}

		private async void ChannelsRecieving(IrcClient server, List<ChannelInfo> channels)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				try
				{
					foreach (ChannelInfo channel in channels)
					{
						this.ListBoxChannels.Items.Add(channel);
					}
				}
				catch (Exception ex)
				{
					Utils.LogException(ex);
				}
			});
		}

		private void JoinClick(object sender, RoutedEventArgs e)
		{
			this.closed = true;

			try
			{
				var app = App.Current as App;

				app.Settings.Channel = this.TextBoxChannel.Text;
				app.Settings.ChannelKey = this.TextBoxKey.Text;
				app.Server.Client.ChannelsRecieving -= new EventHandlerChannelListRecieving(ChannelsRecieving);
				app.Server.Client.JoinChannel(app.Settings.Channel, app.Settings.ChannelKey);
			}
			catch (Exception ex)
			{
				Utils.LogException(ex);
			}

			this.Frame.GoBack();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.closed = true;

			try
			{
				(App.Current as App).Server.Client.ChannelsRecieving -= this.ChannelsRecieving;
			}
			catch (Exception ex)
			{
				Utils.LogException(ex);
			}

			this.Frame.GoBack();
		}

		private bool closed = false;
	}
}

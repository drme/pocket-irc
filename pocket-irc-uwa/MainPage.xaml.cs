using MyIRC;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace pocket_irc_uwa
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();

			this.NavigationCacheMode = NavigationCacheMode.Required;
			this.mainWindow.SelectionChanged += PivotSelectionChanged;

			UpdateMenus();
		}

		private void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateMenus();
		}

		private void SettingsClick(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(SettingsPage));
		}

		private void ChangeNickClick(object sender, RoutedEventArgs e)
		{
			if (null != Server)
			{
				Settings.NickName = Server.User.Nick;
			}

			this.Frame.Navigate(typeof(ChangeNickPage));
		}

		private void JoinClick(object sender, RoutedEventArgs e)
		{
			if (null != this.Server)
			{
				this.Frame.Navigate(typeof(JoinChannelPage));
			}
		}

		private void ConnectClick(object sender, RoutedEventArgs e)
		{
			this.Frame.Navigate(typeof(ConnectPage), this);
		}

		private void OpenPrivateClick(object sender, RoutedEventArgs e)
		{
			if (null != Server)
			{
				this.Frame.Navigate(typeof(OpenPrivatePage));
			}
		}

		private void DisconnectClick(object sender, RoutedEventArgs e)
		{
			if (null != Server)
			{
				Server.Disconnect();
			}
		}

		public async void UpdateMenus()
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var app = (App.Current as App);

				var connected = (null != app.Server) && (null != app.Server.Client) && (app.Server.Client.IsConnected);

				PivotItem selectedPage = (PivotItem)this.mainWindow.SelectedItem;

				if (null == selectedPage)
				{
					if (this.mainWindow.Items.Count > 0)
					{
						selectedPage = (PivotItem)this.mainWindow.Items[0];
					}
				}

				this.ConnectButton.SetVisible(false == connected);
				this.ClosePrivateChatButton.SetVisible(selectedPage?.Content is PrivateChatControl);
				this.WhoIsButton.SetVisible(selectedPage?.Content is PrivateChatControl);
				this.JoinButton.SetVisible(connected & (selectedPage?.Content is ServerControl));
				this.OpenPrivateChatButton.SetVisible(connected & (selectedPage?.Content is ServerControl));
				this.ChangeNickButton.SetVisible(connected & (selectedPage?.Content is ServerControl));
				this.DisconnectButton.SetVisible(connected & (selectedPage?.Content is ServerControl));
				this.LeaveChannelButton.SetVisible(selectedPage?.Content is ChannelChatControl);

				bool showUserActions = connected & (selectedPage?.Content is ChannelChatControl) & (null != (selectedPage?.Content as ChannelChatControl)?.SelectedUser);

				this.BanUserButton.SetVisible(showUserActions);
				this.KickUserButton.SetVisible(showUserActions);
				this.VoiceUserButton.SetVisible(showUserActions);
				this.DeVoiceUserButton.SetVisible(showUserActions);
				this.OpUserButton.SetVisible(showUserActions);
				this.DeOpUserButton.SetVisible(showUserActions);
				this.PrivateChatButton.SetVisible(showUserActions);
			});
		}

		private IrcClient Server
		{
			get
			{
				if (null != (App.Current as App).Server)
				{
					return (App.Current as App).Server.Client;
				}
				else
				{
					return null;
				}
			}
		}

		private IrcConfig Settings
		{
			get
			{
				return (App.Current as App).Settings;
			}
		}

		public Pivot Pages
		{
			get
			{
				return this.mainWindow;
			}
		}

		private void ClosePrivateChatClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as PrivateChatControl)?.Close();
		}

		private void WhoIsClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as PrivateChatControl)?.CommandWhoIs();
		}

		private void LeaveChannelClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.Close();
		}

		private void BanUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.BanClick();
		}

		private void KickUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.KickClick();
		}

		private void VoiceUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.VoiceClick();
		}

		private void DeVoiceUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.DeVoiceClick();
		}

		private void OpUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.OpClick();
		}

		private void DeOpUserClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.DeOpClick();
		}

		private void PrivateChatClick(object sender, RoutedEventArgs e)
		{
			((this.mainWindow.SelectedItem as PivotItem)?.Content as ChannelChatControl)?.QueryClick();
		}
	}
}

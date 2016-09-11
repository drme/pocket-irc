using MyIRC;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace pocket_irc_uwa
{
	public sealed partial class ServerControl : UserControl
	{
		public ServerControl(Pivot mainWindow, MainPage mainPage)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			this.mainPage = mainPage;

			ReConnect();
		}

		public void ReConnect()
		{
			this.irc = new IrcClient();

			IrcConfig config = (App.Current as App).Settings;

			Connect(config.Server, config.Port, config.NickName, config.Password, config.User, config.RealName);
		}

		private void Connect(string server, int port, string nick, string pass, string indent, string realName)
		{
			this.irc.PingPong += new EventHandlerPingPong(PingPong);
			this.irc.RawMessageRecieved += this.RawMessageRecieved;
			this.irc.ChannelJoined += this.ChannelJoined;
			this.irc.PrivateChatCreated += this.PrivateChatCreated;
			this.irc.Disconnected += new EventHandlerDisconnected(Disconnected);
			this.irc.Connected += new EventHandlerConnected(Connected);
			this.irc.Notice += new EventHandlerNotice(Notice);
			this.irc.UnparsedLeftText += new EventHandlerUnparsedLeftText(UnparsedLeftText);

			this.irc.User.Nick = nick;
			this.irc.User.RealName = realName;
			this.irc.User.Ident = indent;

			this.irc.Connect(server, 6667);
		}

		private void PingPong(IrcClient server)
		{
			AddConsoleText("PING PONG");
		}

		private void AddConsoleText(string message)
		{
			Utils.AddText(this.listBoxLog, message, this.scrollViewer_Status);
		}

		private void RawMessageRecieved(IrcClient server, string msg)
		{
			AddConsoleText(msg);
		}

		private void UnparsedLeftText(IrcClient server, string text)
		{
			AddConsoleText(text);
		}

		private void Notice(IrcClient server, IrcUser user, string message)
		{
			AddConsoleText("-" + user.Nick + "- " + message);
		}

		private void Disconnected(IrcClient server)
		{
			try
			{
				AddConsoleText("*** Disconnected");

				this.mainPage.UpdateMenus();

				if (null != this.irc)
				{
					this.irc.PingPong -= new EventHandlerPingPong(PingPong);
					this.irc.RawMessageRecieved -= RawMessageRecieved;
					this.irc.ChannelJoined -= this.ChannelJoined;
					this.irc.PrivateChatCreated -= this.PrivateChatCreated;
					this.irc.Disconnected -= new EventHandlerDisconnected(Disconnected);
					this.irc.Connected -= new EventHandlerConnected(Connected);
					this.irc.Notice -= new EventHandlerNotice(Notice);
					this.irc.UnparsedLeftText -= new EventHandlerUnparsedLeftText(UnparsedLeftText);

					this.irc = null;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private void Connected(IrcClient server)
		{
			AddConsoleText("*** Connected to server");
			this.mainPage.UpdateMenus();
		}

		private void ChannelJoined(IrcClient server, Channel channel)
		{
			AddChannelTab(channel);
		}

		private void PrivateChatCreated(IrcClient server, PrivateChat privateChat)
		{
			NewPrivateChat(privateChat);
		}

		internal IrcClient Client
		{
			get
			{
				return this.irc;
			}
		}

		private async void AddChannelTab(Channel channel)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				PivotItem item = new PivotItem();
				item.Header = channel.Name;
				item.Content = new ChannelChatControl(channel, this.irc, this.mainPage, item);

				this.mainWindow.Items.Add(item);

				this.mainWindow.SelectedItem = item;
			});
		}

		private async void NewPrivateChat(PrivateChat privateChat)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				PivotItem item = new PivotItem();
				item.Header = privateChat.User.Nick;
				item.Content = new PrivateChatControl(privateChat, this.irc, item, this.mainWindow);

				this.mainWindow.Items.Add(item);

				this.mainWindow.SelectedItem = item;
			});
		}

		private void TextBoxCommandKeyDown(object sender, KeyRoutedEventArgs e)
		{
			try
			{
				if (e.Key == VirtualKey.Enter)
				{
					if (this.textBoxCommand.Text.Length > 0)
					{
						if ((null != this.irc) && (true == this.irc.IsConnected))
						{
							if (this.textBoxCommand.Text[0] == '/')
							{
								this.irc.SendMessage(this.textBoxCommand.Text.Substring(1));
							}
							else
							{
								this.irc.SendMessage(this.textBoxCommand.Text);
							}
						}
						else
						{
							AddConsoleText("*** Not connected ***");
						}
					}

					this.textBoxCommand.Text = string.Empty;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		private IrcClient irc;
		private Pivot mainWindow;
		private MainPage mainPage;
	}
}

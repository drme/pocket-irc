using MyIRC;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace pocket_irc_uwa
{
	public sealed partial class ChannelChatControl : UserControl
	{
		public ChannelChatControl(Channel channel, IrcClient server, MainPage mainPage, PivotItem pageItem)
		{
			InitializeComponent();

			this.channel = channel;
			this.server = server;
			this.mainPage = mainPage;
			this.pageItem = pageItem;

			this.server.Disconnected += new EventHandlerDisconnected(ServerDisconnected);
			this.channel.Kicked += new EventHandlerKicked(Kicked);
			this.channel.NoticeAdded += new EventHandlerNoticeAdded(NoticeAdded);
			this.channel.UserParted += new EventHandlerUserPartedChannel(UserParted);
			this.channel.UserQuited += new EventHandlerUserQuitedChannel(UserQuited);
			this.channel.UserKicked += new EventHandlerUserKicked(UserKicked);
			this.channel.TopicChanged += new EventHandlerTopicChanged(TopicChanged);
			this.channel.MessageAdded += new EventHandlerMessageAdded(MessageAdded);
			this.channel.NicksRecieved += new EventHandlerNicksRecieved(NicksRecieved);
			this.channel.UserJoined += new EventHandlerUserJoinedChannel(UserJoined);
			this.channel.TopicRecieved += new EventHandlerTopicRecieved(TopicRecieved);
			this.channel.NickChanged += new EventHandlerUserNickChanngedChannel(NickChanged);
			this.channel.ModeChanged += new EventHandlerModeChanged(ModeChanged);

			this.textBoxMessage.InputScope = new InputScope();
			InputScopeName inputScopeName = new InputScopeName();
			inputScopeName.NameValue = InputScopeNameValue.Chat;
			this.textBoxMessage.InputScope.Names.Add(inputScopeName);

			NicksUpdated();
		}

		private void Kicked(IrcClient server, Channel channel, IrcUser user, string who, string message)
		{
			try
			{
				if (null == message)
				{
					AppendText("*** you were kicked from " + this.channel.Name + " by " + user.Nick);
				}
				else
				{
					AppendText("*** you were kicked from " + this.channel.Name + " by " + user.Nick + " (" + message + ")");
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				Close();
			}
		}

		private void TopicRecieved(IrcClient server, Channel channel, string topic, string author, DateTime setDate)
		{
			AppendText("*** topic is: " + topic);
		}

		private void ServerDisconnected(IrcClient server)
		{
			AppendText("*** Disconnected");
		}

		private void AppendText(string message)
		{
			Utils.AddText(this.listBoxChat, message, this.scrollViewer);
		}

		private void NoticeAdded(IrcClient server, Channel channel, IrcUser user, string message)
		{
			AppendText("-" + user.Nick + "- " + message);
		}

		private void UserKicked(IrcClient server, Channel chan, IrcUser user, string who, string message)
		{
			AppendText("*** " + who + " was kicked by " + user.Nick + " (" + message + ")");
			RemoveNick(who);
		}

		private async void RemoveNick(string nick)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Object toRemove = null;

				foreach (Object item in this.listBoxUsers.Items)
				{
					if (nick.Equals(item.ToString()))
					{
						toRemove = item;
						break;
					}
				}

				if (null != toRemove)
				{
					this.listBoxUsers.Items.Remove(toRemove);
				}
			});
		}

		private void UserJoined(IrcClient server, Channel chan, IrcUser user)
		{
			UserAdd(user);
		}

		private void UserAdd(IrcUser user)
		{
			if (user.Nick.ToUpper().Equals(this.server.User.Nick.ToUpper()))
			{
				AppendText("*** Now talking in " + this.channel.Name);
			}
			else
			{
				if (true == (App.Current as App).Settings.ShowJoins)
				{
					AppendText("*** Joins " + user.Nick + " (" + user.FullId + ")\n");
				}
			}

			AddNick(user.Nick);
		}

		private async void AddNick(string nick)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				foreach (string user in this.listBoxUsers.Items)
				{
					if (nick == user)
					{
						return;
					}
				}

				this.listBoxUsers.Items.Add(nick);
			});
		}

		private void MessageAdded(IrcClient server, Channel channel, IrcUser user, string message)
		{
			AppendText("<" + user.Nick + "> " + message);
		}

		private void TopicChanged(IrcClient server, Channel channel, string newTopic, string changedBy)
		{
			AppendText("*** " + changedBy + " changes topic to " + newTopic);
		}

		private void UserQuited(IrcClient server, Channel chan, IrcUser user, string message)
		{
			if (true == (App.Current as App).Settings.ShowQuits)
			{
				AppendText("*** quits " + user.Nick + " (" + user.Ident + "@" + user.HostName + ") (" + message + ")");
			}

			RemoveNick(user.Nick);
		}

		private void UserParted(IrcClient server, Channel chan, IrcUser user, string message)
		{
			if (true == (App.Current as App).Settings.ShowParts)
			{
				AppendText("*** parts " + user.Nick + " (" + user.Ident + "@" + user.HostName + ") (" + message + ")");
			}

			RemoveNick(user.Nick);
		}

		private void ModeChanged(IrcClient server, Channel chan, IrcUser user, string[] mode)
		{
			string text = "*** " + user.Nick + " sets mode";

			for (int i = 2; i <= mode.Length - 1; i++)
			{
				text += " " + mode[i];
			}

			AppendText(text);

			NicksUpdated();
		}

		private void NickChanged(IrcClient server, Channel chan, IrcUser user, string newNick)
		{
			AppendText("*** " + user.Nick + " is known as " + newNick);
			RenameNick(user.Nick, newNick);
		}

		private void NicksUpdated()
		{
			foreach (ChannelNick nick in this.channel.Nicks)
			{
				AddNick(nick.Name);
			}
		}

		private void RenameNick(string nick, string newNick)
		{
			RemoveNick(nick);
			AddNick(newNick);
		}

		private void NicksRecieved(IrcClient server, Channel channel)
		{
			NicksUpdated();
		}

		public void BanClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.SetMode("+b " + n.Name + "!*@*");
			}
		}

		public void KickClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.Kick(n.Name);
			}
		}

		public void DeVoiceClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.SetMode("-v " + n.Name);
			}
		}

		public void VoiceClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.SetMode("+v " + n.Name);
			}
		}

		public void DeOpClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.SetMode("-o " + n.Name);
			}
		}

		public void OpClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.channel.SetMode("+o " + n.Name);
			}
		}

		public void QueryClick()
		{
			ChannelNick n = GetNick();

			if (null != n)
			{
				this.server.QueryNick(n.Name);
			}
		}

		private ChannelNick GetNick()
		{
			try
			{
				return new ChannelNick((String)this.listBoxUsers.SelectedItem);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public async void Close()
		{
			this.server.Disconnected -= new EventHandlerDisconnected(ServerDisconnected);
			this.channel.Kicked -= new EventHandlerKicked(Kicked);
			this.channel.NoticeAdded -= new EventHandlerNoticeAdded(NoticeAdded);
			this.channel.UserParted -= new EventHandlerUserPartedChannel(UserParted);
			this.channel.UserQuited -= new EventHandlerUserQuitedChannel(UserQuited);
			this.channel.UserKicked -= new EventHandlerUserKicked(UserKicked);
			this.channel.TopicChanged -= new EventHandlerTopicChanged(TopicChanged);
			this.channel.MessageAdded -= new EventHandlerMessageAdded(MessageAdded);
			this.channel.NicksRecieved -= new EventHandlerNicksRecieved(NicksRecieved);
			this.channel.UserJoined -= new EventHandlerUserJoinedChannel(UserJoined);
			this.channel.TopicRecieved -= new EventHandlerTopicRecieved(TopicRecieved);
			this.channel.NickChanged -= new EventHandlerUserNickChanngedChannel(NickChanged);
			this.channel.ModeChanged -= new EventHandlerModeChanged(ModeChanged);

			this.channel.Part("");

			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.mainPage.Pages.Items.Remove(this.pageItem);
			});
		}

		private void TextBoxMessageKeyDown(object sender, KeyRoutedEventArgs e)
		{
			if (e.Key == VirtualKey.Enter)
			{
				if (this.textBoxMessage.Text.Length > 0)
				{
					if (true == this.server.IsConnected)
					{
						if (this.textBoxMessage.Text[0] == '/')
						{
							this.server.SendMessage(this.textBoxMessage.Text.Substring(1));
						}
						else
						{
							AppendText("<" + this.server.User.Nick + "> " + this.textBoxMessage.Text);
							this.channel.SayTextLine(this.textBoxMessage.Text);
						}
					}
					else
					{
						AppendText("*** Not connected ***");
					}
				}

				this.textBoxMessage.Text = string.Empty;
			}
		}

		private void UsersSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			this.mainPage.UpdateMenus();
		}

		public string SelectedUser
		{
			get
			{
				return this.listBoxUsers.SelectedItem?.ToString();
			}
		}

		private Channel channel;
		private IrcClient server;
		private MainPage mainPage;
		private PivotItem pageItem;
	}
}

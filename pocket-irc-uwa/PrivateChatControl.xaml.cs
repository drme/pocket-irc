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
	sealed partial class PrivateChatControl : UserControl
	{
		public PrivateChatControl(PrivateChat privateChat, IrcClient server, PivotItem pivotItem, Pivot mainWindow)
		{
			InitializeComponent();

			this.pivotItem = pivotItem;
			this.mainWindow = mainWindow;
			this.server = server;
			this.privateChat = privateChat;

			try
			{
				this.privateChat.NickChanged += new EventHandlerNickChangedPrivate(PrivateChatNickChanged);
				this.privateChat.PrivateMessage += new EventHandlerPrivMsgPrivate(PrivateChatPrivateMessage);
				this.server.Disconnected += new EventHandlerDisconnected(ServerDisconnected);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}

			this.textBoxMessage.InputScope = new InputScope();
			InputScopeName inputScopeName = new InputScopeName();
			inputScopeName.NameValue = InputScopeNameValue.Chat;
			this.textBoxMessage.InputScope.Names.Add(inputScopeName);
		}

		private void ServerDisconnected(IrcClient server)
		{
			AppendLine("*** Disconnected");
		}

		private async void PrivateChatNickChanged(IrcClient server, PrivateChat privateChat, string oldNick, string newNick)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				this.pivotItem.Header = newNick;
			});
		}

		private void AppendLine(string msg)
		{
			Utils.AddText(this.listBoxChat, msg, this.scrollViewer);
		}

		private void NewMessage(IrcUser user, string message)
		{
			AppendLine("<" + user.Nick + "> " + message);
		}

		private void PrivateChatPrivateMessage(IrcClient server, PrivateChat priv, IrcUser user, string message)
		{
			NewMessage(user, message);
		}

		private void TextBoxMessageKeyDown(object sender, KeyRoutedEventArgs e)
		{
			try
			{
				if (e.Key == VirtualKey.Enter)
				{
					if (this.textBoxMessage.Text.Length > 0)
					{
						if ((null != this.server) && (true == this.server.IsConnected))
						{
							if (this.textBoxMessage.Text[0] == '/')
							{
								this.server.SendMessage(this.textBoxMessage.Text.Substring(1));
							}
							else
							{
								if (null != this.privateChat)
								{
									AppendLine("<" + this.server.User.Nick + "> " + this.textBoxMessage.Text);
									this.privateChat.SayTextLine(this.textBoxMessage.Text);
								}
							}
						}
						else
						{
							AppendLine("*** Not connected ***");
						}
					}

					this.textBoxMessage.Text = string.Empty;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		public void Close()
		{
			this.mainWindow.Items.Remove(this.pivotItem);

			if (null != this.privateChat)
			{
				this.privateChat.Close();
			}

			RemoveHandlers();
		}

		private void RemoveHandlers()
		{
			if (null != this.privateChat)
			{
				this.privateChat.NickChanged -= new EventHandlerNickChangedPrivate(PrivateChatNickChanged);
				this.privateChat.PrivateMessage -= new EventHandlerPrivMsgPrivate(PrivateChatPrivateMessage);
			}

			if (null != this.server)
			{
				this.server.Disconnected -= new EventHandlerDisconnected(ServerDisconnected);
			}
		}

		public void CommandWhoIs()
		{
			if (null != this.privateChat)
			{
				this.privateChat.WhoIs();
			}
		}

		private PivotItem pivotItem = null;
		private Pivot mainWindow = null;
		private PrivateChat privateChat = null;
		private IrcClient server = null;
	}
}

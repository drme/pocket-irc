using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace MyIRC
{
	/// <summary>
	/// irc client class which represents connected client to server.
	/// Has ability to join channels, open private chats, etc.
	/// </summary>
	public class IrcClient : BaseObject
	{
		/// <summary>
		/// Constructs IrcClient class
		/// </summary>
		public IrcClient() : base(null)
		{
			this.user = new ClientUser(this);
		}

		/// <summary>
		/// Connects to specified server. If is connected then disconnects and
		/// connects to given server.
		/// </summary>
		/// <param name="name">server address ex.: irc.dalnet.lt</param>
		/// <param name="port">server port ex.: 6667</param>
		public void Connect(string name, int port)
		{
			lock (this)
			{
				if (name == null)
				{
					this.serverAddress = "127.0.0.1";
				}
				else if ("".Equals(name))
				{
					this.serverAddress = "127.0.0.1";
				}
				else
				{
					this.serverAddress = name;
				}
				this.serverPort = port;

				if (this.IsConnected)
				{
					this.Disconnect();
				}

				WorkerThread();
			}
		}

		/// <summary>
		/// Disconnects from server.
		/// Then disconnected Disconnected event raised
		/// </summary>
		public void Disconnect()
		{
			Disconnect("");
		}

		/// <summary>
		/// Disconnects from server. Waits until client thread is stopped.
		/// </summary>
		/// <param name="message">Quit message</param>
		public void Disconnect(string message)
		{
			if (true == this.IsConnected)
			{
				this.SendMessage("QUIT :" + message);
				this.IsConnected = false;
			}

			OnDisconnected();
		}

		internal void Remove(PrivateChat privateChat)
		{
			this.privates.Remove(privateChat);
		}

		internal void Remove(Channel channel)
		{
			this.channels.Remove(channel);
		}

		/// <summary>
		/// Sends message to the connected server
		/// </summary>
		/// <param name="message">Message to send</param>
		public void SendMessage(string message)
		{
			if (message.Length < 512)
			{
				try
				{
					var asyncEvent = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(this.serverAddress, serverPort) };

					var buffer = Encoding.UTF8.GetBytes(message + Environment.NewLine);
					asyncEvent.SetBuffer(buffer, 0, buffer.Length);

					this.socket.SendAsync(asyncEvent);
				}
				catch (Exception ex)
				{
					PrintException(ex);

					this.IsConnected = false;

					OnDisconnected();
				}
			}
		}

		/// <summary>
		/// Gets user configuration
		/// </summary>
		public IrcUser User
		{
			get
			{
				return this.user;
			}
		}

		/// <summary>
		/// Joins specified channel. On successful join JoinedChannel event is triggered and Channel object is sent.
		/// </summary>
		/// <param name="name">Channel name</param>
		public void JoinChannel(string name, string key)
		{
			SendMessage("JOIN " + name + " " + key);
		}

		/// <summary>
		/// Opens private chat. On successful query nick NewPrivateChat event is triggered and PrivateChat object is sent.
		/// </summary>
		/// <param name="nick">Nick to talk to</param>
		public void QueryNick(string nick)
		{
			if ((null == nick) || ("".Equals(nick)))
			{
				return;
			}

			FindPrivate(nick, true);
		}

		private void AddText(string text)
		{
			if ((null != text) && ("".Equals(text) == false))
			{
				text = text.Trim('\n', '\r');

				OnRawMessageRecieved(text);

				if (false == ParseString(text))
				{
					System.Diagnostics.Debug.WriteLine("[" + text + "]");
				}
			}
		}

		private bool ParseString(string text)
		{
			Regex r = new Regex("[ ]");

			string[] words = r.Split(text);

			if (words.Length == 0)
			{
				return false;
			}

			if ("PING".Equals(words[0]))
			{
				char[] tmp = text.ToCharArray();

				tmp[1] = 'O';

				SendMessage(new string(tmp));

				OnPingPong();

				return true;
			}

			return ParseMessage(text, words);
		}

		private void ReceiveMessage()
		{
			var responseListener = new SocketAsyncEventArgs();
			responseListener.Completed += OnMessageReceivedFromServer;

			var responseBuffer = new byte[1024];
			responseListener.SetBuffer(responseBuffer, 0, 1024);

			this.socket.ReceiveAsync(responseListener);
		}

		private void OnMessageReceivedFromServer(object sender, SocketAsyncEventArgs e)
		{
			var message = Encoding.UTF8.GetString(e.Buffer, 0, e.BytesTransferred);

			var bufferWasPreviouslyFull = !string.IsNullOrWhiteSpace(trailingMessage);

			if (bufferWasPreviouslyFull)
			{
				message = trailingMessage + message;
				trailingMessage = null;
			}

			var isConnectionLost = string.IsNullOrWhiteSpace(message);

			if (isConnectionLost)
			{
				OnDisconnected();

				return;
			}

			var lines = new List<string>(message.Split("\n\r".ToCharArray(), StringSplitOptions.None));

			var lastLine = lines.LastOrDefault();

			var isBufferFull = !string.IsNullOrWhiteSpace(lastLine);

			if (isBufferFull)
			{
				trailingMessage = lastLine;
				lines.Remove(lastLine);
			}

			foreach (var line in lines)
			{
				if (string.IsNullOrWhiteSpace(line))
					continue;

				AddText(line);
			}

			ReceiveMessage();
		}

		/// <summary>
		/// Client worker thread function.
		/// </summary>
		private void WorkerThread()
		{
			byte[] buf = new byte[1024];

			try
			{
				var connectionOperation = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(this.serverAddress, this.serverPort) };
				connectionOperation.Completed += (sender, e) =>
				{
					if (e.SocketError != SocketError.Success)
					{
					}
					else
					{
						this.IsConnected = true;

						OnConnected();

						SendMessage("NICK " + this.user.Nick);
						SendMessage("USER " + this.user.Ident + " localhost " + this.serverAddress + " :" + this.user.RealName);

						ReceiveMessage();
					}
				};

				this.socket.ConnectAsync(connectionOperation);
			}
			catch (Exception ex)
			{
				PrintException(ex);

				lock (this)
				{
					this.IsConnected = false;
				}

				OnDisconnected();

				return;
			}
		}

		private bool ParsePrivMsg(string msg, string[] parameters)
		{
			if (parameters.Length <= 3)
			{
				return false;
			}

			string sender = parameters[0];
			string receiver = parameters[2];

			int pos1 = msg.IndexOf(" :", parameters[0].Length + parameters[1].Length + parameters[2].Length + 2);
			string message = (pos1 >= 0) ? msg.Substring(pos1 + 2) : string.Empty;

			if ((receiver[0] == '#') || (receiver[0] == '&'))
			{
				Channel channel = this.FindChannel(receiver, true);

				if (null != channel)
				{
					channel.OnMessageAdded(sender, message);
					return true;
				}
			}
			else
			{
				PrivateChat p = FindPrivate(sender, true);

				if (null != p)
				{
					p.OnPrivateMessage(sender, message);
					return true;
				}
			}

			return false;
		}

		private bool ParseNoticeMsg(string msg, string[] words)
		{
			if (words.Length <= 3)
			{
				return false;
			}

			if ((words[2][0] == '#') || (words[2][0] == '&'))
			{
				Channel channel = this.FindChannel(words[2], true);

				if (null != channel)
				{
					int pos1 = msg.IndexOf(':', 1);
					channel.OnNoticeAdded(words[0], (pos1 >= 0) ? msg.Substring(pos1 + 1) : null);
					return true;
				}
			}
			else
			{
				if (true == this.createPrivatesForNotice)
				{
					PrivateChat p = FindPrivate(words[0], true);

					if (null != p)
					{
						int pos1 = msg.IndexOf(':', 1);
						p.OnPrivateMessage(words[0], (pos1 >= 0) ? msg.Substring(pos1 + 1) : null);
						return true;
					}
				}
				else
				{
					int pos1 = msg.IndexOf(':', 1);

					OnNotice(new IrcUser(this, words[0]), (pos1 >= 0) ? msg.Substring(pos1 + 1) : null);

					return true;
				}
			}

			return false;
		}

		private bool ParseNamesReply(string msg, string[] words)
		{
			if (words.Length <= 5)
			{
				return false;
			}

			string chanName = words[4];

			if ((chanName[0] == '#') || (chanName[0] == '&'))
			{
				Channel channel = FindChannel(chanName, true);

				if (null != channel)
				{
					int indx = msg.IndexOf(':', 1);

					if (indx >= 0)
					{
						string[] nicks = msg.Substring(indx).Split(new char[2] { ' ', ':' });

						foreach (string nick in nicks)
						{
							channel.AddTempNick(nick);
						}
					}
				}

				return true;
			}

			return false;
		}

		private bool ParseEndOfNamesReply(string msg, string[] words)
		{
			if (words.Length <= 4)
			{
				return false;
			}

			string chanName = words[3];

			if ((chanName[0] == '#') || (chanName[0] == '&'))
			{
				Channel channel = FindChannel(chanName, true);

				if (null != channel)
				{
					channel.SwapNicks();
				}
			}

			return true;
		}

		private bool ParseReplyTopic(string msg, string[] words)
		{
			if (words.Length <= 4)
			{
				return false;
			}

			string chanName = words[3];

			if ((chanName[0] == '#') || (chanName[0] == '&'))
			{
				Channel channel = FindChannel(chanName, true);

				if (null != channel)
				{
					channel.OnTopicRecieved(msg);
					return true;
				}
			}

			return false;
		}

		private bool ParseReplyTopicSetBy(string msg, string[] words)
		{
			if (words.Length == 6)
			{
				string chanName = words[3];

				if (chanName[0] == ':') chanName = words[2].Substring(1);

				if ((chanName[0] == '#') || (chanName[0] == '&'))
				{
					Channel channel = this.FindChannel(chanName, true);

					if (null != channel)
					{
						return true;
					}
				}
			}

			return false;
		}

		private bool ParseJoin(string msg, string[] words)
		{
			if (words.Length == 3)
			{
				string chanName = words[2];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				Channel chan = this.FindChannel(chanName, true);

				if (null != chan)
				{
					chan.OnUserJoined(words[0]);

					return true;
				}
			}

			return false;
		}

		private bool ParsePart(string msg, string[] words)
		{
			if (words.Length > 2)
			{
				string chanName = words[2];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				IrcUser user = new IrcUser(this.server, words[0]);

				if (user.Nick == this.user.Nick)
				{
					return true;
				}

				Channel chan = FindChannel(chanName, true);

				if (null != chan)
				{
					int pos = msg.IndexOf(':', 1);

					chan.OnUserParted(words[0], (pos >= 0) ? msg.Substring(pos + 1) : null);

					return true;
				}
			}

			return false;
		}

		private bool ParseMode(string msg, string[] words)
		{
			if (words.Length > 2)
			{
				string chanName = words[2];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				if (false == IsChannel(chanName))
				{
					return false;
				}

				Channel channel = this.FindChannel(chanName, true);

				if (null != channel)
				{
					channel.OnModeChanged(words[0], words);

					return true;
				}
			}

			return false;
		}

		private bool ParseBanListElementRecieved(string msg, string[] words)
		{
			if (words.Length > 5)
			{
				string chanName = words[3];
				if (chanName[0] == ':') chanName = chanName.Substring(1);
				Channel chan = this.FindChannel(chanName, true);

				if (null != chan)
				{
					chan.AddBan(words[4]);
					return true;
				}
			}
			return false;
		}

		private bool ParseModeListRecieved(string msg, string[] words)
		{
			if (words.Length >= 5)
			{
				string chanName = words[3];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				Channel chan = this.FindChannel(chanName, true);

				if (null != chan)
				{
					chan.OnModesListRecieved(words);

					return true;
				}
			}
			return false;
		}

		private bool ParseBanListRecieved(string msg, string[] words)
		{
			if (words.Length >= 4)
			{
				string chanName = words[3];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				Channel chan = this.FindChannel(chanName, true);

				if (null != chan)
				{
					chan.OnBansListRecieved();
					return true;
				}
			}
			return false;
		}

		private bool ParseKick(string msg, string[] words)
		{
			if (words.Length > 3)
			{
				string chanName = words[2];
				if (chanName[0] == ':') chanName = chanName.Substring(1);

				Channel chan = this.FindChannel(chanName, true);

				if (null != chan)
				{
					int pos = msg.IndexOf(':', 1);

					chan.OnUserKicked(words[0], words[3], (pos >= 0) ? msg.Substring(pos + 1) : null);

					return true;
				}
			}
			return false;
		}

		private bool ParseQuit(string msg, string[] words)
		{
			if (words.Length > 1)
			{
				string quitMessage = null;

				int pos = msg.IndexOf(':', 1);
				if (pos >= 0)
				{
					quitMessage = msg.Substring(pos + 1);
				}

				foreach (Channel chan in this.channels)
				{
					chan.OnUserQuited(words[0], quitMessage);
				}

				return true;
			}
			return false;
		}

		private bool ParseNickChanged(string msg, string[] words)
		{
			if (words.Length > 2)
			{
				IrcUser user = FindUser(words[0], true);
				string newNick = (words[2][0] == ':') ? words[2].Substring(1) : words[2];
				newNick = newNick.Trim();

				if (user.Nick.ToUpper().Equals(this.user.Nick.ToUpper()))
				{
					this.user.OnNickChanged(newNick);
				}

				foreach (Channel chan in this.channels)
				{
					chan.OnNickChanged(words[0], newNick);
				}

				string upNick = user.Nick.ToUpper();

				foreach (PrivateChat p in this.privates)
				{
					if (true == p.User.Nick.ToUpper().Equals(upNick))
					{
						((IrcUser)p.User).OnNickChanged(newNick);
						break;
					}
				}

				return true;
			}

			return false;
		}

		private bool ParseTopicChanged(string msg, string[] words)
		{
			if (words.Length > 2)
			{
				string chanName = words[2];

				if (chanName[0] == ':')
				{
					chanName = chanName.Substring(1);
				}

				Channel chan = FindChannel(chanName, true);

				if (null != chan)
				{
					int pos = msg.IndexOf(':', 1);

					chan.OnTopicChanged(words[0], (pos >= 0) ? msg.Substring(pos + 1) : null);

					return true;
				}
			}
			return false;
		}

		private bool ParseMessage(string msg, string[] words)
		{
			if (null == msg)
			{
				return false;
			}

			if (string.Empty.Equals(msg))
			{
				return false;
			}

			if ((msg[0] == ':') && (words.Length > 2))
			{
				string cmd = words[1].ToUpper();

				if ("PRIVMSG".Equals(cmd))
				{
					return ParsePrivMsg(msg, words);
				}
				else if ("NOTICE".Equals(cmd))
				{
					return ParseNoticeMsg(msg, words);
				}
				else if ("353".Equals(cmd))
				{
					return ParseNamesReply(msg, words);
				}
				else if ("366".Equals(cmd))
				{
					return ParseEndOfNamesReply(msg, words);
				}
				else if ("332".Equals(cmd))
				{
					return ParseReplyTopic(msg, words);
				}
				else if ("333".Equals(cmd))
				{
					return ParseReplyTopicSetBy(msg, words);
				}
				else if ("JOIN".Equals(cmd))
				{
					return ParseJoin(msg, words);
				}
				else if ("PART".Equals(cmd))
				{
					return ParsePart(msg, words);
				}
				else if ("MODE".Equals(cmd))
				{
					return ParseMode(msg, words);
				}
				else if ("KICK".Equals(cmd))
				{
					return ParseKick(msg, words);
				}
				else if ("QUIT".Equals(cmd))
				{
					return ParseQuit(msg, words);
				}
				else if ("NICK".Equals(cmd))
				{
					return ParseNickChanged(msg, words);
				}
				else if ("TOPIC".Equals(cmd))
				{
					return ParseTopicChanged(msg, words);
				}
				else if ("367".Equals(cmd))
				{
					return ParseBanListElementRecieved(msg, words);
				}
				else if ("368".Equals(cmd))
				{
					return ParseBanListRecieved(msg, words);
				}
				else if ("322".Equals(cmd))
				{
					return ParseChannelsRecieving(msg, words);
				}
				else if ("323".Equals(cmd))
				{
					return ParseChannelsRecieved(msg, words);
				}
				else if ("324".Equals(cmd))
				{
					return ParseModeListRecieved(msg, words);
				}
				else
				{
					OnUnparsedLeftText(msg);
					return true;
				}
			}

			return false;
		}

		private bool ParseChannelsRecieved(string msg, string[] words)
		{
			if (null != this.ChannelsRecieved)
			{
				this.ChannelsRecieved(this, this.channelsList);
			}

			return true;
		}

		private bool ParseChannelsRecieving(string msg, string[] words)
		{
			if (words.Length > 3)
			{
				String name = words[3];
				String topic = "";

				int indx = msg.IndexOf(':', 1);

				if (indx > 0)
				{
					topic = msg.Substring(indx + 1);
				}

				ChannelInfo channel = new ChannelInfo(name, topic);

				this.channelsList.Add(channel);

				if (null != this.ChannelsRecieving)
				{
					List<ChannelInfo> channels = new List<ChannelInfo>();
					channels.Add(channel);

					this.ChannelsRecieving(this, channels);
				}
			}

			return true;
		}

		private List<ChannelInfo> ChannelsInfo
		{
			get
			{
				if (this.channelsList.Count == 0)
				{
					ListChannels(null);
				}

				return this.channelsList;
			}
		}

		/// <summary>
		/// Searches channel list for given channel. If channel found returns reference to it otherwise returns null.
		/// </summary>
		/// <param name="channelName">channel name to find</param>
		/// <returns>reference to channel if found otherwise null</returns>
		private Channel FindChannel(string channelName, bool createNew)
		{
			string name1 = channelName.Trim();

			string name = (name1[0] == ':') ? name1.Substring(1) : name1;

			if ((name[0] != '#') && (name[0] != '&'))
			{
				name = "#" + name;
			}

			string nameUpr = name.ToUpper();

			foreach (Channel chan in this.channels)
			{
				if (true == nameUpr.Equals(chan.Name.ToUpper()))
				{
					return chan;
				}
			}

			if (true == createNew)
			{
				Channel chan = new Channel(name, this);
				this.channels.Add(chan);
				OnChannelJoined(chan);
				return chan;
			}

			return null;
		}

		private PrivateChat FindPrivate(string userId, bool createNew)
		{
			userId = userId.ToUpper();

			foreach (PrivateChat p in this.privates)
			{
				if (true == userId.Equals(p.User.FullId.ToUpper()))
				{
					return p;
				}
			}

			string[] words = userId.Split(new char[3] { ':', '!', '@' });

			if (words.Length > 1)
			{
				words[1] = words[1].ToUpper();

				foreach (PrivateChat p in this.privates)
				{
					{
						if (true == words[1].Equals(p.User.Nick.ToUpper()))
						{
							return p;
						}
					}
				}
			}

			if (true == createNew)
			{
				IrcUser usr = FindUser(userId, true);

				if (null == usr)
				{
					return null;
				}

				PrivateChat p = new PrivateChat(this, usr);
				this.privates.Add(p);
				OnPrivateChatCreated(p);
				return p;
			}
			else
			{
				return null;
			}
		}

		internal IrcUser FindUser(string fullId, bool createNew)
		{
			fullId = fullId.ToUpper();

			foreach (IrcUser user in this.users)
			{
				if (true == fullId.Equals(user.FullId.ToUpper()))
				{
					return user;
				}
			}

			if (true == createNew)
			{
				IrcUser user = new IrcUser(this, fullId);

				this.users.Add(user);

				return user;
			}
			else
			{
				return null;
			}
		}

		private void OnRawMessageRecieved(string message)
		{
			if (null != this.RawMessageRecieved)
			{
				this.RawMessageRecieved(this, message);
			}
		}

		private void OnPingPong()
		{
			if (null != this.PingPong)
			{
				this.PingPong(this);
			}
		}

		private void OnConnected()
		{
			if (null != this.Connected)
			{
				this.Connected(this);
			}
		}

		internal override void OnDisconnected()
		{
			base.OnDisconnected();

			try
			{
				foreach (Channel chan in this.channels)
				{
					chan.OnDisconnected();
				}

				foreach (PrivateChat priv in this.privates)
				{
					priv.OnDisconnected();
				}
			}
			catch (Exception ex)
			{
				PrintException(ex);
			}

			this.channels.Clear();
			this.privates.Clear();

			if (null != this.Disconnected)
			{
				this.Disconnected(this);
			}
		}

		private void OnUnparsedLeftText(string text)
		{
			if (null != this.UnparsedLeftText)
			{
				this.UnparsedLeftText(this, text);
			}
		}

		private void OnChannelJoined(Channel chan)
		{
			if ((null != this.ChannelJoined) && (null != chan))
			{
				this.ChannelJoined(this, chan);
			}
		}

		private void OnPrivateChatCreated(PrivateChat privateChat)
		{
			if ((null != this.PrivateChatCreated) && (null != privateChat))
			{
				this.PrivateChatCreated(this, privateChat);
			}
		}

		private void OnNotice(IrcUser user, string message)
		{
			if (null != this.Notice)
			{
				this.Notice(this, user, message);
			}
		}

		private void PrintException(Exception ex)
		{
			Debug.WriteLine(ex.Message);
		}

		private bool IsChannel(string name)
		{
			if ((name[0] == '#') || (name[0] == '&'))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Should the private chat be created for notice messages
		/// </summary>
		public bool CreatePrivatesForNotice
		{
			get
			{
				return this.createPrivatesForNotice;
			}
			set
			{
				this.createPrivatesForNotice = value;
			}
		}

		/// <summary>
		/// Server host name
		/// </summary>
		public String ServerName
		{
			get
			{
				return this.serverAddress;
			}
		}

		/// <summary>
		/// Invokes get channels command on server.
		/// Then channel names are retrieved ChannelsRecieving and ChannelsRecieved events are raised.
		/// </summary>
		/// <param name="message">Channel names of interest, or null for all channels</param>
		public void ListChannels(String message)
		{
			if ((null != message) && (message.Length > 0))
			{
				SendMessage("LIST " + message);
			}
			else
			{
				SendMessage("LIST");
			}
		}

		public event EventHandlerPingPong PingPong = null;
		public event EventHandlerRawMessage RawMessageRecieved;
		public event EventHandlerConnected Connected;
		public event EventHandlerDisconnected Disconnected;
		public event EventHandlerUnparsedLeftText UnparsedLeftText;
		public event EventHandlerChannelJoined ChannelJoined;
		public event EventHandlerPrivateChatCreated PrivateChatCreated;
		public event EventHandlerNotice Notice;
		public event EventHandlerChannelListRecieving ChannelsRecieving;
		public event EventHandlerChannelListRecieved ChannelsRecieved;

		private List<PrivateChat> privates = new List<PrivateChat>();
		private List<IrcUser> users = new List<IrcUser>();
		private List<Channel> channels = new List<Channel>();
		private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private ClientUser user = null;
		private string serverAddress = null;
		private int serverPort = 6667;
		private bool createPrivatesForNotice = false;
		private List<ChannelInfo> channelsList = new List<ChannelInfo>();
		private string trailingMessage;
	}

	/// <summary>
	/// PING PONG event
	/// </summary>
	public delegate void EventHandlerPingPong(IrcClient server);
	/// <summary>
	/// Event handler for handling raw messages. It receives all messages from server.
	/// </summary>
	public delegate void EventHandlerRawMessage(IrcClient server, string msg);
	/// <summary>
	/// Connected to the server event
	/// </summary>
	public delegate void EventHandlerConnected(IrcClient server);
	/// <summary>
	/// Disconnected event
	/// </summary>
	public delegate void EventHandlerDisconnected(IrcClient server);
	/// <summary>
	/// Any other message event that was not understood by irc client
	/// </summary>
	public delegate void EventHandlerUnparsedLeftText(IrcClient server, string text);
	/// <summary>
	/// User has joined the channel
	/// </summary>
	public delegate void EventHandlerChannelJoined(IrcClient server, Channel channel);
	/// <summary>
	/// New private chat opened by other user
	/// </summary>
	public delegate void EventHandlerPrivateChatCreated(IrcClient server, PrivateChat privateChat);
	/// <summary>
	/// Incoming notice event
	/// </summary>
	public delegate void EventHandlerNotice(IrcClient server, IrcUser user, string message);
	/// <summary>
	/// The part of the list of available channels on the server is received
	/// </summary>
	public delegate void EventHandlerChannelListRecieving(IrcClient server, List<ChannelInfo> channels);
	/// <summary>
	/// The full list of available channels on the server is received
	/// </summary>
	public delegate void EventHandlerChannelListRecieved(IrcClient server, List<ChannelInfo> channels);
}

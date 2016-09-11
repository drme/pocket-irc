using System;
using System.Collections.Generic;

namespace MyIRC
{
	/// <summary>
	/// Channel interface. Has events for joining users, leaving, quitting,
	/// nick changes, topic changes, mode changes, messages, notices. Ability to write messages to channel,
	/// change topic, leave channel, change modes.
	/// </summary>
	public class Channel : BaseObject
	{
		internal Channel(string name, IrcClient server) : base(server)
		{
			this.name = name;
			this.server = server;

			this.topic = new Topic(this.server, this);
		}

		/// <summary>
		/// Says text line to channel
		/// </summary>
		/// <param name="text">text to say</param>
		public void SayTextLine(string text)
		{
			if ((null != text) && (text.Length > 0) && (null != this.server))
			{
				this.server.SendMessage("PRIVMSG " + this.name + " :" + text);
			}
		}

		/// <summary>
		/// Parts channel
		/// </summary>
		/// <param name="message">channel part message</param>
		public void Part(string message)
		{
			if (null != this.server)
			{
				string end = "";

				if ((null != message) && (message.Length > 0))
				{
					end = " " + message;
				}

				this.server.SendMessage("PART " + this.name + end);
				this.server.Remove(this);
			}
		}

		/// <summary>
		/// Parts channel
		/// </summary>
		public void Part()
		{
			Part("");
		}

		/// <summary>
		/// Sends given command to server
		/// </summary>
		/// <param name="cmd">command to send</param>
		public void SendMessage(string cmd)
		{
			if (null != this.server) this.server.SendMessage(cmd);
		}

		/// <summary>
		/// Gets specified nick
		/// </summary>
		/// <param name="nick">nick to get</param>
		/// <returns>Reference to nick class if not found null (Nothing in VB)</returns>
		public ChannelNick GetNick(string nick)
		{
			string nck = nick.ToUpper();

			foreach (ChannelNick n in this.nickList)
			{
				if (n.Name.ToUpper().Equals(nck))
				{
					return n;
				}
			}
			return null;
		}

		/// <summary>
		/// Channel's name
		/// </summary>
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		/// <summary>
		/// Channel's topic
		/// </summary>
		public Topic Topic
		{
			get
			{
				return this.topic;
			}
		}

		/// <summary>
		/// Nicks of people in channel
		/// </summary>
		public List<ChannelNick> Nicks
		{
			get
			{
				return this.nickList;
			}
		}

		/// <summary>
		/// Sets mode to channel
		/// </summary>
		/// <param name="mode">mode to set</param>
		public void SetMode(string mode)
		{
			if ((null != mode) && (null != this.server))
			{
				this.server.SendMessage("MODE " + this.name + " " + mode);
			}
		}

		/// <summary>
		/// Kicks person out of channel
		/// </summary>
		/// <param name="nick">Nick to kick</param>
		/// <param name="reason">Reason of kick</param>
		public void Kick(string nick, string reason)
		{
			if ((null != nick) && ("".Equals(nick) == false) && (null != this.server))
			{
				string reason1 = (null == reason) ? "" : reason;
				this.server.SendMessage("KICK " + this.name + " " + nick + " " + reason1);
			}
		}

		/// <summary>
		/// Kicks person out of channel
		/// </summary>
		/// <param name="nick">Nick to kick</param>
		public void Kick(string nick)
		{
			Kick(nick, "");
		}

		internal void AddTempNick(string nick)
		{
			if (null == nick)
			{
				return;
			}

			if ("".Equals(nick))
			{
				return;
			}

			ChannelNick n = null;

			if ((nick[0] == '@') || (nick[0] == '+') || (nick[0] == '%'))
			{
				n = new ChannelNick(nick.Substring(1));

				switch (nick[0])
				{
					case '@':
						n.IsOperator = true;
						break;
					case '%':
						n.IsHalfOperator = true;
						break;
					case '+':
						n.HasVoice = true;
						break;
				}
			}
			else
			{
				n = new ChannelNick(nick);
			}

			if (null == this.tmpNickList)
			{
				this.tmpNickList = new List<ChannelNick>();
			}

			this.tmpNickList.Add(n);
		}

		private void AddNick(string nick)
		{
			ChannelNick p = new ChannelNick(nick);
			this.nickList.Add(p);
			this.nickList.Sort(nickComp);
		}

		internal void SwapNicks()
		{
			this.nickList = this.tmpNickList;
			this.tmpNickList = null;
			this.nickList.Sort(nickComp);
			OnNicksRecieved();
		}

		internal void AddBan(string ban)
		{
			this.banList.Add(ban);
		}

		internal void SetTopic(string topic)
		{
			this.server.SendMessage("TOPIC " + this.name + " :" + topic);
		}

		private bool RenameNick(string oldNick, string newNick)
		{
			string old = oldNick.ToUpper();

			foreach (ChannelNick nick in this.nickList)
			{
				if (nick.Name.ToUpper().Equals(old))
				{
					this.nickList.Remove(nick);
					nick.Name = newNick;
					this.nickList.Add(nick);
					this.nickList.Sort(nickComp);
					return true;
				}
			}

			return false;
		}

		private bool RemoveNick(string nick)
		{
			string n = nick.ToUpper();

			for (int i = 0; i < this.nickList.Count; i++)
			{
				if (((ChannelNick)this.nickList[i]).Name.ToUpper().Equals(n))
				{
					this.nickList.RemoveAt(i);
					return true;
				}
			}

			return false;
		}

		private void AddModeUser(string user, char mode)
		{
			string u = user.ToUpper();

			foreach (ChannelNick n in this.nickList)
			{
				if (u.Equals(n.Name.ToUpper()))
				{
					switch (mode)
					{
						case 'o':
							n.IsOperator = true;
							break;
						case 'h':
							n.IsHalfOperator = true;
							break;
						case 'v':
							n.HasVoice = true;
							break;
					}
					return;
				}
			}
		}

		private void RemoveModeUser(string user, char mode)
		{
			string u = user.ToUpper();

			foreach (ChannelNick n in this.nickList)
			{
				if (u.Equals(n.Name.ToUpper()))
				{
					switch (mode)
					{
						case 'o':
							n.IsOperator = false;
							break;
						case 'h':
							n.IsHalfOperator = false;
							break;
						case 'v':
							n.HasVoice = false;
							break;
					}
					return;
				}
			}
		}

		internal override void OnDisconnected()
		{
			base.OnDisconnected();

			if (null != this.Disconnected)
			{
				this.Disconnected(this.server, this);
			}
		}

		internal virtual void OnTopicChanged(string who, string topic)
		{
			IrcUser user = new IrcUser(this.server, who);

			this.topic.SetTopic((null == topic) ? "" : topic, user.Nick, DateTime.Now);

			if (null != this.TopicChanged)
			{
				this.TopicChanged(this.server, this, this.topic.Text, this.topic.Author);
			}
		}

		internal virtual void OnNicksRecieved()
		{
			if (null != this.NicksRecieved)
			{
				this.NicksRecieved(this.server, this);
			}
		}

		internal virtual void OnTopicRecieved(string msg)
		{
			this.topic.SetTopic(msg);

			if (null != this.TopicChanged)
			{
				this.TopicRecieved(this.server, this, this.topic.Text, this.topic.Author, this.topic.Date);
			}
		}

		internal virtual void OnUserJoined(string fullId)
		{
			IrcUser user = new IrcUser(this.server, fullId);

			AddNick(user.Nick);

			if (null != this.UserJoined)
			{
				this.UserJoined(this.server, this, user);
			}
		}

		internal virtual void OnUserParted(string fullId, string message)
		{
			IrcUser user = new IrcUser(this.server, fullId);

			RemoveNick(user.Nick);

			if (null != this.UserParted)
			{
				this.UserParted(this.server, this, user, message);
			}
		}

		internal virtual void OnModeChanged(string fullId, string[] message)
		{
			IrcUser user = new IrcUser(this.server, fullId);

			bool add = false;
			bool spec = false;

			string s = message[3];
			int pos = 4;

			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '-')
				{
					add = false;
					spec = true;
				}
				else if (s[i] == '+')
				{
					add = true;
					spec = true;
				}
				else if ((s[i] == 'o') || (s[i] == 'v'))
				{
					if (spec)
					{
						if (add == true)
						{
							AddModeUser(message[pos++], s[i]);
						}
						else
						{
							RemoveModeUser(message[pos++], s[i]);
						}
					}
				}
			}

			if (null != this.ModeChanged)
			{
				this.ModeChanged(this.server, this, user, message);
			}
		}

		internal virtual void OnUserKicked(string fullId, string who, string message)
		{
			IrcUser user = new IrcUser(this.server, fullId);
			RemoveNick(who);

			if (null != this.UserKicked)
			{
				this.UserKicked(this.server, this, user, who, message);
			}

			if (who.ToUpper().Equals(this.server.User.Nick.ToUpper()))
			{
				if (null != this.Kicked)
				{
					this.Kicked(this.server, this, user, who, message);
				}
			}
		}

		internal virtual void OnUserQuited(string fullId, string message)
		{
			IrcUser user = new IrcUser(this.server, fullId);

			if (this.RemoveNick(user.Nick))
			{
				if (null != this.UserQuited)
				{
					this.UserQuited(this.server, this, user, message);
				}
			}
		}

		internal virtual void OnNickChanged(string fullId, string toWho)
		{
			IrcUser user = new IrcUser(this.server, fullId);

			string oldNick = user.Nick.ToUpper();

			if (RenameNick(oldNick, toWho))
			{
				if (null != this.NickChanged)
				{
					this.NickChanged(this.server, this, user, toWho);
				}
			}
		}

		internal virtual void OnBansListRecieved()
		{
			if (null != this.BansListRecieved)
			{
				string[] lst = this.banList.ToArray();
				this.BansListRecieved(this.server, this, lst);
			}

			this.banList.Clear();
		}

		internal virtual void OnModesListRecieved(string[] parameters)
		{
			if (null != this.ModesListRecieved)
			{
				string[] list = new string[parameters.Length - 4];

				for (int i = 0; i < parameters.Length - 4; i++)
				{
					list[i] = parameters[4 + i];
				}

				this.ModesListRecieved(this.server, this, list);
			}
		}

		internal virtual void OnMessageAdded(string userId, string message)
		{
			if (null != this.MessageAdded)
			{
				IrcUser user = new IrcUser(this.server, userId);

				this.MessageAdded(this.server, this, user, message);
			}
		}

		internal virtual void OnNoticeAdded(string userId, string message)
		{
			if (null != this.NoticeAdded)
			{
				IrcUser user = new IrcUser(this.server, userId);

				this.NoticeAdded(this.server, this, user, message);
			}
		}

		internal class NicksComparer : IComparer<ChannelNick>
		{
			public int Compare(ChannelNick x, ChannelNick y)
			{
				return string.Compare(x.ToString(), y.ToString());
			}
		}

		public event EventHandlerMessageAdded MessageAdded;
		public event EventHandlerNoticeAdded NoticeAdded;
		public event EventHandlerKicked Kicked;
		public event EventHandlerDisconnectedChannel Disconnected;
		public event EventHandlerTopicChanged TopicChanged;
		public event EventHandlerNicksRecieved NicksRecieved;
		public event EventHandlerTopicRecieved TopicRecieved;
		public event EventHandlerUserJoinedChannel UserJoined;
		public event EventHandlerUserPartedChannel UserParted;
		public event EventHandlerModeChanged ModeChanged;
		public event EventHandlerUserKicked UserKicked;
		public event EventHandlerUserQuitedChannel UserQuited;
		public event EventHandlerUserNickChanngedChannel NickChanged;
		public event EventHandlerBansListRecieved BansListRecieved;
		public event EventHandlerModesListRecieved ModesListRecieved;
		private List<ChannelNick> tmpNickList = null;
		private List<String> banList = new List<String>();
		private List<ChannelNick> nickList = new List<ChannelNick>();
		private string name = "";
		private Topic topic = null;
		private static NicksComparer nickComp = new NicksComparer();
	}

	/// <summary>
	/// somebody says something to channel
	/// </summary>
	public delegate void EventHandlerMessageAdded(IrcClient server, Channel channel, IrcUser user, string message);
	/// <summary>
	/// Notice to channel received
	/// </summary>
	public delegate void EventHandlerNoticeAdded(IrcClient server, Channel channel, IrcUser user, string message);
	/// <summary>
	/// You have been kicked of the channel
	/// </summary>
	public delegate void EventHandlerKicked(IrcClient server, Channel channel, IrcUser user, string who, string message);
	/// <summary>
	/// Disconnected from the server
	/// </summary>
	public delegate void EventHandlerDisconnectedChannel(IrcClient server, Channel channel);
	/// <summary>
	/// Topic changed
	/// </summary>
	public delegate void EventHandlerTopicChanged(IrcClient server, Channel channel, string newTopic, string changedBy);
	/// <summary>
	/// Server sends channel's nicks list
	/// </summary>
	public delegate void EventHandlerNicksRecieved(IrcClient server, Channel channel);
	/// <summary>
	/// Server sends channel's topic
	/// </summary>
	public delegate void EventHandlerTopicRecieved(IrcClient server, Channel channel, string topic, string author, DateTime setDate);
	/// <summary>
	/// User has entered the channel
	/// </summary>
	public delegate void EventHandlerUserJoinedChannel(IrcClient server, Channel chan, IrcUser user);
	/// <summary>
	/// User has parted the channel
	/// </summary>
	public delegate void EventHandlerUserPartedChannel(IrcClient server, Channel chan, IrcUser user, string message);
	/// <summary>
	/// Channel mode has changed
	/// </summary>
	public delegate void EventHandlerModeChanged(IrcClient server, Channel chan, IrcUser user, string[] mode);
	/// <summary>
	/// User has been kicked from the channel
	/// </summary>
	public delegate void EventHandlerUserKicked(IrcClient server, Channel chan, IrcUser user, string who, string message);
	/// <summary>
	/// User has quitted the channel
	/// </summary>
	public delegate void EventHandlerUserQuitedChannel(IrcClient server, Channel chan, IrcUser user, string message);
	/// <summary>
	/// One of the uses in the channel nick has changed
	/// </summary>
	public delegate void EventHandlerUserNickChanngedChannel(IrcClient server, Channel chan, IrcUser user, string newNick);
	/// <summary>
	/// The bans list channel has been received
	/// </summary>
	public delegate void EventHandlerBansListRecieved(IrcClient server, Channel chan, string[] bans);
	/// <summary>
	/// The channel modes list has been received
	/// </summary>
	public delegate void EventHandlerModesListRecieved(IrcClient server, Channel chan, string[] modeStrings);
}

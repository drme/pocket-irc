using System;

namespace MyIRC
{
	/// <summary>
	/// Nick changed
	/// </summary>
	public delegate void EventHandlerNickChanged(IrcClient server, IrcUser user, string oldNick, string newNick);

	/// <summary>
	/// User connected to irc. Has nick, identity, hostname
	/// </summary>
	public class IrcUser : BaseObject
	{
		internal IrcUser(IrcClient server, String fullName) : base(server)
		{
			this.fullName = fullName;

			string v1 = "";
			string v2 = "";
			string v3 = "";

			string[] words = fullName.Split(new char[3] { ':', '!', '@' });

			if (words.Length > 1) v1 = words[1];
			if (words.Length > 2) v2 = words[2];
			if (words.Length > 3) v3 = words[3];

			if (words.Length == 1)
			{
				v1 = words[0];
			}

			SetData(v1, v2, v3);
		}

		internal IrcUser(IrcClient server, string nick, string ident, string hostName) : base(server)
		{
			this.fullName = nick + "!" + ident + "@" + hostName;

			SetData(nick, ident, hostName);
		}

		private void SetData(string nick, string ident, string hostName)
		{
			this.nick = nick;
			this.ident = ident;
			this.hostName = hostName;
		}

		/// <summary>
		/// Client nick name
		/// </summary>
		public virtual string Nick
		{
			get
			{
				return this.nick;
			}
			set
			{
			}
		}

		/// <summary>
		/// Client identity string
		/// </summary>
		public virtual string Ident
		{
			get
			{
				return this.ident;
			}
			set
			{
			}
		}

		/// <summary>
		/// Client host name
		/// </summary>
		public virtual string HostName
		{
			get
			{
				return this.hostName;
			}
		}

		/// <summary>
		/// Real Name
		/// </summary>
		public virtual string RealName
		{
			get
			{
				return this.realName;
			}
			set
			{
			}
		}

		/// <summary>
		/// Full user id
		/// </summary>
		public virtual string FullId
		{
			get
			{
				return this.fullName;
			}
		}

		internal virtual void OnNickChanged(string newNick)
		{
			string oldNick = this.nick;

			this.nick = newNick;
			this.fullName = this.nick + "!" + this.ident + "@" + this.hostName;

			if (null != this.NickChanged)
			{
				this.NickChanged(this.server, this, oldNick, newNick);
			}
		}

		/// <summary>
		/// Then nick changed
		/// </summary>
		public event EventHandlerNickChanged NickChanged;
		protected string nick = null;
		protected string ident = null;
		protected string hostName = null;
		protected string fullName = null;
		protected string realName = null;
	}
}

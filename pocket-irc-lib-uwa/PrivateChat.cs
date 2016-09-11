using System;

namespace MyIRC
{
	/// <summary>
	/// Private chat, represents person and chat text
	/// </summary>
	public class PrivateChat : BaseObject
	{
		internal PrivateChat(IrcClient server, IrcUser user) : base(server)
		{
			this.user = user;
			this.user.NickChanged += new EventHandlerNickChanged(UserNickChanged);
		}

		/// <summary>
		/// Sends text to that person
		/// </summary>
		/// <param name="text">Text to say</param>
		public void SayTextLine(String text)
		{
			this.server.SendMessage("PRIVMSG " + this.User.Nick + " :" + text);
		}

		/// <summary>
		/// Sends WHOIS command
		/// </summary>
		public void WhoIs()
		{
			this.server.SendMessage("WHOIS " + this.User.Nick);
		}

		/// <summary>
		/// Closes private chat
		/// </summary>
		public void Close()
		{
			this.user.NickChanged -= new EventHandlerNickChanged(UserNickChanged);
			this.server.Remove(this);
		}

		/// <summary>
		/// User belonging to this private chat
		/// </summary>
		public IrcUser User
		{
			get
			{
				return this.user;
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

		internal virtual void OnPrivateMessage(string fullId, string message)
		{
			if (null != this.PrivateMessage)
			{
				this.PrivateMessage(this.server, this, this.server.FindUser(fullId, true), message);
			}
		}

		internal virtual void OnNickChanged(string oldNick, string newNick)
		{
			if (null != this.NickChanged)
			{
				this.NickChanged(this.server, this, oldNick, newNick);
			}
		}

		private void UserNickChanged(IrcClient server, IrcUser user, string oldNick, string newNick)
		{
			this.OnNickChanged(oldNick, newNick);
		}

		public event EventHandlerDisconnectedPrivate Disconnected;
		public event EventHandlerPrivMsgPrivate PrivateMessage;
		public event EventHandlerNickChangedPrivate NickChanged;
		private IrcUser user = null;
	}

	/// <summary>
	/// Disconnected form the server
	/// </summary>
	public delegate void EventHandlerDisconnectedPrivate(IrcClient server, PrivateChat privateChat);
	/// <summary>
	/// Private message received
	/// </summary>
	public delegate void EventHandlerPrivMsgPrivate(IrcClient server, PrivateChat priv, IrcUser user, string message);
	/// <summary>
	/// Nick changed
	/// </summary>
	public delegate void EventHandlerNickChangedPrivate(IrcClient server, PrivateChat privateChat, string oldNick, string newNick);
}

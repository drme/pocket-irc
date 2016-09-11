using System;

namespace MyIRC
{
	/// <summary>
	/// This class represents channel topic.
	/// Contains topic text, nick, who changed it and date.
	/// Gives ability to change topic by modifying property Topic
	/// </summary>
	public class Topic : BaseObject
	{
		internal Topic(IrcClient server, Channel channel) : base(server)
		{
			this.channel = channel;
		}

		/// <summary>
		/// Sets and Gets channel Topic
		/// </summary>
		public string Text
		{
			get
			{
				return (null == this.topic) ? "" : this.topic;
			}
			set
			{
				this.channel.SetTopic(value);
			}
		}

		/// <summary>
		/// The nick of the person who modified this topic
		/// </summary>
		public string Author
		{
			get
			{
				return (null == this.setBy) ? "" : this.setBy;
			}
		}

		/// <summary>
		/// When topic was set
		/// </summary>
		public DateTime Date
		{
			get
			{
				return this.date;
			}
		}

		internal void SetTopic(string message)
		{
			int pos = message.IndexOf(':', 1);

			if (pos >= 0)
			{
				if (message.Length == pos)
				{
					this.topic = null;
				}
				else
				{
					this.topic = message.Substring(pos + 1);
				}
			}
		}

		internal void SetTopic(string topic, string who, DateTime date)
		{
			this.topic = topic;
			this.setBy = who;
			this.date = date;
		}

		internal void SetDetails(string nick, DateTime date)
		{
			this.setBy = nick;
			this.date = date;
		}

		internal void SetBy(string nick)
		{
			this.setBy = nick;
		}

		private Channel channel = null;
		private string topic = null;
		private string setBy = null;
		private DateTime date = DateTime.Now;
	}
}

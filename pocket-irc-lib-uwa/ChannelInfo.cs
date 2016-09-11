using System;

namespace MyIRC
{
	/// <summary>
	/// Holds information about channel
	/// </summary>
	public class ChannelInfo
	{
		internal ChannelInfo(String name, String topic)
		{
			this.Name = name;
			this.Topic = topic;
		}

		/// <summary>
		/// Channel name
		/// </summary>
		public String Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Channel topic.
		/// </summary>
		public String Topic
		{
			get;
			private set;
		}

		public override String ToString()
		{
			return this.Name + " " + this.Topic;
		}
	}
}

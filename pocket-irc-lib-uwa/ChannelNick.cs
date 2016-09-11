using System;

namespace MyIRC
{
	/// <summary>
	/// Represents person in a channel, contains its nickname and flags
	/// </summary>
	public class ChannelNick
	{
		/// <summary>
		/// Constructs ChannelNick object
		/// </summary>
		/// <param name="name">NickName</param>
		public ChannelNick(string name)
		{
			this.Name = name;
			this.HasVoice = false;
			this.IsOperator = false;
			this.IsHalfOperator = false;
		}

		/// <summary>
		/// Gets NickName
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Returns if user has voice flag
		/// </summary>
		public bool HasVoice
		{
			get;
			internal set;
		}

		/// <summary>
		/// Returns if user has op
		/// </summary>
		public bool IsOperator
		{
			get;
			internal set;
		}

		/// <summary>
		/// Return if user has half op
		/// </summary>
		public bool IsHalfOperator
		{
			get;
			internal set;
		}

		/// <summary>
		/// Returns NicName + flags
		/// </summary>
		/// <returns>NickName with flags</returns>
		public override string ToString()
		{
			string flag = "";

			if (this.IsOperator)
			{
				flag = "@";
			}
			else if (this.IsHalfOperator)
			{
				flag = "%";
			}
			else if (this.HasVoice)
			{
				flag = "+";
			}

			return flag + this.Name;
		}
	}
}

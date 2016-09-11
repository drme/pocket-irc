using System;
using System.Text.RegularExpressions;

namespace MyIRC
{
	/// <summary>
	/// User info
	/// </summary>
	internal class ClientUser : IrcUser
	{
		internal ClientUser(IrcClient server) : base(server, string.Empty, string.Empty, string.Empty)
		{
			this.nick = "Charlie" + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");
			this.realName = "Charlie Root";
			this.ident = "charlieRoot";
			this.hostName = "localhost";
		}

		/// <summary>
		/// Sets, gets nick
		/// </summary>
		public override string Nick
		{
			get
			{
				return base.Nick;
			}
			set
			{
				if (null != value)
				{
					Regex r = new Regex("[ \t]");

					string[] val = r.Split(value);

					if (val.Length > 0)
					{
						this.UpdateNick(val[0]);
					}
				}
			}
		}

		/// <summary>
		/// User identity
		/// </summary>
		public override string Ident
		{
			get
			{
				return base.Ident;
			}
			set
			{
				if (null != value)
				{
					Regex r = new Regex("[ \t]");

					string[] val = r.Split(value);

					if (val.Length > 0)
					{
						this.ident = val[0];
					}
				}
			}
		}

		/// <summary>
		/// Real name
		/// </summary>
		public override string RealName
		{
			get
			{
				return base.RealName;
			}
			set
			{
				if ((null != value) && (value.Length > 0))
				{
					this.realName = value;
				}
			}
		}

		private void UpdateNick(string nick)
		{
			if (this.server.IsConnected)
			{
				this.server.SendMessage("NICK " + nick);
			}
			else
			{
				OnNickChanged(nick);
			}
		}
	}
}

using Windows.Storage;

namespace pocket_irc_uwa
{
	class IrcConfig : Config
	{
		internal IrcConfig() : base()
		{
			this.showTime = base.GetValue("ShowTime", false);
			this.showJoins = base.GetValue("ShowJoins", false);
			this.showParts = base.GetValue("ShowParts", false);
			this.showQuits = base.GetValue("ShowQuits", false);
		}

		public string Channel
		{
			get
			{
				return GetValue("channel", "#kamputis");
			}
			set
			{
				SetValue("channel", value);
			}
		}

		public string ChannelKey
		{
			get
			{
				return GetValue("channel_key", "");
			}
			set
			{
				SetValue("channel_key", value);
			}
		}

		public string LastPrivateChat
		{
			get
			{
				return GetValue("last_chat", "ME");
			}
			set
			{
				SetValue("last_chat", value);
			}
		}

		public string Server
		{
			get
			{
				return base.GetValue("Server", "irc.omnitel.net");
			}
			set
			{
				base.SetValue("Server", value);
			}
		}

		public int Port
		{
			get
			{
				return base.GetValue("Port", 6667);
			}
			set
			{
				base.SetValue("Port", value);
			}
		}

		public string NickName
		{
			get
			{
				return base.GetValue("NickName", "PocketUser");
			}
			set
			{
				base.SetValue("NickName", value);
			}
		}

		public string RealName
		{
			get
			{
				return base.GetValue("RealName", "My Pocket IRC");
			}
			set
			{
				base.SetValue("RealName", value);
			}
		}

		public string Password
		{
			get
			{
				return base.GetValue("Password", "");
			}
			set
			{
				base.SetValue("Password", value);
			}
		}

		public string User
		{
			get
			{
				return base.GetValue("User", "mypirc");
			}
			set
			{
				base.SetValue("User", value);
			}
		}

		public bool ShowTime
		{
			get
			{
				return this.showTime;
			}
			set
			{
				this.showTime = value;
				base.SetValue("ShowTime", value);
			}
		}

		public bool ShowJoins
		{
			get
			{
				return this.showJoins;
			}
			set
			{
				this.showJoins = value;
				base.SetValue("ShowJoins", value);
			}
		}

		public bool ShowParts
		{
			get
			{
				return this.showParts;
			}
			set
			{
				this.showParts = value;
				base.SetValue("ShowParts", value);
			}
		}

		public bool ShowQuits
		{
			get
			{
				return this.showQuits;
			}
			set
			{
				this.showQuits = value;
				base.SetValue("ShowQuits", value);
			}
		}

		private bool showTime = true;
		private bool showJoins = true;
		private bool showParts = true;
		private bool showQuits = true;
	}

	class Config
	{
		public string GetValue(string key, string defaultValue)
		{
			if (ApplicationData.Current.LocalSettings.Values[key] != null)
			{
				return ApplicationData.Current.LocalSettings.Values[key].ToString();
			}
			else
			{
				return defaultValue;
			}
		}

		public int GetValue(string key, int defaultValue)
		{
			return int.Parse(GetValue(key, defaultValue.ToString()));
		}

		public bool GetValue(string key, bool defaultValue)
		{
			return bool.Parse(GetValue(key, defaultValue.ToString()));
		}

		public void SetValue(string key, string value)
		{
			ApplicationData.Current.LocalSettings.Values[key] = value;
		}

		public void SetValue(string key, int value)
		{
			SetValue(key, value.ToString());
		}

		public void SetValue(string key, bool value)
		{
			SetValue(key, value.ToString());
		}
	}
}

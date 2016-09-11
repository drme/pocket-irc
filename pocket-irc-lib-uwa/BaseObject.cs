using System;

namespace MyIRC
{
	/// <summary>
	/// Base class for library objects
	/// </summary>
	public abstract class BaseObject
	{
		protected BaseObject(IrcClient server)
		{
			this.server = server;
			this.IsConnected = false;
		}

		internal virtual void OnDisconnected()
		{
			this.IsConnected = false;
		}

		/// <summary>
		/// Is the client connected to the server
		/// </summary>
		public bool IsConnected
		{
			get;
			set;
		}

		protected IrcClient server;
	}
}

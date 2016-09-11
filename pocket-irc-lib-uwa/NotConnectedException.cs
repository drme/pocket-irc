using System;

namespace MyIRC
{
	/// <summary>
	/// Exception then trying to do something on server while being not connected. ex. joining channel, writing message.
	/// </summary>
	public class NotConnectedException : Exception
	{
	}
}

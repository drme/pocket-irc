using MyIRC;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace pocket_irc_uwa
{
	sealed partial class App : Application
	{
		public App()
		{
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			Frame rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
				}

				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}

				Window.Current.Activate();
			}
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			deferral.Complete();
		}

		internal IrcConfig Settings
		{
			get
			{
				return this.settings;
			}
		}

		public ServerControl Server
		{
			get
			{
				return this.server;
			}
			set
			{
				this.server = value;
			}
		}

		public IrcClient Client
		{
			get
			{
				var server = this.server;

				if (null != server)
				{
					return server.Client;
				}

				return null;
			}
		}

		private IrcConfig settings = new IrcConfig();
		private ServerControl server = null;
	}
}

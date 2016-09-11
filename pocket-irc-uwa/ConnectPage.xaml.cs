using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace pocket_irc_uwa
{
	public sealed partial class ConnectPage : Page
	{
		public ConnectPage()
		{
			InitializeComponent();

			this.TextBoxNick.Text = (App.Current as App).Settings.NickName;
			this.TextBoxPassword.Text = (App.Current as App).Settings.Password;
			this.TextBoxRealName.Text = (App.Current as App).Settings.RealName;
			this.TextBoxServer.Text = (App.Current as App).Settings.Server;
			this.TextBoxUser.Text = (App.Current as App).Settings.User;
			this.TextBoxPort.Text = (App.Current as App).Settings.Port.ToString();

			Utils.SetType(this.TextBoxNick, InputScopeNameValue.Text);
			Utils.SetType(this.TextBoxPassword, InputScopeNameValue.Password);
			Utils.SetType(this.TextBoxRealName, InputScopeNameValue.PersonalFullName);
			Utils.SetType(this.TextBoxServer, InputScopeNameValue.Url);
			Utils.SetType(this.TextBoxUser, InputScopeNameValue.EmailNameOrAddress);
			Utils.SetType(this.TextBoxPort, InputScopeNameValue.Number);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			this.mainWindow = (MainPage)e.Parameter;
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}

		private void ApplicationBarIconButtonConnectClick(object sender, RoutedEventArgs e)
		{
			if (this.CheckBoxStupid.IsChecked == false)
			{
				Utils.ShowMessage("Sorry, you are too young to use IRC");
			}
			else
			{
				var app = App.Current as App;

				app.Settings.NickName = this.TextBoxNick.Text;
				app.Settings.Password = this.TextBoxPassword.Text;
				app.Settings.RealName = this.TextBoxRealName.Text;
				app.Settings.Server = this.TextBoxServer.Text;
				app.Settings.User = this.TextBoxUser.Text;

				try
				{
					app.Settings.Port = int.Parse(this.TextBoxPort.Text);

					this.mainWindow.Pages.Items.Clear();

					(App.Current as App).Server = new ServerControl(this.mainWindow.Pages, this.mainWindow);

					PivotItem item = new PivotItem();

					item.Header = app.Server.Client.ServerName;
					item.Content = app.Server;

					this.mainWindow.Pages.Items.Add(item);

					this.Frame.GoBack();
				}
				catch (Exception)
				{
					Utils.ShowMessage("Invalid port number");
				}
			}
		}

		private MainPage mainWindow = null;
	}
}

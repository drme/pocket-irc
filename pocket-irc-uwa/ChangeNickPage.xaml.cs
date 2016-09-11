using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace pocket_irc_uwa
{
	public sealed partial class ChangeNickPage : Page
	{
		public ChangeNickPage()
		{
			InitializeComponent();
			this.TextBoxNick.Text = (App.Current as App).Settings.NickName;
		}

		private void AcceptClick(object sender, RoutedEventArgs e)
		{
			var app = App.Current as App;

			app.Settings.NickName = this.TextBoxNick.Text;

			if (null != app.Client)
			{
				app.Client.User.Nick = app.Settings.NickName;
			}

			this.Frame.GoBack();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}
	}
}

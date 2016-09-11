using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace pocket_irc_uwa
{
	public sealed partial class OpenPrivatePage : Page
	{
		public OpenPrivatePage()
		{
			InitializeComponent();

			this.TextBoxNick.Text = (App.Current as App).Settings.LastPrivateChat;
		}

		private void AcceptClick(object sender, RoutedEventArgs e)
		{
			var app = App.Current as App;

			app.Settings.LastPrivateChat = this.TextBoxNick.Text;

			app.Server?.Client?.QueryNick(app.Settings.LastPrivateChat);

			this.Frame.GoBack();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}
	}
}

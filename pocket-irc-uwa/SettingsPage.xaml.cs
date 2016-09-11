using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace pocket_irc_uwa
{
	public sealed partial class SettingsPage : Page
	{
		public SettingsPage()
		{
			InitializeComponent();

			this.checkBoxJoins.IsChecked = (App.Current as App).Settings.ShowJoins;
			this.checkBoxParts.IsChecked = (App.Current as App).Settings.ShowParts;
			this.checkBoxQuits.IsChecked = (App.Current as App).Settings.ShowQuits;
			this.checkBoxTime.IsChecked = (App.Current as App).Settings.ShowTime;
		}

		private void ApplicationBarIconButtonOkClick(object sender, RoutedEventArgs e)
		{
			(App.Current as App).Settings.ShowJoins = (bool)this.checkBoxJoins.IsChecked;
			(App.Current as App).Settings.ShowParts = (bool)this.checkBoxParts.IsChecked;
			(App.Current as App).Settings.ShowQuits = (bool)this.checkBoxQuits.IsChecked;
			(App.Current as App).Settings.ShowTime = (bool)this.checkBoxTime.IsChecked;

			this.Frame.GoBack();
		}

		private void ApplicationBarIconButtonCancelClick(object sender, RoutedEventArgs e)
		{
			this.Frame.GoBack();
		}
	}
}

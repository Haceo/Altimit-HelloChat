using System.Windows;

namespace HelloWorld
{
    public partial class Settings : Window
    {
        public Config _config;
        public Settings()
        {
            InitializeComponent();
        }
        private void WPF_Loaded(object sender, RoutedEventArgs e)
        {
            if (_config != null)
            {
                tokenBox.Text = _config.Token;
                clientIdBox.Text = _config.Client;
                usernameBox.Text = _config.User;
                oAuthBox.Text = _config.Auth;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (clientIdBox.Text == "" || tokenBox.Text == "" || usernameBox.Text == "" || oAuthBox.Text == "")
            {
                MessageBox.Show("Sorry but you must give all information to save a configuration!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

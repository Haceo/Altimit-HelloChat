using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace HelloWorld
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const string resDir = "Resources";
        public static bool loading = false;
        public static Config config;

        public static TwitchAPI _api;
        public static TwitchClient _client;
        public bool IsConnected = false;
        private ObservableCollection<FirstMessage> messages = new ObservableCollection<FirstMessage>();
        public ObservableCollection<FirstMessage> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                RaisePropertyChanged("Messages");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            FileHandler("load", "config");
            if (config == null)
                config = new Config();
            if (config != null && config.Client != "" && config.Auth != "" && config.Token != "" && config.User != "")
                connectionButton.IsEnabled = true;
            _api = new TwitchAPI();
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static async Task FileHandler(string option, string file)
        {
            switch (option)
            {
                case "load":
                    loading = true;
                    if (!Check(file))
                    {
                        loading = false;
                        return;
                    }
                    else
                    {
                        string loadJson = File.ReadAllText($"{resDir}/{file}.json");
                        switch (file)
                        {
                            case "config":
                                config = new Config();
                                config = JsonConvert.DeserializeObject<Config>(loadJson);
                                break;
                        }
                    }
                    loading = false;
                    break;
                case "save":
                    string saveJson = "";
                    switch (file)
                    {
                        case "config":
                            saveJson = JsonConvert.SerializeObject(config, Formatting.Indented);
                            break;
                    }
                    try
                    {
                        File.WriteAllText($"{resDir}/{file}.json", saveJson);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    break;
            }
        }
        public static bool Check(string file)
        {
            if (!Directory.Exists(resDir))
                Directory.CreateDirectory(resDir);
            if (!File.Exists($"{resDir}/{file}.json"))
                return false;
            else
                return true;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
                ConnectAsync();
            else
                DisconnectAsync();
        }
        private void connectionButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsConnected)
                connectionButton.Content = "Connect!";
            else
                connectionButton.Content = "Disconnect!";
        }
        private void connectionButton_MouseLeave(object sender, MouseEventArgs e)
        {
                connectionButton.Content = "Connected:";
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings sp = new Settings();
            if (config != null)
                sp._config = config;
            sp.Owner = this;
            sp.ShowDialog();
            if (sp.DialogResult.HasValue && sp.DialogResult.Value)
            {
                config.Token = sp.tokenBox.Text;
                config.User = sp.usernameBox.Text;
                config.Auth = sp.oAuthBox.Text;
                config.Client = sp.clientIdBox.Text;
                FileHandler("save", "config");
                connectionButton.IsEnabled = false;
                if (config != null && config.Client != "" && config.Auth != "" && config.Token != "" && config.User != "")
                    connectionButton.IsEnabled = true;
            }
        }
        private void Dismiss_Click(object sender, RoutedEventArgs e)
        {
            if (helloListBox.SelectedIndex == -1)
                return;
            (helloListBox.ItemContainerGenerator.ContainerFromIndex(helloListBox.SelectedIndex) as ListViewItem).Visibility = Visibility.Collapsed;
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            int count = helloListBox.Items.Count;
            for (var i = 0; i < count; i++)
                (helloListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListViewItem).Visibility = Visibility.Collapsed;
        }
        private async Task ConnectAsync()
        {
            if (loading)
            {
                MessageBox.Show("There was a loading error please review config files and try again!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (config.Token == null || config.Token == ""
                || config.Client == null || config.Client == ""
                || config.Auth == null || config.Auth == ""
                || config.User == null || config.User == "")
            {
                MessageBox.Show("Some info is missing, please populate all settings and try again!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            connectionButton.Content = "Connecting...";
            _api.Settings.ClientId = config.Client;
            _api.Settings.AccessToken = config.Token;
            _client = new TwitchClient();
            var cred = new ConnectionCredentials(config.User, config.Auth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient socClient = new WebSocketClient(clientOptions);
            _client.Initialize(cred, channelEntryBox.Text);
            _client.OnMessageReceived += MessageRecievedHandler;
            _client.Connect();
            IsConnected = true;
            connectionLight.Fill = Brushes.Green;
            connectionButton.Content = "Connected:";
        }
        private async Task DisconnectAsync()
        {
            Messages.Clear();
            _client.Disconnect();
            IsConnected = false;
            connectionLight.Fill = Brushes.Red;
        }

        private void MessageRecievedHandler(object sender, OnMessageReceivedArgs e)
        {
            var message = e.ChatMessage;
            var res = Messages.FirstOrDefault(x => x.UserId == message.UserId);
            if (res == null)
            {
                FirstMessage newFirst = new FirstMessage()
                {
                    UserId = message.UserId,
                    Username = message.DisplayName,
                    Message = message.Message,
                    Sent = DateTime.Now
                };
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Messages.Add(newFirst);
                    helloListBox.ScrollIntoView(helloListBox.Items[helloListBox.Items.Count - 1]);
                });
            }
        }
    }
    public class Config
    {
        public string Token { get; set; }
        public string Client { get; set; }
        public string User { get; set; }
        public string Auth { get; set; }
    }
    public class FirstMessage
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public DateTime Sent { get; set; }
    }
}

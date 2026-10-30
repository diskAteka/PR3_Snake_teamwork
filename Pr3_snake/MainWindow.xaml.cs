using System.Diagnostics;
using System.Net;
using System.Security;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Pr3_snake.Pages;

namespace Pr3_snake
{
    
    public partial class MainWindow : Window
    {

        public static MainWindow Instance { get; private set; }
        public ViewModelUserSettings ViewModelUserSettings = new();
        public ViewModelGames ViewModelGames = new();
        public static IPAddress remoteIpAddress = IPAddress.Parse("127.0.0.1");
        public static int remotePort = 5001;
        public Thread tRec;
        public UpdClient recivingUpdClient;
        public Pages.Home HomePage = new();
        public Pages.Game GamePage = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        public void StartReciver()
        {
            tRec = new Thread(new ThreadStart(Receiver));
            tRec.Start();
        }

        public void OpenPage(Page pageToOpen)
        {
            DoubleAnimation startAnimation = new DoubleAnimation();
            startAnimation.From = 1;
            startAnimation.To = 0;
            startAnimation.Duration = TimeSpan.FromSeconds(0.3);
            startAnimation.Completed += delegate
            {
                MainFrame.Navigate(pageToOpen);
                DoubleAnimation endAnimation = new();
                endAnimation.From = 0;
                endAnimation.To = 1;
                endAnimation.Duration = TimeSpan.FromSeconds(0.3);
                MainFrame.BeginAnimation(OpacityProperty, endAnimation);

            };
            MainFrame.BeginAnimation(OpacityProperty, startAnimation);
        }


        public void Receiver()
        {
            recivingUpdClient = new UpdClient(int.Parse(ViewModelUserSettings.Port));
            IPEndPoint RemoteIpEndPoint = null;
            try
            {
                while (true)
                {
                    byte[] receiveBytes = recivingUpdClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.UTF8.GetString(receiveBytes);
                    if(ViewModelGames is null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OpenPage(GamePage);
                        });
                    }

                    ViewModelGames = JsonConvert.DeserializeObject<ViewModelGames>(returnData.ToString());
                    if (ViewModelGames.SnakePlayers.GameOver)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            OpenPage(new Pages.EndGame());
                        });
                    }
                    else
                    {
                        Game.CreateUI();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Возникло исключение: " + ex.ToString() + "\n " + ex.Message);
            }
        }
    }
}
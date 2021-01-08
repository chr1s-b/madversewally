using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.IO;

namespace desktop_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MENU = 0;
        const int LOBBY = 1;
        const int TUTORIAL = 2;
        const int INTRO = 3;
        const int WRITING = 4;
        const int RAPPING = 5;
        const int INTER = 6;
        const int END = 7;
        int appstatus = MENU;
        String msg;

        String server_addr;
        String port;
        public MainWindow(String server_addr, String port)
        {
            this.server_addr = server_addr;
            this.port = port;
            
            InitializeComponent();
        }
        private void connect_button_Click(object sender, RoutedEventArgs e)
        {
            msg = "CODE_REQ";
            DoClientWebSocket(this.server_addr, this.port);
        }

        private async void DoClientWebSocket(String addr, String port)
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri(String.Format("ws://{0}:{1}/", addr, port));
                var source = new CancellationTokenSource();

                await ws.ConnectAsync(serverUri, source.Token);
                Console.WriteLine("Connected to {0}", serverUri);

                while (ws.State == WebSocketState.Open)
                {
                    if (msg != "")
                    {
                        ArraySegment<byte> bytesToSend =
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                        await ws.SendAsync(bytesToSend, WebSocketMessageType.Text,
                                            true, source.Token);
                    }
                    msg = "";

                    //Receive buffer
                    var receiveBuffer = new byte[128];
                    //Multipacket response
                    var offset = 0;
                    var dataPerPacket = 32;
                    while (true)
                    {
                        ArraySegment<byte> bytesReceived =
                                    new ArraySegment<byte>(receiveBuffer, offset, dataPerPacket);
                        WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived,
                                                                        source.Token);
                        offset += result.Count;
                        if (result.EndOfMessage)
                            break;
                    }

                    // Recieved full string
                    String[] response = Encoding.UTF8.GetString(receiveBuffer, 0, offset).Split('.');
                    String server_prompt = response[0];
                    String server_data = "";
                    if (response.Length == 2) { server_data = response[1]; }
                    //Console.WriteLine("Complete response: {0}", server_prompt);

                    switch (server_prompt)
                    {
                        case "CODE_RESP": // server returns game code
                            this.recieved_code(server_data);
                            break;
                        case "PLAYERJOIN":
                            // add player
                            this.addplayer(server_data);
                            break;
                        case "TUTORIALSTART":
                            // close lobby, move to tutorial
                            appstatus = TUTORIAL;
                            this.switchto(this.tutorialXAML);
                            this.showtutorial();
                            break;
                        case "TUTORIALSKIP":
                            // if in tutorial, skip tutorial and move to intro
                            if (appstatus == TUTORIAL) { this.endtutorial(); }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void showtutorial()
        {
            this.tutorialPlayer.MediaEnded += this.tutorialended;
            this.tutorialPlayer.Play();
        }

        private void tutorialended(object sender, EventArgs e)
        {
            this.endtutorial();
        }

        private void endtutorial()
        {
            this.tutorialPlayer.Stop();
            // ending sequence of tutorial, also at end of skipping
            // show intro to first round
            appstatus = INTRO;
            this.switchto(this.introXAML);
            this.introPlayer.MediaEnded += this.startroundone;
            this.introPlayer.Play();
        }

        private void startroundone(object sender, EventArgs e)
        {
            appstatus = WRITING;
            msg = "WRITINGSTART."+this.tbgamecode.Text;
            this.switchto(this.writingXAML);
        }

        private void addplayer(string data)
        {
            // display the player in the lobby
            String[] nameandnumber = data.Split('&');
            Console.WriteLine("Player {0} joined",nameandnumber[1]);
            ((TextBlock)this.players.Children[Int32.Parse(nameandnumber[1])-1]).Text = nameandnumber[0];

        }

        private void recieved_code(string data)
        {
            Console.WriteLine("Recieved room code {0}",data);
            this.tbgamecode.Text = data;
            appstatus = LOBBY;
            this.switchto(this.lobbyXAML);
        }

        private void switchto(Grid makevisible)
        {
            foreach (Grid grid in this.container.Children)
            {
                if (grid == makevisible)
                {
                    grid.Visibility = Visibility.Visible;
                } else
                {
                    grid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void code_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

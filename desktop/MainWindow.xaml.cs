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

namespace desktop_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int MAIN_MENU = 0;
        const int LOBBY = 1;
        int appstatus = MAIN_MENU;
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
            DoClientWebSocket(this.server_addr, this.port);
        }

        private async void DoClientWebSocket(String addr, String port)
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri(String.Format("ws://{0}:{1}/", addr, port));
                var source = new CancellationTokenSource();

                await ws.ConnectAsync(serverUri, source.Token);
                Console.WriteLine("Connected to" + String.Format("ws://{0}:{1}/", addr, port));

                while (ws.State == WebSocketState.Open)
                {
                    switch (appstatus)
                    {
                        case MAIN_MENU:
                            msg = "CODE_REQ";
                            break;
                        default:
                            msg = "";
                            break;
                    }

                    if (msg != "")
                    {
                        ArraySegment<byte> bytesToSend =
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                        await ws.SendAsync(bytesToSend, WebSocketMessageType.Text,
                                            true, source.Token);
                    }
                    
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
                        //Partial data received
                        //Console.WriteLine("Data:{0}", Encoding.UTF8.GetString(receiveBuffer, offset,result.Count));
                        offset += result.Count;
                        if (result.EndOfMessage)
                            break;
                    }

                    // Recieved full string
                    String[] response = Encoding.UTF8.GetString(receiveBuffer, 0, offset).Split('.');
                    String server_prompt = response[0];
                    String server_data = "";
                    if (response.Length == 2) { server_data = response[1]; }
                    Console.WriteLine("Complete response: {0}", server_prompt);

                    switch (server_prompt)
                    {
                        case "CODE_RESP": // server returns game code
                            this.recieved_code(server_data);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void recieved_code(string data)
        {
            Console.WriteLine("Recieved game code" + data);
            this.tbgamecode.Text = data;
            appstatus = LOBBY;
            // now displayed game code to user
            // remove connect button
            this.connect_button.Visibility = Visibility.Collapsed;
        }

        private void code_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}

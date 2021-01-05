using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace desktop_app
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow wnd = new MainWindow();
			String server_addr = "madversewally.herokuapp.com";
            String port = "80";
			if (e.Args.Length == 1)  // override server_addr for debug
				server_addr = e.Args[0];
            if (e.Args.Length == 2)
            { // override server_addr and port for debug
                server_addr = e.Args[0];
                port = e.Args[1];
            }
			Console.WriteLine("[INFO] Server address set to {0}", server_addr);
			wnd.Show();

            DoClientWebSocket(server_addr, port);
        }

        private static async void DoClientWebSocket(String addr, String port)
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri(String.Format("ws://{0}:{1}/", addr, port));
                var source = new CancellationTokenSource();

                await ws.ConnectAsync(serverUri, source.Token);
                while (ws.State == WebSocketState.Open)
                {

                    Console.WriteLine("Connected to" + String.Format("ws://{0}:{1}/", addr, port));
                    string msg = "CODE_REQ";
                    ArraySegment<byte> bytesToSend =
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text,
                                        true, source.Token);
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
                    String server_prompt = Encoding.UTF8.GetString(receiveBuffer, 0, offset);
                    Console.WriteLine("Complete response: {0}", server_prompt);

                    switch (server_prompt)
                    {
                        case "CODE_RESP": // server returns game code
                            Console.WriteLine("Recieved game code" + server_prompt);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

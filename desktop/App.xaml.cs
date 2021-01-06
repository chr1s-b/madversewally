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
            MainWindow wnd = new MainWindow(server_addr,port);
            wnd.Show();
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace WebTrace_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void getRouteButton_Click(object sender, RoutedEventArgs e)
        {
            string hostname = sendTextBox.Text;
            int timeout = 1000; // 1000ms or 1 second.
            int max_TTL = 30;   // # of server to querry.
            int current_TTL = 0;
            const int bufferSize = 32;  // Tracking the # of server found.
            Stopwatch s1 = new Stopwatch();
            Stopwatch s2 = new Stopwatch();
            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            Ping pinger = new Ping();

            Task.Factory.StartNew(() =>
            {
                WriteListBox($"Started ICMP Trace route on {hostname}");
                for (int ttl = 1; ttl <= max_TTL; ttl++)
                {
                    current_TTL++;
                    s1.Start();
                    s2.Start();
                    PingOptions options = new PingOptions(ttl, true);
                    PingReply reply = null;
                    try
                    {
                        reply = pinger.Send(hostname, timeout, buffer, options);
                    }
                    catch
                    {
                        WriteListBox("Error");
                        break; //the rest of the code relies on reply not being null so...
                    }
                    if (reply != null) //dont need this but anyway...
                    {
                        //the traceroute bits :)
                        if (reply.Status == IPStatus.TtlExpired)
                        {
                            //address found after yours on the way to the destination
                            WriteListBox($"[{ttl}] - Route: {reply.Address} - Time: {s1.ElapsedMilliseconds} ms - Total Time: {s2.ElapsedMilliseconds} ms");
                            continue; //continue to the other bits to find more servers
                        }
                        if (reply.Status == IPStatus.TimedOut)
                        {
                            //this would occour if it takes too long for the server to reply or if a server has the ICMP port closed (quite common for this).
                            WriteListBox($"Timeout on {hostname}. Continuing.");
                            continue;
                        }
                        if (reply.Status == IPStatus.Success)
                        {
                            //the ICMP packet has reached the destination (the hostname)
                            WriteListBox($"Successful Trace route to {hostname} in {s1.ElapsedMilliseconds} ms - Total Time: {s2.ElapsedMilliseconds} ms");
                            s1.Stop();
                            s2.Stop();
                        }
                    }
                    break;
                }
            });
        }

        private void WriteListBox(string text)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                addressList.Items.Add(text);
            }));
        }
    }
}

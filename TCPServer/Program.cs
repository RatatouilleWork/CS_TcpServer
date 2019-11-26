using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create socket:
            Socket tcp_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind IP Address;
            IPAddress server_ip = new IPAddress(new Byte[] { 127,0,0,1 });
            EndPoint end_point = new IPEndPoint(server_ip, 10086);
            tcp_server.Bind(end_point);

            // Set Listen;
            tcp_server.Listen(100);
            Console.WriteLine("Start Listening");

            // Accept connection:
            while(true)
            {
                Socket sock_peer = tcp_server.Accept();

                if(null == sock_peer)
                {
                    Console.WriteLine("accept failed...");
                }
                else
                {
                    ThreadCallBack t = new ThreadCallBack(sock_peer);
                    Thread serve_thread = new Thread(t.run);
                    serve_thread.Start();
                }

            }
        }
    }

    public class ThreadCallBack
    {
        private Socket sock_peer;
        public ThreadCallBack(Socket sock_peer)
        {
            this.sock_peer = sock_peer;
        }

        public void run()
        {
            while(true)
            {
                // Peer Info:
                IPEndPoint peer_endpoint = (IPEndPoint)sock_peer.RemoteEndPoint;
                Console.WriteLine("Peer " + peer_endpoint.Address.ToString() + "@" + peer_endpoint.Port.ToString() + " Connected:");

                if(!sock_peer.Connected)
                {
                    Console.WriteLine("Peer disconnected, Aufwiedersehen~");
                    break;
                }

                // Receive from peer:
                Byte[] buffer = new byte[1024];
                try
                {
                    int len = sock_peer.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, len);
                    Console.WriteLine(peer_endpoint.Address.ToString() + "@" + peer_endpoint.Port.ToString() + ":" + message);
                    // Parse received stream:
                    List<string> message_list = new List<string>();
                    foreach (string i in Regex.Split(message, ";", RegexOptions.IgnoreCase))
                    {
                        message_list.Add(i);
                    }

                    if(message_list.ToArray().Length != 4)
                    {
                        Console.WriteLine("Data Error, please check your data then input again!");
                        Byte[] error_stream = new Byte[5];
                        error_stream = Encoding.UTF8.GetBytes("x");
                        sock_peer.Send(error_stream);
                        /*
                        if(!sock_peer.Connected)
                        {
                            Console.WriteLine("Peer disconnected, Aufwiedersehen~");
                            break;
                        }
                        */
                        continue;
                    }
                    else
                    {
                        // Calculate:
                        double d1 = Convert.ToDouble(message_list[0]);
                        double d2 = Convert.ToDouble(message_list[1]);
                        double l1 = Convert.ToDouble(message_list[2]);
                        double l2 = Convert.ToDouble(message_list[3]);

                        Byte[] send_buffer = new byte[100];
                        double theta = 0;
                        if(Math.Abs(d1-d2) < 0.001)
                        {
                            theta = 0;
                            send_buffer = Encoding.UTF8.GetBytes(Convert.ToString(theta));
                            sock_peer.Send(send_buffer);
                        }
                        else
                        {
                            theta = Math.Atan((d1-d2)/(l1 + l2));
                            Console.WriteLine("theta is: " + theta);

                        }


                        // Send result:
                        send_buffer = Encoding.UTF8.GetBytes(Convert.ToString(theta));
                        sock_peer.Send(send_buffer);

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }


            }
        }
    }
}

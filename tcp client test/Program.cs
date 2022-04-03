using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace tcp_client_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            while (true)
            {
                Console.WriteLine("name: diego");
                string a = Console.ReadLine();

                Console.WriteLine("pass: 123654");
                string b = Console.ReadLine();

                checktest(a, b);

                Thread.Sleep(1000);
                Console.WriteLine("Next: \n");

            }



            Console.ReadKey();
        }


        public static void checktest(string name, string passwort)
        {

            int port = 7755;
            IPAddress address = IPAddress.Loopback;
            IPEndPoint remoteEP = new IPEndPoint(address, port);

            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(remoteEP);
                using (NetworkStream ns = tcpClient.GetStream())
                {
                    using (StreamWriter writer = new StreamWriter(ns, leaveOpen: true))
                    {
                        writer.Write(name.Length);
                        writer.WriteLine(name);
                        writer.WriteLine(passwort);
                      


                    }
                    tcpClient.Client.Shutdown(SocketShutdown.Send);
                }
            }
        }
    }



}

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
          

                checktest();


            



            Console.ReadKey();
        }


        public static void checktest()
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

                        string[] cars = { "register", "albertoderg", "123465"};
                        writer.Write(cars);
                
                      


                    }
                    tcpClient.Client.Shutdown(SocketShutdown.Send);
                }
            }
        }
    }



}









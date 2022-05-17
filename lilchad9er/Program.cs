using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace pgdemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("172.16.37.107");
            TcpListener listener = new TcpListener(ip, 7755);
            listener.Start();
            Console.WriteLine("Server started...\n");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                _ = Task.Factory.StartNew(async (object outerc) =>
                {
                    TcpClient innerclient = (TcpClient)outerc;
                    NetworkStream ns = innerclient.GetStream();
                    byte[] buffer = new byte[512];
                    int res = await ns.ReadAsync(buffer, 0, buffer.Length);
                    string USERNAME = "empt usrn";
                    string PASSPHRASE = "empt pw";
                    Console.WriteLine("buffer: ");
                    
                    /* FÜR DEBUG PURPOSES:
                    foreach (byte b in buffer)
                    {
                        Console.WriteLine(b.ToString());
                    }*/

                    string handleddata = Encoding.UTF8.GetString(buffer, 0, res);
                    
                    var splitdata = handleddata.Split(';');
                    USERNAME = splitdata[0];
                    PASSPHRASE = splitdata[1];
                    Console.WriteLine("Username: " + USERNAME + " - " + "Password: " + PASSPHRASE);

                    using (BinaryWriter writer = new BinaryWriter(ns))
                    {
                        string msgtosend = "Hello from the server! +*-123"; // ------!!!! HIER ANGEBEN WAS GESENDET WERDEN SOLL
                        writer.Write(Encoding.UTF8.GetBytes(msgtosend));
                        Console.WriteLine("sent");
                    }

                }, client);

            }

        }

            public static void registrieren(string name, string passwort, double gewicht, double groesse, string pet_name)
            {
                using (NpgsqlConnection con = GetConnection())
                {
                    


                var cmd = new NpgsqlCommand("INSERT INTO users (name,passwort,gewicht,groesse,pet_name) VALUES (@p1, @p2,@p3, @p4,@p5)", con)
                {
                    Parameters =
                    {
                        new("p1", name),
                        new("p2", passwort),
                        new("p3", gewicht),
                        new("p4", groesse),
                        new("p5", pet_name)
              


                    }
                };


                con.Open();
                int n = cmd.ExecuteNonQuery();
                // int n2 = cmd2.ExecuteNonQuery();


                if (n == 1)
                {
                    Console.WriteLine("Insert ist erfolgreich :)");
                }

            }
        }



        private static void checklogin(string name, string passwort)
        {
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE passwort = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", passwort),
                    }
                };

                con.Open();
                int n = cmd.ExecuteNonQuery();


                object obj = cmd.ExecuteScalar();
                if (Convert.ToInt32(obj) > 0)
                {
                    Console.WriteLine("hab passwort gefunden gefunden");
                }
                else
                {
                    Console.WriteLine("hab  passwd nicht gefunden");
                }



                if (n == 1)
                {
                    Console.WriteLine("Insert ist erfolgreich :)");
                }

            }

            using (NpgsqlConnection con = GetConnection())
            {

                var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE name = @p1  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p1", name),

                    }
                };

                con.Open();
                int n = cmd2.ExecuteNonQuery();

                object obj = cmd2.ExecuteScalar();
                if (Convert.ToInt32(obj) > 0)
                {
                    Console.WriteLine("hab name gefunden gefunden");
                }
                else
                {
                    Console.WriteLine("hab  name nicht gefunden");
                }



                if (n == 1)
                {
                    Console.WriteLine("Insert ist erfolgreich :)");
                }

            }
        }

        private static void TestConnection()
        {
            using (NpgsqlConnection con = GetConnection())
            {
                con.Open();
                if (con.State == System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connected bro ");
                }
                else
                {
                    Console.WriteLine("not connected");
                }
            }
        }


        private static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(@"Server=dumbo.db.elephantsql.com;Port=5432;User Id=lgdwxufa;Password=Fh-WAkVVIAztSzrgTb_CfKG6lVbWjC3o;Database=lgdwxufa;");
        }
    }
}

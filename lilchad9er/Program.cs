using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace pgdemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("172.16.36.242");
            TcpListener listener = new TcpListener(ip, 7755);
            listener.Start();
            Console.WriteLine("Server started...\n");

            while (true)
            {
                using (TcpClient client = listener.AcceptTcpClient())
                {
                    using (NetworkStream ns = client.GetStream())
                    {
                        using (BinaryReader reader = new BinaryReader(ns,Encoding.UTF8, leaveOpen: true))
                        {
                            int len = reader.Read7BitEncodedInt(); // liest die länge des aufkommenden bytearrays aus
                            var firstread = reader.ReadBytes(len); // liest bytearray aus 
                            string USERNAME = Encoding.UTF8.GetString(firstread); //Konvertiert bytearray zu string


                            int secondlen = reader.Read7BitEncodedInt();  // ,,___,,____,, repeat
                            var secondread = reader.ReadBytes(secondlen);
                            string PASSPHRASE = Encoding.UTF8.GetString(secondread);



                            Console.WriteLine("Username: " + USERNAME + " - " + "Password: " + PASSPHRASE) ;

                        }

                        ns.Flush();

                        using (BinaryWriter writer = new BinaryWriter(ns))
                        {
                            string msgtosend = ""; // ------!!!! HIER ANGEBEN WAS GESENDET WERDEN SOLL
                            writer.Write(Encoding.UTF8.GetBytes(msgtosend));
                            Console.WriteLine("sent");
                        }
                    }
                }

            }


        }


        // use UTF8 in this sample:
        private static void registrieren(string name, string passwort, double gewicht, double groesse, string pet_name)
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

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


                    if (registrieren(USERNAME, PASSPHRASE, 50, 6.2, "susi"))//groesee gewicht usw. sind testdaten. dass alles muss man von app holen.
                    {
                        var id = getidname(USERNAME);

                        //app sagen es geht und dann id zurück schicken.
                    }
                    else
                    {
                        //fehler : app sagen es geht net
                    }


                    trainingadd(new DateTime(2021, 4, 5), "lifting", 40, 5, 10,//app schickt id );


                    //wenn user sich einloggt dann

                    if (checklogin(USERNAME, PASSPHRASE))
                    {
                        //User wurde gefunden. App sagen es passt.
                    }
                    else
                    {
                        //fehler : app sagen user gibt es nicht
                    }


                }, client);

            }

        }

        private static int getidname(string name)
        {

            using (NpgsqlConnection con = GetConnection())
            {

                var cmd = new NpgsqlCommand("SELECT user_id FROM users WHERE name = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", name)
                    }
                };

                con.Open();
                int n = cmd.ExecuteNonQuery();
                Console.WriteLine(cmd.Parameters[0].Value);

                NpgsqlDataReader dr = cmd.ExecuteReader();

                dr.Read();

                var result = dr.GetInt32(0);

                return result;
            }
        }

        private static void trainingadd(DateTime datum, string uebungsname, int gewicht, int sets, int reps, int userid)
        {
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd = new NpgsqlCommand("INSERT INTO training (datum,uebungsname,gewicht,sets,reps,user_id) VALUES (@p1, @p2,@p3, @p4,@p5,@p6)", con)
                {
                    Parameters =
                    {
                        new("p1", datum),
                        new("p2", uebungsname),
                        new("p3", gewicht),
                        new("p4", sets),
                        new("p5", reps),
                        new("p6", userid)
                    }
                };
                con.Open();
                int n = cmd.ExecuteNonQuery();
                if (n == 1)
                {
                    Console.WriteLine("Insert ist erfolgreich :)");
                }
            }
        }

        public static bool registrieren(string name, string passwort, double gewicht, double groesse, string pet_name)
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
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }



        private static bool checklogin(string name, string passwort)
        {
            int verifer = 0;
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE name = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", name),
                    }
                };

                con.Open();
                int n1 = cmd1.ExecuteNonQuery();


                object obj1 = cmd1.ExecuteScalar();
                if (Convert.ToInt32(obj1) > 0)
                {
                    Console.WriteLine("´Name wurde gefunden!!");
                    verifer = verifer + 1;
                }
                else
                {
                    Console.WriteLine("Name wurde nicht gefunden!");

                }

                if (n1 == 1)
                {
                    Console.WriteLine("Check Name Befehl erfolgreich ausgeführt");

                }




                var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE passwort = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", passwort),
                    }
                };


                int n = cmd.ExecuteNonQuery();



                object obj = cmd.ExecuteScalar();
                if (Convert.ToInt32(obj) > 0)
                {
                    Console.WriteLine("Passwort wurde gefunden!!");
                    verifer = verifer + 1;
                }
                else
                {
                    Console.WriteLine("Passwort wurde nicht gefunden!");

                }

                if (n == 1)
                {
                    Console.WriteLine("Check password Befehl erfolgreich ausgeführt");
                }


                if (verifer == 2)
                {
                    return true;
                }

                else
                {
                    return false;
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

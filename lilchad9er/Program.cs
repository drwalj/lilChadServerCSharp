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
            IPAddress ip = IPAddress.Parse("192.168.0.87");
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
                    string[] recievedMessage = await RecieveFromAppAsync(ns);


                    if (recievedMessage[0] == "register")
                    {
                        string replay;
                        if (registrieren(recievedMessage[1], recievedMessage[2]))
                        {
                            int userid = getidname(recievedMessage[1]);

                            string useridstr = Convert.ToString(userid);

                            /* drwal sende*/
                            replay = $"true;{useridstr}";
                            SendToApp(replay, ns);
                        }
                        else
                        {
                            SendToApp("false;0", ns);
                        }
                    }

                    if (recievedMessage[0] == "login")
                    {


                        if (checklogin(recievedMessage[1], recievedMessage[2]))
                        {
                            string replay = login(recievedMessage[1]);


                            SendToApp(replay, ns);

                        }
                        else
                        {
                            SendToApp("false;0", ns);
                        }


                    }



                    if (recievedMessage[0] == "saveuser")
                    {
                        //Hamed dings neue methode erstellen. dass die daten von recievemessage[diese kack numemrn] in datenbank speichert


                        int userid = Convert.ToInt32((string)recievedMessage[1]);
                        int groesse = Convert.ToInt32((string)recievedMessage[4]);
                        int alter = Convert.ToInt32((string)recievedMessage[5]);

                        var vorherigegewichte = getpreviousweight(recievedMessage[2]);


                        var gewichtzuspeichern = vorherigegewichte + "," + recievedMessage[3];

                        if (saveusermeth(userid, recievedMessage[2], gewichtzuspeichern, groesse, alter))
                        {
                            SendToApp($"true", ns);
                        }
                        else
                        {
                            SendToApp("false;0", ns);
                        }
                    }

                    if (recievedMessage[0] == "setpetname")
                    {
                        int userid = Convert.ToInt32((string)recievedMessage[1]);

                        if (savepetname(userid, recievedMessage[2]))
                        {
                            SendToApp($"true", ns);
                        }

                        else
                        {
                            SendToApp("false;0", ns);
                        }
                    }

                    if (recievedMessage[0] == "lvl")
                    {

                        int userid = Convert.ToInt32((string)recievedMessage[1]);
                        int levelup = Convert.ToInt32((string)recievedMessage[2]);
                        var currentpetlvl = getcrurentpetlvl(userid);

                        //ändere savepetname lol


                        if (savepetlvl(userid, levelup, currentpetlvl))
                        {
                            SendToApp($"true", ns);
                        }

                        else
                        {
                            SendToApp("false;0", ns);
                        }

                        Console.WriteLine("Recieved request: " + recievedMessage);
                    }


                    if (recievedMessage[0] == "upxp")
                    {

                        int userid = Convert.ToInt32((string)recievedMessage[1]);
                        int xpup = Convert.ToInt32((string)recievedMessage[2]);
                        var currentpetxp = getcurrentxp(userid);

                        //ändere savepetname lol

                        Console.WriteLine($"\nRecieved request: \n{recievedMessage}");
                        if (savepetxp(userid, xpup, currentpetxp))
                        {
                            SendToApp($"true", ns);
                        }

                        else
                        {
                            SendToApp("false;0", ns);
                        }
                    }



                }, client);

            }

        }

        private static bool SendToApp(string messageToSend, NetworkStream ns)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(ns))
                {
                    writer.Write(Encoding.UTF8.GetBytes(messageToSend));
                    Console.WriteLine("SentToServer: " + messageToSend);
                }
                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private async static Task<string[]> RecieveFromAppAsync(NetworkStream ns) //gibt schon gesplittetes array zurück
        {
            try
            {
                byte[] buffer = new byte[512];
                int res = await ns.ReadAsync(buffer, 0, buffer.Length);
                string handleddata = Encoding.UTF8.GetString(buffer, 0, res);
                Console.WriteLine("Recieved Request:\n" + handleddata + "\n");
                var recArray = handleddata.Split(';');
                return recArray;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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


        private static string getpreviousweight(string name)
        {
            string finaloutput = "f";
            using (NpgsqlConnection con = GetConnection())
            {

                con.Open();
                var cmd1 = new NpgsqlCommand("SELECT * FROM users WHERE name = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", name),
                    }
                };


                var reader = cmd1.ExecuteReader();

                while (reader.Read())
                {
                    string output = string.Format("{0}", reader.GetValue(3));

                    string newoutput = output;

                    finaloutput = newoutput;

                }

                return finaloutput;

            }
        }

        private static int getcrurentpetlvl(int userid)
        {
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd = new NpgsqlCommand("SELECT pet_level FROM users WHERE user_id = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", userid)
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

        private static int getcurrentxp(int userid)
        {
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd = new NpgsqlCommand("SELECT pet_xp FROM users WHERE user_id = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", userid)
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

        private static string login(string name)
        {
            string finaloutput = "f";
            using (NpgsqlConnection con = GetConnection())
            {

                con.Open();
                var cmd1 = new NpgsqlCommand("SELECT * FROM users WHERE name = @p2  LIMIT 1;", con)
                {
                    Parameters =
                    {
                        new("p2", name),
                    }
                };


                var reader = cmd1.ExecuteReader();

                while (reader.Read())
                {
                    string output = string.Format("{0} {1} {2} {3} {4} {5} {6} {7}", reader.GetValue(0), reader.GetValue(5), reader.GetValue(6), reader.GetValue(7), reader.GetValue(1), reader.GetValue(4), reader.GetValue(3), reader.GetValue(8));

                    string newoutput = output.Replace(' ', ';');


                    finaloutput = "alldata;" + newoutput;

                }

                return finaloutput;

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




        private static bool savepetlvl(int userid, int lvlup, int currentlvl)
        {

            int geupdatetlvl = currentlvl + lvlup;

            using (NpgsqlConnection con = GetConnection())
            {

                var cmd4 = new NpgsqlCommand("UPDATE users SET pet_level = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", geupdatetlvl),

                    }
                };
                con.Open();
                int n4 = cmd4.ExecuteNonQuery();
                if (n4 == 1)
                {

                    Console.WriteLine("Insert ist erfolgreich :)");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        private static bool savepetxp(int userid, int xpup, int currentxp)
        {

            int geupdatetxp = currentxp + xpup;

            using (NpgsqlConnection con = GetConnection())
            {

                var cmd4 = new NpgsqlCommand("UPDATE users SET pet_xp = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", geupdatetxp),

                    }
                };
                con.Open();
                int n4 = cmd4.ExecuteNonQuery();
                if (n4 == 1)
                {

                    Console.WriteLine("Insert ist erfolgreich :)");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        private static bool savepetname(int userid, string petname)
        {
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd4 = new NpgsqlCommand("UPDATE users SET pet_name = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", petname),

                    }
                };
                con.Open();
                int n4 = cmd4.ExecuteNonQuery();
                if (n4 == 1)
                {

                    Console.WriteLine("Insert ist erfolgreich :)");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        private static bool saveusermeth(int userid, string username, string weight, int size, int age)
        {
            int verifirer = 0;
            using (NpgsqlConnection con = GetConnection())
            {

                var cmd4 = new NpgsqlCommand("UPDATE users SET pet_name = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", username),

                    }
                };
                con.Open();
                int n4 = cmd4.ExecuteNonQuery();
                if (n4 == 1)
                {
                    verifirer += 1;
                    Console.WriteLine("Insert ist erfolgreich :)");
                }

                var cmd = new NpgsqlCommand("UPDATE users SET gewicht = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", weight),

                    }
                };

                int n = cmd.ExecuteNonQuery();
                if (n == 1)
                {
                    verifirer += 1;
                    Console.WriteLine("Insert ist erfolgreich :)");
                }




                var cmd1 = new NpgsqlCommand("UPDATE users SET groesse = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", size),

                    }
                };

                int n1 = cmd1.ExecuteNonQuery();
                if (n1 == 1)
                {
                    verifirer += 1;
                    Console.WriteLine("Insert ist erfolgreich :)");
                }

                var cmd3 = new NpgsqlCommand("UPDATE users SET age = @p2 WHERE user_id = @p1", con)
                {
                    Parameters =
                    {
                        new("p1", userid),
                        new("p2", age),

                    }
                };

                int n3 = cmd3.ExecuteNonQuery();
                if (n3 == 1)
                {
                    verifirer += 1;
                    Console.WriteLine("Insert ist erfolgreich :)");
                }


                if (verifirer == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool registrieren(string name, string passwort)
        {
            using (NpgsqlConnection con = GetConnection())
            {



                var cmd = new NpgsqlCommand("INSERT INTO users (name,passwort) VALUES (@p1, @p2)", con)
                {
                    Parameters =
                    {
                        new("p1", name),
                        new("p2", passwort),

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
                    Console.WriteLine("Name wurde gefunden!!");
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

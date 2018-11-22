using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TcpMessenger
{
    class Program
    {
        static void Main(string[] args)
        {   
            TcpMessengerDB dbcontext = new TcpMessengerDB();
            
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1024);
            listener.Start(10);
            listener.BeginAcceptTcpClient(MyCallback, listener);
            Console.WriteLine(new string('-',60));
            Console.WriteLine("\t\t\tServer started");
            Console.WriteLine(new string('-', 60));
            Console.ReadKey();
        }

        private static void MyCallback(IAsyncResult ar)
        {
            string clientIpAddress = "";
            try
            {
                TcpListener listener = ar.AsyncState as TcpListener;
                TcpClient client = listener.EndAcceptTcpClient(ar);
                listener.BeginAcceptTcpClient(MyCallback, listener);

                StreamReader reader = new StreamReader(client.GetStream(), Encoding.GetEncoding(866));
                StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.GetEncoding(866));
                IPAddress remoteIpAddress = (client.Client.RemoteEndPoint as IPEndPoint).Address;
                clientIpAddress = remoteIpAddress.ToString();

                Console.WriteLine($"User connect to server from IP: {clientIpAddress}");

                writer.WriteLine(new string('-', 60));
                writer.WriteLine("\t\t\tWelcome to Tcp Messenger!");
                writer.WriteLine(new string('-', 60));

                bool isLogin = false;
                string currentName = null;
                var controller = new TcpMessengerDataBaseController();

                while (true)
                {
                   LabelA:
                    if(isLogin)
                        writer.Write($"<{currentName}> Enter command or HELP => ");
                    else
                        writer.Write("Enter command or HELP => ");
                    writer.Flush();
                    string command = reader.ReadLine().Trim().ToUpper();
                    switch (command)
                    {
                        case "HELP":
                            writer.WriteLine("CHECK IN - регистрация нового пользователя(Имя, Логин, Пароль)");
                            writer.WriteLine("LOGIN - вход под своим логином и паролем");
                            writer.WriteLine("LOG OUT - выход с текущего пользователя");
                            writer.WriteLine("LIST USERS - получить список Имен зарегистрированых пользователей");
                            writer.WriteLine("SEND MESSAGE - отправить сообщение(указать само сообщение и Имя получателя)");
                            writer.WriteLine("MESSAGES - перейти в меню просмотра сообщений");
                            writer.WriteLine("QUIT - выход");
                            break;

                        case "CHECK IN":
                            if(isLogin) break;
                            writer.Write("Enter your name =>");
                            writer.Flush();
                            string registrationName = reader.ReadLine();
                            writer.Write("Enter login =>");
                            writer.Flush();
                            string registrationLogin = reader.ReadLine();
                            writer.Write("Enter password =>");
                            writer.Flush();
                            string registrationPassword = reader.ReadLine();
                            
                            var list = controller.GetUsers();
                            if (list.Count(x => x.Name == registrationName) > 0)
                            {
                                writer.WriteLine("This name is already taken!");
                            }
                            else if (list.Count(x => x.Login == registrationLogin) > 0)
                            {
                                writer.WriteLine("This login is already taken!");
                            }
                            else
                            {
                                controller.AddUser(registrationName, registrationLogin, registrationPassword);
                                writer.WriteLine("User added! Now you can Login into Tcp Messenger");
                            }
                            writer.Flush();
                            break;

                        case "LIST USERS":

                            if (controller.GetUsers().Count == 0)
                            {
                               writer.WriteLine("Data base of users - EMPTY");
                                goto LabelA;
                            }

                            writer.WriteLine("Users:\n");
                            foreach (var user in controller.GetUsers())
                            {
                                writer.WriteLine($"{user.Id} {user.Name}\t {user.Login}\t\t {user.Pwd}");
                            }

                            writer.WriteLine();
                            writer.Flush();
                            break;



                        case "SEND MESSAGE":
                            if(!isLogin)break;
                            writer.Write("Enter name Recipient =>");
                            writer.Flush();
                            string nameRecipient = reader.ReadLine();
                            if (!controller.GetUsers().Exists(x => x.Name == nameRecipient))
                            {
                                writer.Write("Incorrect name Recipient");
                                goto case "SEND MESSAGE";
                            }
                            writer.Write("Enter message =>");
                            writer.Flush();
                            string message = reader.ReadLine();
                            writer.Flush();

                            var dateIn = DateTime.Now.ToString("F");
                            controller.AddMessage(message,dateIn,
                                controller.GetUsers().First(x => x.Name==currentName).Id,
                                controller.GetUsers().First(x=>x.Name==nameRecipient).Id);
                            writer.WriteLine("Message was sent!");
                            break;

                        case "MESSAGES":
                            if(!isLogin) break;
                            var messages = controller.GetMessages();
                            var users = controller.GetUsers();
                            writer.WriteLine();
                            while (true)
                            {
                                writer.WriteLine("Commands: UNREAD - Show new messages; ALL - show all messages; BACK - to main menu");
                                writer.Write("Enter command => ");
                                writer.Flush();
                                command = reader.ReadLine().Trim().ToUpper();
                                switch (command)
                                {
                                    case "UNREAD":
                                        foreach (var mess in messages)
                                        {
                                            if (mess.RcptId == users.Find(x => x.Name == currentName).Id &&
                                                mess.DateOut == "--:--:--")
                                            {
                                                writer.WriteLine(
                                                    $"From <{users.Find(x => x.Id == mess.SenderId).Name}> at {mess.DateIn}: {mess.Messg}");
                                                controller.UpdateMessage(mess.Id, DateTime.Now.ToString("F"));
                                            }
                                        }
                                        writer.WriteLine();
                                        writer.Flush();
                                        break;
                                    case "ALL":
                                        foreach (var mess in messages)
                                        {
                                            if (mess.RcptId == users.Find(x => x.Name == currentName).Id )
                                            {
                                                writer.WriteLine(
                                                    $"From <{users.Find(x => x.Id == mess.SenderId).Name}> at {mess.DateIn}: {mess.Messg}");
                                                writer.WriteLine();
                                            }
                                        }
                                        writer.WriteLine();
                                        writer.Flush();
                                        break;
                                    case "BACK":
                                        goto MESS;
                                }
                            }

                            MESS:;
                            break;

                        case "LOGIN":
                            writer.Write("Enter login =>");
                            writer.Flush();
                            string login = reader.ReadLine();
                            writer.Write("Enter password =>");
                            writer.Flush();
                            string passwd = reader.ReadLine();
                            if (isLogin = CheckUser(login, passwd, out string name))
                            {
                                writer.WriteLine();
                                writer.WriteLine($"Login successed! Hello {name}");
                                currentName = name;
                            }
                            else
                            {
                                writer.WriteLine("Login failed");
                            }
                            writer.Flush();
                            break;

                        case "LOG OUT":
                            if (isLogin)
                            {
                                writer.WriteLine($"\nYou are log out");
                                isLogin = false;
                                currentName = null;
                            }
                            else
                            {
                                writer.WriteLine("Login failed");
                            }
                            writer.Flush();
                            break;

                        case "QUIT":
                            writer.WriteLine("Good buy!!!");
                            writer.Flush();
                            goto END;

                    }
                }

                END:;
                Console.WriteLine($"Disonnect from IP: {remoteIpAddress}");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disonnect from IP: {clientIpAddress}");
            }
        }

        private static bool CheckUser(string login, string passwd, out string name)
        {
            var controller = new TcpMessengerDataBaseController();
            var list = controller.GetUsers();
            var users = list.Where( x => x.Login == login && x.Pwd==passwd );
            if (users.Count()!=0)
            {
                name = users.First().Name;
                return true;
            }

            name = null;
            return false;
        }
    }
}

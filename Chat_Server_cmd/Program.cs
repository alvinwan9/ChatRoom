using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace Chat_Server_cmd
{
    internal class Program
    {
        public static List<User> userlist = new List<User>();
        //userlist中数据的形式
        private static TcpListener tl; //声明监听对象
        private static NetworkStream ns; //声明网络流
        private static string localAddress = GetAddressIP();

        private static void Main(string[] args)
        {
            try
            {
                //建立TCP监听器TcpListener对象
                tl = new TcpListener(9999);
                //启动监听器
                tl.Start();
                Console.WriteLine("服务器启动成功...");
                //开启线程
                Thread th = new Thread(new ThreadStart(ListenClientConnect));
                th.IsBackground = true;
                th.Start();
                while (true)
                {
                    string index = Console.ReadLine();
                    if (index == "exit")
                    {
                        Console.WriteLine("开始停止服务，并依次使用户退出!");
                        foreach (User user in userlist)
                        {
                            user.br.Close();
                            user.bw.Close();
                            user.client.Close();
                        }
                        tl.Stop();
                        break;
                    }
                    else
                    {
                        SendToClient(userlist, "barchmsg#服务器#" + index);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("服务器启动失败..." + e.Message);
                throw;
            }
        }

        private static void ListenClientConnect()
        {
            Console.WriteLine("开始在{0}:{1}监听客户连接", localAddress, 9999);
            while (true)
            {
                TcpClient newClient = null;
                try
                {
                    //等待用户进入
                    newClient = tl.AcceptTcpClient();
                }
                catch
                {
                    //当单击“停止监听”或者退出此窗体时AcceptTcpClient()会产生异常
                    //因此可以利用此异常退出循环
                    break;
                }
                //每接受一个客户端连接,就创建一个对应的线程循环接收该客户端发来的信息
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ReceiveData);
                Thread threadReceive = new Thread(pts);
                User user = new User(newClient);
                threadReceive.Start(user);
                // userlist.Add(user);
                //AddComboBoxitem(user);

            }
        }

        /// <summary>
        /// 接收、处理客户端信息，每客户1个线程
        /// </summary>
        /// <param name="obj">用于区分是哪个客户</param>
        private static void ReceiveData(object obj)
        {
            User user = (User)obj;
            TcpClient client = user.client;
            //是否正常退出接收线程
            bool normalExit = false;
            //用于控制是否退出循环
            bool exitWhile = false;
            while (exitWhile == false)
            {
                string receiveString = null;
                try
                {
                    //从网络流中读出字符串
                    //此方法会自动判断字符串长度前缀，并根据长度前缀读出字符串
                    receiveString = user.br.ReadString();
                }
                catch
                {
                    //底层套接字不存在时会出现异常
                    Console.WriteLine("接收数据失败");
                }
                if (receiveString == null)
                {
                    if (normalExit == false)
                    {
                        //如果停止了监听，Connected为false
                        if (client.Connected == true)
                        {
                            Console.WriteLine("与{0}]失去联系，已终止接收该用户信息", client.Client.RemoteEndPoint);
                        }
                    }
                    break;
                }
                Console.WriteLine("来自[{0}]：{1}", user.client.Client.RemoteEndPoint, receiveString);
                string[] splitString = receiveString.Split('#');
                string sendString = "";
                switch (splitString[0])
                {
                    case "login":
                        //格式：login#jack#12345
                        user.Username = splitString[1];
                        user.Password = splitString[2];
                        if (LoginCheck(user))
                        {
                            bool flag = false;
                            foreach (User u in userlist)
                            {
                                if (u.Username.Equals(user.Username))
                                    flag = true;
                            }
                            if (!flag)
                            {
                                user.IsLogin = true;
                                userlist.Add(user);
                                Console.WriteLine("{0}({1})登录成功\t当前连接用户数{2}", user.Username, user.client.Client.RemoteEndPoint, userlist.Count);
                                sendString = "users#" + GetUsers();
                                SendToClient(userlist, sendString);//发给所有用户
                                break;
                            }
                            sendString = "already login";
                        }
                        else
                        {
                            sendString = "login fail";
                        }
                        SendToClient(user, sendString);//登陆消息反馈，只发给自己
                        break;
                    case "signup":
                        //格式:signup#jack#12345
                        string temp = SignUp(splitString[1], splitString[2]);
                        SendToClient(user, temp);
                        //返回格式:state
                        break;
                    case "logout":
                        //格式：logout#jack
                        Console.WriteLine("[{0}{1}]退出", user.Username, user.client.Client.RemoteEndPoint);
                        normalExit = true;
                        exitWhile = true;
                        user.IsLogin = false;
                        break;
                    case "barchmsg":
                        //格式：barchmsg#msg
                        Console.WriteLine("[{0}({1})]说：{2}", user.Username, client.Client.RemoteEndPoint, receiveString.Substring(splitString[0].Length + 1));
                        SendToClient(userlist.Where(u => !u.Username.Equals(user.Username)), receiveString);
                        break;
                    case "getusers":
                        SendToClient(user, "users#" + GetUsers());
                        break;
                    default:
                        Console.WriteLine("[{0}({1})]指令有误：{2}", user.Username, client.Client.RemoteEndPoint,
                            receiveString);
                        break;
                }
            }
            userlist.Remove(user);
            SendToClient(userlist, "users#" + GetUsers());//发给其他用户
            client.Close();
            Console.WriteLine("当前连接用户数：{0}", userlist.Count);
        }


        /// <summary>
        /// 发给单个客户端
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="str">发送的消息</param>
        private static void SendToClient(User user, string str)
        {
            try
            {
                //将字符串写入网络流，此方法会自动附加字符串长度前缀
                user.bw.Write(str);
                user.bw.Flush();
                //Console.WriteLine("向[{0}({1})]发送：{2}", user.Username,user.client.Client.RemoteEndPoint, str);
            }
            catch
            {
                Console.WriteLine("向[{0}({1})]发送信息失败", user.Username, user.client.Client.RemoteEndPoint);
            }
        }

        /// <summary>
        /// 重载方法 发给所有客户端(linq语句)
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <param name="str">要发送的消息</param>
        private static void SendToClient(IEnumerable<User> users, string str)
        {
            foreach (var user in users)
            {
                try
                {
                    //将字符串写入网络流，此方法会自动附加字符串长度前缀
                    user.bw.Write(str);
                    user.bw.Flush();
                }
                catch
                {
                    Console.WriteLine("向[{0}({1})]发送信息失败", user.Username, user.client.Client.RemoteEndPoint);
                }
            }
        }

        /// <summary>
        /// 重载方法 发给所有客户端
        /// </summary>
        /// <param name="users">用户列表</param>
        /// <param name="str">要发送的消息</param>
        private static void SendToClient(List<User> users, string str)
        {
            foreach (var user in users)
            {
                try
                {
                    //将字符串写入网络流，此方法会自动附加字符串长度前缀
                    user.bw.Write(str);
                    user.bw.Flush();
                }
                catch
                {
                    Console.WriteLine("向[{0}({1})]发送信息失败", user.Username, user.client.Client.RemoteEndPoint);
                }
            }
        }
        #region static string SignUp(string username, string password) 注册方法
        static string SignUp(string username, string password)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                string filepath = "info.xml";
                if (!File.Exists(filepath))
                {
                    CreateXML(doc, filepath); 
                }
                doc.Load(filepath);
                XmlNodeList userinfos = doc.GetElementsByTagName("info");
                foreach (XmlNode userinfo in userinfos)
                {
                    if (userinfo.SelectSingleNode("name").InnerText == username)
                    {
                        return "accounted";
                    }
                }
                XmlElement root = doc.DocumentElement;
                XmlNode newUser = doc.CreateElement("info");

                XmlNode nameNode = doc.CreateElement("name");
                nameNode.InnerText = username;
                newUser.AppendChild(nameNode);

                XmlNode pwdNode = doc.CreateElement("password");
                pwdNode.InnerText = password;
                newUser.AppendChild(pwdNode);

                root.AppendChild(newUser);

                doc.Save(filepath);
                return "success";
            }
            catch (Exception e)
            {
                return "error";
            }
        }
        #endregion
        #region static bool LoginCheck(User user) 登录验证
        static bool LoginCheck(User user)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                string filepath = "info.xml";
                if (!File.Exists(filepath))
                {
                    CreateXML(doc, filepath); 
                    return false;
                }
                doc.Load(filepath);
                XmlNodeList userinfos = doc.GetElementsByTagName("info");
                foreach (XmlNode userinfo in userinfos)
                {
                    string name = userinfo.SelectSingleNode("name").InnerText;
                    string pws = userinfo.SelectSingleNode("password").InnerText;
                    if (name.Equals(user.Username) &&
                        pws.Equals(user.Password))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion
        #region static void CreateXML(XmlDocument doc, string path)创建XML
        private static void CreateXML(XmlDocument doc, string path)
        {
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(dec);
            XmlNode rootnNode = doc.CreateElement("userinfo");
            doc.AppendChild(rootnNode);

            XmlNode newUserxNode = doc.CreateElement("info");
            rootnNode.AppendChild(newUserxNode);

            XmlNode name = doc.CreateElement("name");
            name.InnerText = "admin";
            newUserxNode.AppendChild(name);

            XmlNode pwd = doc.CreateElement("password");
            pwd.InnerText = "8888";
            newUserxNode.AppendChild(pwd);
            doc.Save(path);
        }

        #endregion
        #region static string GetUsers() 获取所有用户列表
        static string GetUsers()
        {
            StringBuilder sb = new StringBuilder();
            foreach (User u in userlist)
            {
                if (sb.Length > 0)
                    sb.Append("#");
                sb.Append(u.client.Client.RemoteEndPoint + ";" + u.Username);
            }
            return sb.ToString();
            //ex 192.168.0.1:12334;jack#192.168.0.3:12234;petter
        }
        #endregion
        #region static string GetAddressIP() 获取本地的IP地址
        static string GetAddressIP()
        {
            //获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
        #endregion
    }
}

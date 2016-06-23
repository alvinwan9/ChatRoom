using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_Client_V1
{
    public partial class Main : Form
    {
        public string Username { get; set; }
        public string Users { get; set; }
        public int Port { get; set; }
        public BinaryReader Br { get; set; }
        public BinaryWriter Bw { get; set; }

        public string ServerIP;
        public string ServerPort;
        public string IP { get; set; }
        public static List<Talking> TalkList = new List<Talking>();
        public List<User> UserList = new List<User>();
        bool iswork = false;
        UdpClient uc = null;
        Socket socketClient;
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Text = Username;
            LoadUser();
            StartListen();
        }
        //加载用户列表
        private void LoadUser()
        {
            if (string.IsNullOrEmpty(Users))
                return;
            this.lv_Userlist.Items.Clear();
            UserList.Clear();//users#192.168.0.1:12334;jack#192.168.0.3:12234;petter
            Users = Users.Replace("users#", "");//192.168.0.1:12334;jack#192.168.0.3:12234;petter
            string[] _userindex = Users.Split('#');

            foreach (string s in _userindex)
            {
                User user = new User();
                string[] _index = s.Split(';');
                //解析出IP和端口
                string[] ips = _index[0].Split(':');
                user.IP = ips[0];
                user.Port = int.Parse(ips[1]);
                //解析出用户名
                user.Username = _index[1];
                if (user.Username != this.Username)
                {
                    ListViewItem lvitem = new ListViewItem();

                    lvitem.ImageIndex = 0;
                    lvitem.Text = user.Username;
                    lvitem.Tag = _index[0];
                    this.lv_Userlist.Items.Add(lvitem);
                    UserList.Add(user);
                }
                else
                {
                    IP = user.IP;
                    this.Port = user.Port;
                }
                lb_num.Text = (UserList.Count+1).ToString();
            }
        }
        //开启监听服务
        private void StartListen()
        {
            iswork = true;
            //udp消息监听
            Thread th = new Thread(new ThreadStart(UdpListen));
            //设置为后台
            th.IsBackground = true;
            th.Start();
            //tcp消息监听
            Thread tcpth = new Thread(TcpListen);
            tcpth.IsBackground = true;
            tcpth.Start();
        }
        //监听客户端之间的通信
        private void UdpListen()
        {
            uc = new UdpClient(Port);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
            while (iswork)
            {
                //获得Form1发送过来的数据包
                string text = System.Text.Encoding.UTF8.GetString(uc.Receive(ref iep));
                string[] infos = text.Split('#');
                string command = infos[0];
                string fromuser = infos[1];
                string message = infos[2];
                switch (command)
                {
                    case "singlemsg":
                        Talking _tk = isHaveTalk(fromuser);
                        if (_tk != null)
                        {
                            this.BeginInvoke(new MethodInvoker(delegate()
                            {
                                _tk.Focus();
                                _tk.AddMessage(message, true);
                            }));
                        }
                        else
                        {
                            this.BeginInvoke(new MethodInvoker(delegate()
                            {
                                this.CreatTalking(text);
                            }));
                        }
                        break;
                }   
            }
        }
        //监听服务器端发来的消息
        private void TcpListen()
        {
            while (iswork)
            {
                string receiveMsg = null;
                try
                {
                    receiveMsg = Br.ReadString();
                }
                catch (Exception)
                {
                }
                if (receiveMsg != null)
                {
                    string command = string.Empty;
                    string[] splitStrings = receiveMsg.Split('#');
                    command = splitStrings[0];
                    switch (command)
                    {
                        case "barchmsg":
                            AddMessage(receiveMsg, true);
                            break;
                        case "users":
                            Users = receiveMsg.Replace("users#", "");
                            LoadUser();
                            break;
                    }
                }
            }
        }


        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                string sendmsg = "logout#" + Username;
                Bw.Write(sendmsg);
                Bw.Flush();
                iswork = false;
                Br.Close();
                Bw.Close();
            }
            catch
            { }
            Application.Exit();
        }

        private void lv_Userlist_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.lv_Userlist.SelectedItems.Count > 0)
            {
                ListViewItem lvitem = this.lv_Userlist.SelectedItems[0];
                string toname = lvitem.Text;
                string toips = lvitem.Tag.ToString();
                Talking t = isHaveTalk(toname);
                if (t != null)
                {
                    t.Focus();
                }
                else
                {
                    Talking talk = new Talking();
                    talk.UserName = Username;
                    talk.ToName = toname;
                    talk.ToIP = toips;
                    TalkList.Add(talk);
                    talk.Show();
                }
            }
        }

        private Talking isHaveTalk(string toname)
        {
            foreach (Talking tk in TalkList)
            {
                if (tk.ToName == toname)
                    return tk;
            }
            return null;
        }

        public static void RemoveTalking(Talking _talk)
        {
            foreach (Talking tk in TalkList)
            {
                if (tk.ToName == _talk.ToName)
                {
                    TalkList.Remove(_talk);
                    return;
                }
            }
        }

      
        //消息处理
        public void AddMessage(string text, bool isuser)
        {
            int startindex = this.rtb_ShowMsg.Text.Length;
            string[] infos = text.Split('#');
            //string command = infos[0];
            string fromuser = infos[1];
            string str = infos[2];
            string message = string.Empty;

            if (isuser)
                message = "【" + fromuser + "】  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + str + "\n";
            else
                message = "【" + this.Username + "】  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + str + "\n";
            this.rtb_ShowMsg.AppendText(message);
            this.rtb_ShowMsg.Select(startindex, message.Length);
            if (isuser)
            {
                this.rtb_ShowMsg.SelectionAlignment = HorizontalAlignment.Left;
            }
            else
            {
                this.rtb_ShowMsg.SelectionAlignment = HorizontalAlignment.Right;
            }
            this.rtb_ShowMsg.Select(this.rtb_ShowMsg.Text.Length, 0);
        }

        public void CreatTalking(object _text)
        {
            string[] infos = _text.ToString().Split('#');
            //string command = infos[0];
            string fromuser = infos[1];
            string message = infos[2];
            Talking talk = new Talking(message);
            talk.UserName = Username;
            talk.ToName = fromuser;
            talk.ToIP = GetIP(fromuser);
            TalkList.Add(talk);
            talk.Show();
        }

        private string GetIP(string toname)
        {
            foreach (User user in UserList)
            {
                if (user.Username == toname)
                    return user.IP + ':' + user.Port;
            }
            return "";
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {       //用户列表刷新（貌似没什么乱用）
            try
            {
                Bw.Write("getusers");
            }
            catch
            {
                MessageBox.Show("刷新失败！");
            }
        }
        //群发消息
        private void btn_Send_Click(object sender, EventArgs e)
        {
            try
            {//通过TCP协议向服务器发送群发消息
                string temp = this.tb_SendMsg.Text; //保存TextBox文本
                Bw.Write("barchmsg#" + Username + "#" + temp);
                Bw.Flush();
                AddMessage("barchmsg#" + Username + '#' + temp, false);
                this.tb_SendMsg.Clear();
            }
            catch
            { }
        }





    }

    public class User
    {
        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        private string ip;
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }
        private int port;
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        //private bool isOnline=false;
        //public bool IsOnline { get; set; }
    }
}

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
    public partial class Login : Form
    {
        private TcpClient tc;
        //声明网络流
        private NetworkStream ns;
        private BinaryReader br;
        private BinaryWriter bw;
        public Login()
        {
            InitializeComponent();
            tb_IP.Text = "127.0.0.1";
            tb_Port.Text = "9999";
            this.Text = "登录";
        }

        private void btn_Login_Click(object sender, EventArgs e)
        {
            string username = tb_Username.Text.Trim();
            string password = tb_Password.Text.Trim();
            string ipstr = tb_IP.Text.Trim();
            string poitstr = tb_Port.Text.Trim();


            StringBuilder sb = new StringBuilder();
            sb.Append("login#");
            sb.Append(username + "#" + password);

            try
            {
                lb_ShowMsg.Text = "正在连接到主机";
                tc = new TcpClient(ipstr, int.Parse(poitstr));
                //实例化网络流对象
                ns = tc.GetStream();
                br=new BinaryReader(ns);
                bw=new BinaryWriter(ns);
                bw.Write(sb.ToString());
                bw.Flush();
                string info = null;
                try
                {
                   info = br.ReadString();
                }
                catch (Exception)
                {
                    lb_ShowMsg.Text = "服务器无响应";
                }
                if (info == null)
                {
                    lb_ShowMsg.Text = "登陆失败";
                }
                else
                {
                    string[] splitString = info.Split('#');
                    switch (splitString[0])
                    {
                        case "login fail":
                            lb_ShowMsg.Text = "用户名或密码错误";
                            break;
                        case "already login":
                            lb_ShowMsg.Text = "请不要重复登陆";
                            break;
                        case "users":
                            Main main = new Main();
                            main.Username = username;
                            main.Users = info;
                            main.ServerIP = tb_IP.Text.Trim();
                            main.ServerPort = tb_Port.Text.Trim();
                            main.Br = br;
                            main.Bw = bw;
                            this.Hide();
                            main.ShowDialog();
                            break;
                        default:
                            MessageBox.Show(info);
                            break;
                    }
                }
            }
            catch
            {
                lb_ShowMsg.Text = "无法连接到主机";
            }
        }

        private void btn_SignUp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tb_IP.Text.Trim()) || string.IsNullOrEmpty(tb_Port.Text.Trim()))
                MessageBox.Show("服务器IP、端口不能为空");
            else
            {
                SignUp sign = new SignUp();
                sign.ServerIP = tb_IP.Text;
                sign.ServerPort = tb_Port.Text;
                sign.ShowDialog();
            }   
        }

    }
}

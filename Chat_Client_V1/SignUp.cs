using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_Client_V1
{
    public partial class SignUp : Form
    {
        private TcpClient tc;
        //声明网络流
        private NetworkStream ns;
        private BinaryReader br;
        private BinaryWriter bw;
        public string ServerIP;
        public string ServerPort;
        public SignUp()
        {
            InitializeComponent();
        }

        private void btn_submit_Click(object sender, EventArgs e)
        {
            string username = tb_name.Text.Trim();
            string password = tb_password.Text.Trim();
            string cpassword = tb_cpassword.Text.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(cpassword))
                MessageBox.Show("不能为空！");
            else
            {
                if (!password.Equals(cpassword))
                {
                    MessageBox.Show("两次密码不一致！");
                }
                else
                {

                    try
                    {
                        tc = new TcpClient(ServerIP, int.Parse(ServerPort));
                        //实例化网络流对象
                        ns = tc.GetStream();
                        br = new BinaryReader(ns);
                        bw = new BinaryWriter(ns);

                        bool isSignUp = true;
                        StringBuilder sb = new StringBuilder();
                        sb.Append("signup#");
                        sb.Append(username + "#" + password);
                        bw.Write(sb.ToString());
                        bw.Flush();
                        //while (isSignUp)
                        //{
                        string info = null;
                        try
                        {
                            info = br.ReadString();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("服务器无响应");
                        }
                        if (info == null)
                        {
                            MessageBox.Show("注册失败");
                        }
                        else
                        {
                            string[] splitString = info.Split('#');
                            switch (splitString[0])
                            {
                                case "null":
                                    MessageBox.Show("用户名或密码为空！");
                                    break;
                                case "accounted":
                                    MessageBox.Show("用户名已被占用！");
                                    break;
                                case "success":
                                    MessageBox.Show("注册成功！");
                                    isSignUp = false;
                                    this.Hide();
                                    break;
                                case "error":
                                    MessageBox.Show("未知错误！");
                                    break;
                            }
                        }
                        //  }
                        br.Close();
                        bw.Close();
                        tc.Close();

                    }
                    catch (Exception)
                    {
                        MessageBox.Show("连接服务器失败");
                    }
                }
            }

        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            tb_name.Text = "";
            tb_password.Text = "";
            tb_cpassword.Text = "";
        }

        private void SignUp_FormClosed(object sender, FormClosedEventArgs e)
        {
                if (br != null)
                    br.Close();
                if (bw != null)
                    bw.Close();
                if (tc != null)
                    tc.Close();
        }
    }
}

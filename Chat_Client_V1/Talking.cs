using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_Client_V1
{
    public partial class Talking : Form
    {

        public string UserName { get; set; }
        public string ToName { get; set; }
        public string ToIP { get; set; }

        UdpClient uc;
        public Talking()
        {
            InitializeComponent();
        }
        string getmessage = string.Empty;
        public Talking(string message)
        {
            getmessage = message;
            InitializeComponent();
        }

        private void Talking_Load(object sender, EventArgs e)
        {
            uc = new UdpClient();
            this.Text = "和" + ToName + "聊天中";
            if (!string.IsNullOrEmpty(getmessage))
            {
                ShowTalking();
                AddMessage(getmessage, true);
            }

        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            string temp = this.tb_SendMsg.Text; //保存TextBox文本
            //将该文本转化为字节数组
            byte[] b = System.Text.Encoding.UTF8.GetBytes("singlemsg#" + UserName + "#" + temp);
            //向本机的8888端口发送数据
            string[] _ip = ToIP.Split(':');
            uc.Send(b, b.Length, _ip[0], int.Parse(_ip[1]));
            AddMessage(temp, false);
            this.tb_SendMsg.Clear();
        }
        public void AddMessage(string str, bool isuser)
        {
            int startindex = this.rtb_ShowMsg.Text.Length;

            string message = string.Empty;

            if (isuser)
                message = "【" + ToName + "】  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + str + "\n";
            else
                message = "【" + UserName + "】  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" + str + "\n";
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
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern long FlashWindow(IntPtr hwnd, bool bInvert);
        private static void FlashWindow(object _handle)
        {
            IntPtr handle = (IntPtr)_handle;
            int flashindex = 0;
            while (true)
            {
                if (flashindex > 5)
                    break;
                FlashWindow(handle, true);
                flashindex++;
                Thread.Sleep(500);
            }
        }
        public void ShowTalking()
        {
            Thread _thread = new Thread(FlashWindow);
            _thread.IsBackground = true;
            _thread.Start(this.Handle);
        }

        private void Talking_FormClosed(object sender, FormClosedEventArgs e)
        {
            Main.RemoveTalking(this);
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server_cmd
{
    class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsLogin { get; set; }
        //private bool isOnline=false;
        //public bool IsOnline { get; set; }
        public TcpClient client;
        public BinaryReader br;
        public BinaryWriter bw;
        public User(TcpClient client)
        {
            this.client = client;
            NetworkStream networkStream = client.GetStream();
            br = new BinaryReader(networkStream);
            bw = new BinaryWriter(networkStream);
        }

    }
}

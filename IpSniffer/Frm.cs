using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.IO;

namespace IpSniffer
{
    public partial class Frm : Form
    {
        public Thread Listener;
        public delegate void FileListDelegate(byte[] buf);
        private FileListDelegate fileListDelegate;

        public Frm()
        {
            InitializeComponent();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            fileListDelegate = new FileListDelegate(AddFiles);
            Listener = new Thread(new ThreadStart(Run));
            Listener.Start();
        }

        public void Run()
        {
            int len_receive_buf = 4096;
            int len_send_buf = 4096;
            byte[] receive_buf = new byte[len_receive_buf];
            byte[] send_buf=new byte[len_send_buf];
            int cout_receive_bytes;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Blocking = false;
            IPHostEntry IPHost = Dns.GetHostByName(Dns.GetHostName());
            socket.Bind(new IPEndPoint(IPAddress.Parse(IPHost.AddressList[0].ToString()),5000));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, 1);
            byte[] IN = new byte[4] { 1, 0, 0, 0 };
            byte[] OUT = new byte[4];
            int SIO_RCVALL = unchecked((int)0x98000001);
            int ret_code = socket.IOControl(SIO_RCVALL, IN, OUT);
            while (true)
            {
                IAsyncResult ar = socket.BeginReceive(receive_buf, 0, len_receive_buf, SocketFlags.None, null, this);
                cout_receive_bytes = socket.EndReceive(ar);
                Receive(receive_buf, cout_receive_bytes);
            }
        }
        public void Receive(byte[] buf, int len)
        {
            if (buf[9] == 6)
            {
                IAsyncResult r = BeginInvoke(fileListDelegate, new object[] { buf });
                //lbPackets.Items.Add(buf.ToString());
                //lbPackets.Items.Add(Encoding.ASCII.GetString(buf).Replace("\0", " "));
            }
        }

        private void AddFiles(byte[] buf)
        {
                lbPackets.Items.Add(Encoding.ASCII.GetString(buf).Replace("\0", " "));
                //char[] charar = lbPackets.Items[0].ToString().ToCharArray();
                //textBox1.Text= lbPackets.Items[0].ToString();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            btnStart.Enabled = true;
            if (Listener != null)
            {
                Listener.Abort();
                Listener.Join();
                Listener = null;
            }
        }

        private void Frm_Load(object sender, EventArgs e)
        {

        }
    }
}

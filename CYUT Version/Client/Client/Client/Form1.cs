using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace Client
{
    public partial class Form1 : Form
    {
        Socket clientSocket=null;
        delegate void setTextDel(String tmpStr);
        Thread clientThread;
       // Socket[] serverSocket;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string hostname;
            hostname = Dns.GetHostName();
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);
            foreach (IPAddress addrList in host.AddressList)
            {

                // Console.WriteLine( addrList);
                comboBox1.Items.Add(addrList);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                IPAddress serverIp = IPAddress.Parse(comboBox1.Text);
                IPEndPoint serverhost;
               
                serverhost = new IPEndPoint(serverIp,int.Parse(textBox1.Text));
                Console.WriteLine("Test:" + int.Parse(textBox1.Text)+serverIp);
                clientSocket.Connect(serverhost);
                Console.WriteLine("Server is connected....");
                clientThread = new Thread(reviceThreadProc);
                clientThread.Start();
            
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] msg;
            msg = Encoding.Default.GetBytes(textBox2.Text);
            clientSocket.Send(msg, 0, msg.Length, SocketFlags.None);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }
        private void reviceThreadProc()
        {
            try
            {
                byte[] bytes = new byte[1024];
                int rcvBytes;
                String tmpStr;
                do
                {
                    rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                    tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes);
                    setText(tmpStr);

                }
                while (clientSocket.Available != 0 || rcvBytes != 0);
        
               
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void setText(String tmpStr)
        {
            if (textBox2.InvokeRequired == true)
            {
                setTextDel d = new setTextDel(setText);
                Invoke(d, tmpStr);
            }
            else
            {
                textBox2.Text = tmpStr;
            }
        }
        private void Form1_FormClosing(Object sender,System.Windows.Forms.FormClosingEventArgs e)
        {
            if (clientSocket != null)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            if(clientThread != null)
            {
                clientThread.Abort();
            }
          
        }
    }
}

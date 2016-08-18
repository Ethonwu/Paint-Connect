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
 namespace WindowsFormsApplication1
{
  
    public partial class Form1 : Form
    {
        //Socket clientSocket = null;

        Socket serverSocket = null;
        Thread acceptThread;
        ListenClient lc;        
         public Form1()
        {
            InitializeComponent();
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string hostname;
            hostname =  Dns.GetHostName();
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);
            foreach(IPAddress addrList in host.AddressList)
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
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                 IPAddress serverIp =IPAddress.Parse(comboBox1.Text);
                 IPEndPoint serverhost =  new IPEndPoint(serverIp,int.Parse(textBox1.Text)); 
                
                  serverSocket.Bind(serverhost);
                serverSocket.Listen(10);
                Console.WriteLine("Server is listening");
            }  
            catch(Exception ex)
                {
                    MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
             
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

                //clientSocket = serverSocket.Accept();
                //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() +"is connected.");
                lc = new ListenClient(serverSocket, this); 
                acceptThread = new Thread(lc.ServerThreadProc);
                acceptThread.Start();

                    }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*byte[] bytes = new byte[1024];
            int i;
            i = clientSocket.Receive(bytes, 0, clientSocket.Available, SocketFlags.None);
            textBox2.Text = Encoding.Default.GetString(bytes, 0, i);*/
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            //clientSocket.Shutdown(SocketShutdown.Both);
           // clientSocket.Close();
            serverSocket.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
       private void Form1_Form(Object sender,System.Windows.Forms.FormClosedEventArgs e)
        {
            if (lc != null)
            {
                if(lc.clientSocket != null)
                {
                    lc.clientSocket.Shutdown(SocketShutdown.Both);
                    lc.clientSocket.Close();
                }
                acceptThread.Abort(); 
            }
            if (serverSocket != null)
            {
                serverSocket.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Byte[] msg = Encoding.Default.GetBytes(textBox2.Text);
            lc.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None);
        }
    }


    class ListenClient
    {
        private Socket serverSocket;
        public Socket clientSocket;
        private Form1 f1 = new Form1();
        delegate void setTextDel(String tmpStr);
        public ListenClient(Socket tmpSocket, Form1 tmpForm1)
        {
            serverSocket = tmpSocket;
            f1 = tmpForm1;
        }
        public void ServerThreadProc()
        {
            while(true)
            {
                try
                {
                    clientSocket = serverSocket.Accept();
                    Thread t = new Thread(receiveThreadProc);
                    t.Start();
                    Console.WriteLine(clientSocket.RemoteEndPoint.ToString() + "is connected");
                 }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        private void receiveThreadProc()
        {
            try
            {
                Byte[] bytes = new Byte[1024];
                int rcvBytes;
                string tmpStr;
                do
                {
                    rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                    tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes);
                    setText(tmpStr);

                }
                while (clientSocket.Available != 0 || rcvBytes != 0);
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void setText(string tmpStr)
        {
            if(f1.textBox2.InvokeRequired == true)
            {
                setTextDel d = new setTextDel(setText);
                f1.Invoke(d, tmpStr);
            }
            else
            {
                f1.textBox2.Text = tmpStr;
            }
        }
    }
  
}

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
using System.Text.RegularExpressions;
using System.IO;
namespace WindowsFormsApplication1
{
    
    public partial class Form1 : Form
    {
        //Socket clientSocket = null;
        public Graphics g;
        public Bitmap bmp;
        int oldx, oldy;
        int server_size = 1;
        public int client_size = 1;
          //Socket serverSocket = null;  
       public TcpListener tcpListener=null;
        Thread acceptThread;
        ListenClient lc;
        bool connect = false;
        delegate void setLabelDel(String dir);
        Color server_color = Color.Red;
        public Color client_color = Color.Blue;
       
        //delegate void setTextDel(String tmpStr);      
        //
        //  bool isMouseUp = false;
        // int oldx1, oldy1;
        //  int oldx2, oldy2;
        public Form1()
        {
            InitializeComponent();
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
           //     serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                 IPAddress serverIp =IPAddress.Parse(comboBox1.Text);
                // IPEndPoint serverhost =  new IPEndPoint(serverIp,int.Parse(textBox1.Text));
                IPEndPoint serverhost = new IPEndPoint(serverIp,888);
             //   serverSocket.Bind(serverhost);
               //  serverSocket.Listen(10);
                tcpListener = new TcpListener(serverhost);
                tcpListener.Start(10);
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
                //  lc = new ListenClient(serverSocket, this); 
                lc = new ListenClient(tcpListener,this);
            //    lc = new ListenClient(tcpListener,);
             //   lc = new ListenClient(tcpListener, this);

                acceptThread = new Thread(lc.ServerThreadProc);
                acceptThread.Start();
                connect = true;
                    }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }              
        private void button5_Click(object sender, EventArgs e)
        {
            //  lc.clientSocket.Shutdown(SocketShutdown.Both);
            //lc.clientSocket.Close();
            //serverSocket.Close();

            lc.networkstream.Close();
            tcpListener.Stop();
            connect = false;
          
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;
           
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
       private void Form1_Form(Object sender,System.Windows.Forms.FormClosedEventArgs e)
        {
            if (lc != null)
            {
                if(lc.networkstream != null)
                {
                    //lc.clientSocket.Shutdown(SocketShutdown.Both);
                    lc.networkstream.Close();
                }
                acceptThread.Abort(); 
            }
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Byte[] msg = Encoding.Default.GetBytes("Mesg" + textBox2.Text);
            //lc.clientSocket.Send(msg, 0, msg.Length, SocketFlags.None);
            lc.networkstream.Write(msg, 0, msg.Length);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "認意格式|*.*";
            openFileDialog1.Title = "請選擇想要傳送的檔案";
            if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Byte[] preBuffer = Encoding.Default.GetBytes("File");
                Byte[] postBuffer = Encoding.Default.GetBytes("####");
                //   lc.clientSocket.SendFile(openFileDialog1.FileName, preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread);
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
                byte[] bytes = new byte[1024];
                int byteRead;
                lc.networkstream.Write(preBuffer, 0, preBuffer.Length);
                do
                {
                    byteRead = fs.Read(bytes, 0, bytes.Length);
                    lc.networkstream.Write(bytes, 0, byteRead);
                }
                while (byteRead > 0);
                lc.networkstream.Write(postBuffer, 0, postBuffer.Length);
                fs.Close();
            }

        }
        private void button8_Click(object sender, EventArgs e) //Send Big File
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "認意格式|*.*";
            openFileDialog1.Title = "請選擇想要傳送的檔案";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //  Byte[] preBuffer = Encoding.Default.GetBytes("File");
                Byte[] preBuffer = Encoding.Default.GetBytes("Larg");
                Byte[] postBuffer = Encoding.Default.GetBytes("####");
                //   lc.clientSocket.SendFile(openFileDialog1.FileName, preBuffer, postBuffer, TransmitFileOptions.UseDefaultWorkerThread);
                FileStream fs = new FileStream(openFileDialog1.FileName, FileMode.Open);
                byte[] bytes = new byte[1024];
                int byteRead;
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                string tmp = fi.Length.ToString();
                byte[] lenBuffer = Encoding.Default.GetBytes(tmp.PadLeft(10)); 
              
                lc.networkstream.Write(preBuffer, 0, preBuffer.Length);
                lc.networkstream.Write(lenBuffer, 0, lenBuffer.Length);
                do
                {
                    byteRead = fs.Read(bytes, 0, bytes.Length);
                    lc.networkstream.Write(bytes, 0, byteRead);
                }
                while (byteRead > 0);
               // lc.networkstream.Write(postBuffer, 0, postBuffer.Length);
                fs.Close();
            }
        }
        private void picture_box1_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e)
        {
           
            string my_first_location;
            Byte[] toge;      
            if (e.Button == MouseButtons.Left)
            {
                oldx = e.X;
                oldy = e.Y;              
                my_first_location = "Draw" + "[" + oldx+ "]" + "[" + oldy+ "]";
                if (connect == true)
                {
                    toge = Encoding.Default.GetBytes(my_first_location);
                    //lc.clientSocket.Send(toge, 0, toge.Length, SocketFlags.None);
                    lc.networkstream.Write(toge, 0, toge.Length);
                }
            }
        }
        private void picture_box1_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            string move_location;
            byte[] togomove;
            byte[] send_server_size;
            if(e.Button == MouseButtons.Left)
            {
                move_location = "Move" + "[" + e.X + "]" + "[" + e.Y + "]";
                //Console.WriteLine(move_location);
                server_size = comboBox2.SelectedIndex;
                if(connect == true)
                {
                    togomove = Encoding.Default.GetBytes(move_location);
                    send_server_size = Encoding.Default.GetBytes("chsi" + server_size);
                    //  lc.clientSocket.Send(togomove, 0, togomove.Length, SocketFlags.None);
                    // lc.clientSocket.Send(send_server_size, 0, send_server_size.Length, SocketFlags.None);
                    lc.networkstream.Write(togomove, 0, togomove.Length);
                    lc.networkstream.Write(send_server_size, 0, send_server_size.Length);
                }
                g.DrawLine(new Pen(server_color, server_size), oldx, oldy, e.X, e.Y);
                pictureBox1.Image = bmp;
                oldx = e.X;
                oldy = e.Y;
            }
        }

        private void picture_box1_MouseDoubleClick(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            byte[] togee;
            g.Clear(Color.White);
            pictureBox1.Image = bmp;
            if(connect == true)
            {
                togee = Encoding.Default.GetBytes("Clea");
                //lc.clientSocket.Send(togee, 0, togee.Length, SocketFlags.None);
                lc.networkstream.Write(togee, 0, togee.Length);
            }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] color_change_for_server;
        
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {

                server_color = colorDialog1.Color;
                 string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco"+color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
                
            }
           
        }

        private void picture_box1_MouseUp(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            byte[] enough;
            Console.WriteLine("Up");
            enough = Encoding.Default.GetBytes("iMup");
            if (connect == true)
                //   lc.clientSocket.Send(enough, 0, enough.Length, SocketFlags.None);
                lc.networkstream.Write(enough, 0, enough.Length);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
          
        }
    }


    class ListenClient
    {
        // private Socket serverSocket;
        //public Socket clientSocket;
        private TcpClient tcpClient;
        private TcpListener tcpListener;
        public NetworkStream networkstream;
        Thread clientThread;
        private Form1 f1 = new Form1();
        delegate void setTextDel(String tmpStr);
        //delegate void setLabelDel(String dir);
        bool isMouseUp = false;
        int oldx1, oldy1;
        int oldx2, oldy2;
        //public object label4;
       //  Graphics g;
        // Bitmap bmp;
            
        int penWidth=1;
     //   bmp = new Bitmap(f1.pictureBox1.Width, f1.pictureBox1.Height);
    
        public ListenClient(TcpListener serverSocket, Form1 tmpForm1)
        {
            //serverSocket = tmpSocket;
            //f1 = tmpForm1;
            this.tcpListener = serverSocket;
            f1 = tmpForm1;
         
        }
        public void ServerThreadProc()
        {
            while(true)
            {
                try
                {
                    //  clientSocket = serverSocket.Accept();
                    tcpClient = tcpListener.AcceptTcpClient();
                    networkstream = tcpClient.GetStream();
                    clientThread = new Thread(receiveThreadProc);
                    clientThread.Start();
                    //Thread t = new Thread(receiveThreadProc);
                    //  t.Start();
                    //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() + "is connected");
                    Console.WriteLine(tcpClient.Client.RemoteEndPoint.ToString() + "is connected");
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
                    // rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                    rcvBytes = networkstream.Read(bytes, 0, bytes.Length);
                    tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes);
                    //setText(tmpStr);
                   // MessageBox.Show("1");
                 //   Console.WriteLine("Get:"+tmpStr);
                    if(tmpStr.Length >= 4)
                    {
                        switch(tmpStr.Substring(0,4))
                        {
                            case "Mesg":
                                setText(tmpStr.Substring(4, tmpStr.Length - 4));
                               // setText(tmpStr);
                                break;
                            case "Draw":
                                drowing(tmpStr);
                                break;
                            case "Move":
                                drowing_line(tmpStr);
                                break;
                            case "iMup":
                                isMouseUp = false;

                                break;
                            case "Clea":
                                
                                f1.g.Clear(Color.White);
                                f1.pictureBox1.Image = f1.bmp;
                                break;
                            case "chco":
                                change_color(tmpStr.Substring(4, tmpStr.Length - 4));
                                break;
                            case "chsi":
                                f1.client_size = Int32.Parse(tmpStr.Substring(4, tmpStr.Length - 4));
                                break;

                        }
                    }
              
                }
                while (networkstream.DataAvailable ==true || rcvBytes != 0);
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private void change_color(string tmpStr)
        {
            f1.client_color = System.Drawing.ColorTranslator.FromHtml(tmpStr);
        }
        private void drowing(string tmpStr)
        {
            isMouseUp = true;
            string input = tmpStr;
            String pattern = @"\[([^\[\]]+)\]";
            int[] value1 = new int[100];
            int ctr = 0;
            foreach (Match m in Regex.Matches(input, pattern))
            {
                value1[ctr++] = Int32.Parse(m.Groups[1].Value);
            }
            oldx1 = value1[0];
            oldy1 = value1[1];


        }
        private void drowing_line(string tmpStr)
        {
           // Console.WriteLine(1);
            isMouseUp = true;
            string input = tmpStr;
            String pattern = @"\[([^\[\]]+)\]";
            int[] value2 = new int[100];
            int ctr1 = 0;
            foreach (Match m in Regex.Matches(input, pattern))
            {
                value2[ctr1++] = Int32.Parse(m.Groups[1].Value);

            }
            oldx2 = value2[0];
            oldy2 = value2[1];
            if (isMouseUp == true)
            {
                f1.g.DrawLine(new Pen(f1.client_color, f1.client_size), oldx1, oldy1, oldx2, oldy2);
                f1.pictureBox1.Image = f1.bmp;
                oldx1 = oldx2;
                oldy1 = oldy2;
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

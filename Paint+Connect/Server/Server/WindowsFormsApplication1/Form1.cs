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
using System.Reflection;
using System.Drawing.Drawing2D;

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
        bool connect_client = false;
        delegate void setLabelDel(String dir);
        Color server_color = Color.Red;
        public Color client_color = Color.Blue;

        public Form1()
        {
            InitializeComponent();
        }

 
        private void button5_Click(object sender, EventArgs e)
        {         
            lc.networkstream.Close();
            tcpListener.Stop();
            connect = false;
          
        } 
        private void Form1_Load(object sender, EventArgs e)
        {
            string hostname;
            hostname = Dns.GetHostName();
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);
            foreach (IPAddress addrList in host.AddressList)
            {
                if(addrList.AddressFamily == AddressFamily.InterNetwork)
                // Console.WriteLine( addrList);
                comboBox1.Items.Add(addrList);
            }
            comboBox1.SelectedIndex = 0;
            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bmp;
            
        }    
      
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.ForeColor = Color.Red;
            
        }
        private void enter_send(object sender,System.Windows.Forms.KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                button6.Focus();
                button6_Click(sender, e);
                textBox2.Focus();
                textBox2.Clear();
            }
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
            if(connect==true)
            lc.networkstream.Write(msg, 0, msg.Length);
            // listBox1.ForeColor = Color.Red;      
           // string se = "Server:"+textBox2.Text;
           
          //   se = System.Drawing.Color.Red.ToString();
            listBox1.Items.Add("Server:"+textBox2.Text);
         
            textBox2.Clear();
        }
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                Brush mybsh = Brushes.Black;
                
                if (listBox1.Items[e.Index].ToString().IndexOf("Server:") != -1)
                {
                    mybsh = Brushes.Red;
                }
                else if (listBox1.Items[e.Index].ToString().IndexOf("Client:") != -1)
                {
                    mybsh = Brushes.Blue;
                }
                
                e.DrawFocusRectangle();
               
                e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), e.Font, mybsh, e.Bounds, StringFormat.GenericDefault);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
          

        }
        private void button8_Click(object sender, EventArgs e) //Send Big File
        {
         
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
            //byte[] send_server_size;
         

            if (e.Button == MouseButtons.Left)
            {
                move_location = "Move" + "[" + e.X + "]" + "[" + e.Y + "]";
                //Console.WriteLine(move_location);
               // server_size = comboBox2.SelectedIndex;
                if(connect == true)
                {
                    togomove = Encoding.Default.GetBytes(move_location);
                    //send_server_size = Encoding.Default.GetBytes("chsi" + server_size);
                    //  lc.clientSocket.Send(togomove, 0, togomove.Length, SocketFlags.None);
                    // lc.clientSocket.Send(send_server_size, 0, send_server_size.Length, SocketFlags.None);
                    lc.networkstream.Write(togomove, 0, togomove.Length);
                 //   lc.networkstream.Write(send_server_size, 0, send_server_size.Length);
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] send_server_size;
            server_size = comboBox2.SelectedIndex;
            send_server_size = Encoding.Default.GetBytes("chsi" + server_size);
            if(connect==true)
            lc.networkstream.Write(send_server_size, 0, send_server_size.Length);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void textBox2_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                textBox2.Clear();
            }
        }
        private void button1_click(object sender,EventArgs e)
        {

        }
        private void button9_Click(object sender, EventArgs e)
        {
          
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                //     serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                IPAddress serverIp = IPAddress.Parse(comboBox1.Text);
                // IPEndPoint serverhost =  new IPEndPoint(serverIp,int.Parse(textBox1.Text));
                IPEndPoint serverhost = new IPEndPoint(serverIp, 888);
                //   serverSocket.Bind(serverhost);
                //  serverSocket.Listen(10);
                tcpListener = new TcpListener(serverhost);
                tcpListener.Start(10);
                Console.WriteLine("Server is listening");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            try
            {

                //clientSocket = serverSocket.Accept();
                //Console.WriteLine(clientSocket.RemoteEndPoint.ToString() +"is connected.");
                //  lc = new ListenClient(serverSocket, this); 
                lc = new ListenClient(tcpListener, this);
                //    lc = new ListenClient(tcpListener,);
                //   lc = new ListenClient(tcpListener, this);

                acceptThread = new Thread(lc.ServerThreadProc);
                acceptThread.Start();
                connect_client = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
         
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            
            server_color = Color.Red;
            if(connect==true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            server_color = Color.Black;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            server_color = Color.Gray;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            server_color = Color.Yellow;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            server_color = Color.Green;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            server_color = Color.Orange;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            server_color = Color.Blue;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            server_color = Color.Brown;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            server_color = Color.Pink;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            server_color = Color.Purple;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server_size = 1;
            server_color = Color.Red;
            g.Clear(Color.White);
            pictureBox1.Image = bmp;

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

            if (connect_client == false)
            {
                MessageBox.Show("You are not connect yet");
                
                return;
            }
            else
                connect = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            connect = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "(*.jpg;*.jpeg;*.PNG;*.png)|*.jpg;*.jpeg;*.PNG;*.png";          
            saveFileDialog1.Title = "Chose where you want to save";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap mp = new Bitmap(pictureBox1.Image);
                bmp.Save(saveFileDialog1.FileName);

            }
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            server_color = Color.White;
            if (connect == true)
            {
                byte[] color_change_for_server;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);              
                color_change_for_server = Encoding.Default.GetBytes("chco" + color_str);               
                lc.networkstream.Write(color_change_for_server, 0, color_change_for_server.Length);
            }
        }    
    }
    class ListenClient
    { 
        private TcpClient tcpClient;
        private TcpListener tcpListener;
        public NetworkStream networkstream;
        Thread clientThread;
        private Form1 f1 = new Form1();
        delegate void setTextDel(String tmpStr);        
        bool isMouseUp = false;
        int oldx1, oldy1;
        int oldx2, oldy2;           
        public ListenClient(TcpListener serverSocket, Form1 tmpForm1)
        {           
            this.tcpListener = serverSocket;
            f1 = tmpForm1;
         
        }
        public void ServerThreadProc()
        {
            while(true)
            {
                try
                {
                    tcpClient = tcpListener.AcceptTcpClient();
                    networkstream = tcpClient.GetStream();
                    clientThread = new Thread(receiveThreadProc);
                    clientThread.Start();                  
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
                    rcvBytes = networkstream.Read(bytes, 0, bytes.Length);
                    tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes);
                    //setText(tmpStr);                 
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
                string cli = "Client:";
                cli = System.Drawing.Color.Blue.ToString();                 
                 f1.listBox1.Items.Add("Client:" + tmpStr);        
            }
        }
      
    }
  
}

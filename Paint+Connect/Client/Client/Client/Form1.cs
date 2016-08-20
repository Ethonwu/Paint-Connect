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
using System.IO;
using System.Text.RegularExpressions;
namespace Client
{
    public partial class Form1 : Form
    {
        //  Socket clientSocket=null;
        TcpClient tcpClient = null;
        NetworkStream networkStream;
        delegate void setProgressDel(string p, int value);
        delegate void setTextDel(String tmpStr);
        delegate void setPictureBoxDel(String filename);
        delegate void setLabelDel(String dir);
        Graphics g;
        bool connect = false;
        Color server_color = Color.Red;
        Color client_color = Color.Blue;
        Thread clientThread;
        int oldx, oldy;
        int client_size = 1;
        int server_size = 1;
        int oldx1, oldy1;
        bool isMouseUp = false;
        int oldx2, oldy2;
        Bitmap bmp;       
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
            //    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
                
                IPAddress serverIp = IPAddress.Parse(comboBox1.Text);
                IPEndPoint serverhost;             
                serverhost = new IPEndPoint(serverIp,888);
                //Console.WriteLine("Test:" + int.Parse(textBox1.Text)+serverIp);
                //  clientSocket.Connect(serverhost);
                tcpClient = new TcpClient();
                tcpClient.Connect(serverhost);
                networkStream = tcpClient.GetStream();
                Console.WriteLine("Server is connected....");
                connect = true;
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
            bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, pictureBox2.Width, pictureBox2.Height);
            pictureBox2.Image = bmp;
            
           /* int[] size = { 1, 5, 10, 12 };
            for (int i = 0; i <= 3; i++)
            {
                comboBox2.Items.Add(size[i]);
            }*/
            
           
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] msg;
            msg = Encoding.Default.GetBytes("Mesg"+ textBox2.Text);
            //   clientSocket.Send(msg, 0, msg.Length, SocketFlags.None);
            networkStream.Write(msg, 0, msg.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //    clientSocket.Shutdown(SocketShutdown.Both);
            //   clientSocket.Close();
            networkStream.Close();
            connect = false;
        }
        private void reviceThreadProc()
        {
            byte[] bytes = new byte[1024];
            int rcvBytes;
            String tmpStr;
            try
            {
                do
                {
                    //  rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                    rcvBytes = networkStream.Read(bytes, 0, bytes.Length);
                    tmpStr = Encoding.Default.GetString(bytes, 0, rcvBytes);
                    if (tmpStr.Length >= 4)
                    {
                        switch (tmpStr.Substring(0, 4))
                        {
                            case "Mesg":
                                setText(tmpStr.Substring(4, tmpStr.Length - 4));
                                break;
                            case "File":
                                string path = @"c:\test.jpeg";
                                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                                fs.Write(bytes, 4, rcvBytes-4);
                                do
                                {
                                    //   rcvBytes = clientSocket.Receive(bytes, 0, bytes.Length, SocketFlags.None);
                                    rcvBytes = networkStream.Read(bytes, 0, bytes.Length);
                                    tmpStr = Encoding.Default.GetString(bytes, rcvBytes-4, 4);
                                    if(tmpStr == "####")
                                    {
                                        fs.Write(bytes, 0, rcvBytes-4);
                                    }
                                    else
                                    {
                                        fs.Write(bytes, 0, rcvBytes);
                                    }
                                }
                                while (tmpStr == "####");
                                fs.Close();
                                Console.WriteLine("File is received sucessfully");
                                setPictureBox(path);                          
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
                                g.Clear(Color.White);
                                  pictureBox2.Image = bmp;
                                break;
                            case "chsi":
                                server_size = Int32.Parse(tmpStr.Substring(4, tmpStr.Length - 4));
                                 break;
                            case "chco":
                                change_color(tmpStr.Substring(4, tmpStr.Length - 4));
                                break;
                            case "Larg":
                                string lapath= @"C:\test.bin";

                                FileStream fs1 =new FileStream(lapath, FileMode.OpenOrCreate);
                                double len;
                                string len_conv;                        
                                if(tmpStr.Length<=4)
                                {
                                    rcvBytes = networkStream.Read(bytes, 0, 10);                                    
                                    len_conv = Encoding.Default.GetString(bytes, 0,rcvBytes);
                                    len = Convert.ToDouble(len_conv);                                     
                                }
                                else
                                {
                                    len_conv = tmpStr.Substring(4, 10);
                                    len = Convert.ToDouble(len_conv);
                                    fs1.Write(bytes, 14, rcvBytes - 14);
                                    len = len - (rcvBytes-14);
                                }
                                double ratio = len / 200;
                                int trdou = Convert.ToInt32(ratio / len);
                                setProgess("Max", trdou);
                                setProgess("Value", 0);

                                do
                                {
                                    rcvBytes = networkStream.Read(bytes, 0, rcvBytes);
                                    fs1.Write(bytes, 0, rcvBytes);
                                    len -= rcvBytes;
                                    setProgess("Value", progressBar1.Maximum - trdou);
                                }
                                while (len ==0);
                                fs1.Close();
                                break;                          
                        }
                    }

                } 
                while (networkStream.DataAvailable == true || rcvBytes != 0);                      
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void setProgess(string p,int value)
        {
            if(progressBar1.InvokeRequired == true)
            {
                setProgressDel d = new setProgressDel(setProgess);
                this.Invoke(d, p, value);
            }
            else
            {
                switch(p)
                {
                    case "Max":
                        progressBar1.Maximum = value;
                        break;
                    case "Value":
                        progressBar1.Value = value;
                        break;
                }
            }
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
        private void change_color(string tmpStr)        
        {
            server_color = System.Drawing.ColorTranslator.FromHtml(tmpStr);
        }
        private void drowing_line(string tmpStr)
        {
            Console.WriteLine(1);
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
                
               g.DrawLine(new Pen(server_color,server_size), oldx1, oldy1, oldx2, oldy2);
                pictureBox2.Image = bmp;
                oldx1 = oldx2;
                oldy1 = oldy2;
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
        private void setPictureBox(String filename)
        {
            if(pictureBox1.InvokeRequired == true)
            {
                setPictureBoxDel d = new setPictureBoxDel(setPictureBox);
                this.Invoke(d, filename);
                
            }
            else
            {
                pictureBox1.ImageLocation = filename;
            }
        }

       
        private void Form1_FormClosing(Object sender,System.Windows.Forms.FormClosingEventArgs e)
        {
            if (tcpClient != null)
            {
                //clientSocket.Shutdown(SocketShutdown.Both);
                //clientSocket.Close();
                tcpClient.Close();
            }
            if(clientThread != null)
            {
                clientThread.Abort();
            }
          
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            byte[] color_change_for_client;

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {

                client_color = colorDialog1.Color;
                string color_str = System.Drawing.ColorTranslator.ToHtml(client_color);               
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                //clientSocket.Send(color_change_for_client, 0, color_change_for_client.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void setLabel(object sender, EventArgs e)
        {

        }

        private void Picturebox2_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            string my_first_location;
            byte[] toge;
            if(e.Button == MouseButtons.Left)
            {
                oldx = e.X;
                oldy = e.Y;
                my_first_location = "Draw" + "[" + oldx + "]" + "[" + oldy + "]";
                if (connect == true)
                {
                    toge = Encoding.Default.GetBytes(my_first_location);
                    // clientSocket.Send(toge, 0, toge.Length, SocketFlags.None);
                    networkStream.Write(toge, 0, toge.Length);

                }
            }
        }
        private void PictureBox2_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            byte[] togomove;
            string move_location;
            byte[] send_client_size;
            
            if(e.Button == MouseButtons.Left)
            {
                move_location = "Move" + "[" + e.X + "]" + "[" + e.Y + "]";
                client_size = comboBox2.SelectedIndex;

                if (connect == true)
                {
                    togomove = Encoding.Default.GetBytes(move_location);
                    send_client_size = Encoding.Default.GetBytes("chsi" + client_size);
                    //clientSocket.Send(togomove, 0, togomove.Length, SocketFlags.None);
                    //clientSocket.Send(send_client_size, 0, send_client_size.Length, SocketFlags.None);
                    networkStream.Write(togomove, 0, togomove.Length);
                    networkStream.Write(send_client_size, 0, send_client_size.Length);
                }
               
                g.DrawLine(new Pen(client_color, client_size), oldx, oldy, e.X, e.Y);
                pictureBox2.Image = bmp;
                oldx = e.X;
                oldy = e.Y;
            }
        }
        private void PictureBox2_MouseDoubleClick(object sender,System.Windows.Forms.MouseEventArgs e)
        {
            byte[] togee;
            g.Clear(Color.White);
            pictureBox2.Image = bmp;
            if (connect == true)
            {
                togee = Encoding.Default.GetBytes("Clea");
                //clientSocket.Send(togee, 0, togee.Length, SocketFlags.None);
                networkStream.Write(togee, 0, togee.Length);
            }
        }
      
    }
}

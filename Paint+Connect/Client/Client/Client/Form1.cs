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
        bool connect_server = false;
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
                connect_server = true;
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
            string hostname;
            hostname = Dns.GetHostName();
            IPHostEntry host;
            host = Dns.GetHostEntry(hostname);
            foreach (IPAddress addrList in host.AddressList)
            {
                if (addrList.AddressFamily == AddressFamily.InterNetwork)
                    // Console.WriteLine( addrList);
                    comboBox1.Items.Add(addrList);
            }
            comboBox1.SelectedIndex = 0;
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
            if(connect==true)
                networkStream.Write(msg, 0, msg.Length);
            listBox1.ForeColor = Color.Blue;
            listBox1.Items.Add("Client:" + textBox2.Text);
            textBox2.Clear();
        }
        private void enter_send(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3.Focus();
                button3_Click(sender, e);
                textBox2.Focus();
                textBox2.Clear();
            }
        }
        private void textBox2_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                textBox2.Clear();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //    clientSocket.Shutdown(SocketShutdown.Both);
            //   clientSocket.Close();
            networkStream.Close();
            connect = false;
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
                //  textBox2.Text = tmpStr;
                listBox1.ForeColor = Color.Red;
              //  textBox2.ForeColor = Color.Red;
                string ser = "Server=";
                ser = System.Drawing.Color.Black.ToString();
                listBox1.Items.Add("Server:" + tmpStr);
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
            byte[] send_client_size;
            client_size = comboBox2.SelectedIndex;
            send_client_size = Encoding.Default.GetBytes("chsi" + client_size);
            if(connect==true)
            networkStream.Write(send_client_size, 0, send_client_size.Length);
        }

        private void setLabel(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
          
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.ForeColor = Color.Blue;
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {

            saveFileDialog1.Filter = "(*.jpg;*.jpeg;*.PNG;*.png)|*.jpg;*.jpeg;*.PNG;*.png";
            saveFileDialog1.Title = "Chose where you want to save";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap mp = new Bitmap(pictureBox2.Image);
                bmp.Save(saveFileDialog1.FileName);

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            client_size = 1;
            client_color = Color.Blue;
            g.Clear(Color.White);
            pictureBox2.Image = bmp;
        }

        private void pictureBox1_Click_2(object sender, EventArgs e)
        {
            client_color = Color.Red;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            client_color = Color.Black;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            client_color = Color.Gray;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            client_color = Color.Yellow;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            client_color = Color.Green;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            client_color = Color.Orange;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            client_color = Color.Blue;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            client_color = Color.Brown;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            client_color = Color.Pink;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            client_color = Color.Purple;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            client_color = Color.White;
            if (connect == true)
            {
                byte[] color_change_for_client;
                string color_str = System.Drawing.ColorTranslator.ToHtml(server_color);
                //  Console.WriteLine(color_str);
                color_change_for_client = Encoding.Default.GetBytes("chco" + color_str);
                // lc.clientSocket.Send(color_change_for_server, 0, color_change_for_server.Length, SocketFlags.None);
                networkStream.Write(color_change_for_client, 0, color_change_for_client.Length);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (connect_server == false && radioButton1.Checked == true)
            {
                MessageBox.Show("You are not connect yet");
                radioButton1.Checked = false;
            }
            else
                connect = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            connect = false;
            radioButton1.Checked = false;
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
           // byte[] send_client_size;
        
            if (e.Button == MouseButtons.Left)
            {
                move_location = "Move" + "[" + e.X + "]" + "[" + e.Y + "]";
             //   client_size = comboBox2.SelectedIndex;

                if (connect == true)
                {
                    togomove = Encoding.Default.GetBytes(move_location);
                   // send_client_size = Encoding.Default.GetBytes("chsi" + client_size);
                    //clientSocket.Send(togomove, 0, togomove.Length, SocketFlags.None);
                    //clientSocket.Send(send_client_size, 0, send_client_size.Length, SocketFlags.None);
                    networkStream.Write(togomove, 0, togomove.Length);
                    //.Write(send_client_size, 0, send_client_size.Length);
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

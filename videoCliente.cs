using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Touchless.Vision.Camera;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Threading;
using System.Net.Mail;

namespace videoCliente
{
    public partial class videoCliente : Form
    {
        private MemoryStream memory;
        private UdpClient udpClient;
        private IPAddress multicastAddress;
        private IPEndPoint remoteep;
        private Socket socket;
        private byte[] buffer;

        //Chat
        UdpClient chatServer;
        IPEndPoint remoteChat;

        //Mail
        MailMessage mensaje = new MailMessage();
        MailAddress from;
        MailAddress to;
        SmtpClient protocolo = new SmtpClient();
        Boolean estado = true;
        String mensajeError;

        public videoCliente()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        //View images 
        private void visualizar_imagen()
        {
            while (true)
            {
                try
                {
                    buffer = udpClient.Receive(ref remoteep);
                    memory = new MemoryStream(buffer);
                    pictureBox1.Image = Image.FromStream(memory);
                }
                catch { }
            }
        }

        //To prepare the video client
        private void videoCliente_Load(object sender, EventArgs e)
        {
            udpClient = new UdpClient();
            multicastAddress = IPAddress.Parse("224.0.0.1");
            remoteep = new IPEndPoint(IPAddress.Any, 1050);
            udpClient.JoinMulticastGroup(multicastAddress);
            socket = udpClient.Client;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            socket.Bind(remoteep);

            chatServer = new UdpClient(8080);
            IPAddress multicastaddressChat = IPAddress.Parse("224.1.0.1");
            chatServer.JoinMulticastGroup(multicastaddressChat);

            remoteChat = null;

            Thread t = new Thread(new ThreadStart(MyThreadMethod));
            t.Start();
        }

        private void MyThreadMethod()
        {
            while (true)
            {
                Byte[] recieveByte = chatServer.Receive(ref remoteChat);
                string returnData = Encoding.UTF8.GetString(recieveByte);
                
                listBoxChat.Items.Add(returnData);

                textBoxChat.Text = returnData;
            }
        }


        // button to start capturing video
        private void buttonCapturar_Click(object sender, EventArgs e)
        {
            Task t1 = new Task(visualizar_imagen);
            t1.Start();
        }

        //button to send the email
        private void buttonSend_Click(object sender, EventArgs e)
        {
            from = new MailAddress(textBox1.Text);
            to = new MailAddress(textBox3.Text);
            mensaje.Subject = textBox4.Text;
            mensaje.To.Add(to);
            mensaje.From = from;
            mensaje.Body = textBox5.Text;
            
            protocolo = new SmtpClient("smtp.gmail.com", 587);
            protocolo.EnableSsl = true;
            NetworkCredential credenciales = new NetworkCredential(textBox1.Text, textBox2.Text);
            protocolo.Credentials = credenciales;
            mensaje.Attachments.Add(new Attachment(label8.Text));


            try
            {
                protocolo.Send(mensaje);
                textBox6.Text = "Enviado";
            }
            catch (SmtpException error)
            {
                estado = false;
                mensajeError = error.Message.ToString();

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.PasswordChar = '*';
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //codigo para abirr y leer archivo
                label8.Text = openFileDialog1.FileName;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Touchless.Vision.Camera;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace videoServer
{
    public partial class videoServer : Form
    {
        private CameraFrameSource _frameSource;
        private static Bitmap _latestFrame;
        private MemoryStream memoryStream;
        private UdpClient udpServer;
        private IPAddress multicastAddress;
        IPEndPoint remote;

        //CHAT
        private UdpClient chatServer;
        IPEndPoint remoteChat;

        
        public videoServer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox.Items.Clear();
            foreach (Camera cam in CameraService.AvailableCameras)
            {
                comboBox.Items.Add(cam);
            }

            chatServer = new UdpClient();
            IPAddress multicastadressChat = IPAddress.Parse("224.1.0.1");
            chatServer.JoinMulticastGroup(multicastadressChat);

            remoteChat = new IPEndPoint(multicastadressChat, 8080);

        }

        private void startCapturing()
        {

            udpServer = new UdpClient();
            multicastAddress = IPAddress.Parse("224.0.0.1");
            remote = new IPEndPoint(multicastAddress, 1050);
            memoryStream = new MemoryStream();
            udpServer.JoinMulticastGroup(multicastAddress);
            Camera c = (Camera)comboBox.SelectedItem;
            setFrameSource(new CameraFrameSource(c));
            _frameSource.Camera.CaptureWidth = 320;
            _frameSource.Camera.CaptureHeight = 240;
            _frameSource.Camera.Fps = 20;
            _frameSource.NewFrame += OnImageCaptured;
            pictureBox1.Paint += new PaintEventHandler(drawLatestImage);
            _frameSource.StartFrameCapture();
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)//
        {
            if (_frameSource == cameraFrameSource)
                return;
            _frameSource = cameraFrameSource;
        }

        private void drawLatestImage(object sender, PaintEventArgs e) //PDF
        {
            if (_latestFrame != null)
            {
                e.Graphics.DrawImage(_latestFrame, 0, 0, _latestFrame.Width, _latestFrame.Height);
                sendtoclient();
            }
        }

        public void sendtoclient() 
        {
            _latestFrame.Save(memoryStream, ImageFormat.Jpeg);
            udpServer.Send(memoryStream.ToArray(), memoryStream.ToArray().Length, remote);
            memoryStream = new MemoryStream();
        }

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps) //PDF
        {
            _latestFrame = frame.Image;
            pictureBox1.Invalidate();
        }

        private void buttonCapturar_Click(object sender, EventArgs e)
        {
            if (_frameSource != null && _frameSource.Camera == comboBox.SelectedItem)
                return;
            startCapturing();

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (_frameSource != null)
            {
                _frameSource.NewFrame -= OnImageCaptured;
                _frameSource.Camera.Dispose();
                setFrameSource(null);
                pictureBox1.Paint -= new PaintEventHandler(drawLatestImage);
            }
            pictureBox1.Invalidate();

        }

        private void buttonChat_Click(object sender, EventArgs e)
        {
            Byte[] sendBytes = Encoding.ASCII.GetBytes(richTextBox1.Text);
            chatServer.Send(sendBytes, sendBytes.Length, remoteChat);

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

    }
}

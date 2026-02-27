using Asbt.FaceDetection;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FaceDetection
{
    public partial class FrmNew : Form
    {
        public FrmNew()
        {
            InitializeComponent();
        }


        MyFaceDetection faceDetection;

        private void FrmMain_Load(object sender, EventArgs e)
        {
            string imagePath = System.IO.Path.Combine(Application.StartupPath, @"images\726A457986E4463AB2A464E00F7DE6D9.jpg");
            if (System.IO.File.Exists(imagePath))
            {
                pictureBox1.Load(imagePath);
                faceDetection = new MyFaceDetection();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
           pictureBox2.Image = faceDetection.Find(pictureBox1.Image);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            faceDetection.Save(Application.StartupPath+"\\fc1.png");            
        }
    }
}

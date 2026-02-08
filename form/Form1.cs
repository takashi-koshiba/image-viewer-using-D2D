using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static imageViewer.D2DRenderer;

namespace imageViewer
{
    public partial class Form1 : Form
    {
        private ImageController image;
        private const float ScaleUp = 1.4f;
        private const float ScaleDown = 1.0f / ScaleUp;
        private string[] _args;
        public Form1(string[] args)
        {
            InitializeComponent();
            _args = args;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            image = new ImageController( this.Width, this.Height,this);
            if (_args.Length > 0) {
                image.addImage(this.ClientSize.Width, this.ClientSize.Height, _args[0]);
     
                this.Controls.Add(image.getPictureBoxAt(image.Count()-1));

            }

            this.MouseWheel += Form1_MouseWheel;

            this.AllowDrop = true;

            this.DragDrop += Form1_DragDrop;
            this.DragEnter += Form1_DragEnter;
            this.BackColor=Color.White;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                image.addImage(this.ClientSize.Width, this.ClientSize.Height, file);

                this.Controls.Add(image.getPictureBoxAt(image.Count()-1));

            }

        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) { image.AddScalingToPoint(ScaleUp); }
            else
            {
                image.AddScalingToPoint(ScaleDown);
            }


            
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                this.Close();
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            image.Dispose();
        }



        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData== Keys.R)
            {
                image.Resize(this.Width,this.Height);
                image.drawImg();

            }

        }

        

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            //描画先の画面サイズの更新
            //  image.setDisplaySize(this.Width, this.Height);

            //d2d側の画面サイズ更新
            // image.OnResize((uint)this.Width, (uint)this.Height);

            //  image.FitImageToWindow();
            image.Resize(this.Size.Width,this.Size.Height);
           
            image.drawImg();
        }
    }
}

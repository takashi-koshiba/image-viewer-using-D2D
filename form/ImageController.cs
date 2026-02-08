using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace imageViewer
{
    internal class ImageController : IDisposable
    {
        private List<PictureBox> _pictureBox;
        private List<loadImage> _loadImage;
        private bool _disposed;

        private int Column;

        private static PointF currentMousePos;
        private static PointF pendingDelta = new PointF();
        private static bool isDragging = false;
        private static PointF lastMousePos;

        private Form _parent;
        public ImageController(int width,int   height,Form parent)
        {

            Column = 2;
            _pictureBox = new List<PictureBox>();
            _loadImage = new List<loadImage>();
            pendingDelta = new PointF(0,0);


            _parent= parent;

            DrawTimer.initTimer();
            //描画のスロットリング
            DrawTimer.OnDraw = () =>
            {
                ForEachImage(img => img.addImgPos(pendingDelta));
                ForEachImage(img => img.drawImg());
                
            };

        }
        public void addImage(int mainWidth,int mainHeight,string path)
        {
            PictureBox tempPicture = new PictureBox();
            tempPicture.AllowDrop=true;

            tempPicture.DragDrop += TempPicture_DragDrop;
            tempPicture.DragEnter += TempPicture_DragEnter;





            _pictureBox.Add(tempPicture);
            Point size = getViewSize(mainWidth, mainHeight);
            Resize(mainWidth, mainHeight);

            tempPicture.MouseDown += (sender, e) => MouseDown(sender, e);
            tempPicture.MouseUp += (sender, e) => MouseUp(sender, e);

            tempPicture.MouseMove += (sender, e) => MouseMove(sender, e);
            tempPicture.DoubleClick += TempPicture_DoubleClick;
            tempPicture.MouseClick += TempPicture_MouseClick;

 
            tempPicture.HandleCreated += (s, e) =>
            {

                _loadImage.Add(new loadImage(tempPicture.Handle));
                int index = _pictureBox.IndexOf(tempPicture);

                //画像の読み込み
                bool hr = _loadImage[index].Initialize(mainWidth, mainHeight, path);


                //
                _parent.BeginInvoke(new Action(() =>
                {
                    int idx = _pictureBox.IndexOf(tempPicture);
                    if (idx < 0) return;

                    bool wasLast = (idx == _pictureBox.Count - 1);

                    if (!hr)
                    {
                        _loadImage[idx]?.Dispose();
                        _loadImage.RemoveAt(idx);
                        _pictureBox.RemoveAt(idx);

                        _parent.Controls.Remove(tempPicture);
                        tempPicture.Dispose();
                    }

                    // 削除前に「最後だった」なら実行
                    if (wasLast)
                    {
                        Resize(mainWidth, mainHeight);
                        drawImg();
                    }
                }));





            };


        }

        private void TempPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _parent.Close();
            }
        }

        private void TempPicture_DoubleClick(object sender, EventArgs e)
        {

            FormBorderStyle style = FormBorderStyle.None;
            FormWindowState windowState= FormWindowState.Maximized;

            if (_parent.WindowState == windowState)
            {
                 style = FormBorderStyle.Sizable;
                 windowState = FormWindowState.Normal;
            }


                _parent.WindowState = windowState;
            _parent.FormBorderStyle=style; ;

        }
        

        private void TempPicture_DragEnter(object sender, DragEventArgs e)
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

        private void TempPicture_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files) {
                Debug.Print(file);

                addImage(_parent.Width, _parent.Height, file);
                _parent.Controls.Add(this.getPictureBoxAt(Count() - 1));
            }

        }

        public  void Resize(int mainWidth, int mainHeight)
        {
            Point size = getViewSize(mainWidth, mainHeight);
            setPictureSize(size);

            setDisplaySize(size.X, size.Y);
            OnResize((uint)size.X, (uint)size.Y);
            FitImageToWindow();
        }

        private  void MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePos = e.Location;
            if (!isDragging || !DrawTimer.canRedraw())
            {


                return;
            }
            pendingDelta.X = e.X - lastMousePos.X;
            pendingDelta.Y = e.Y - lastMousePos.Y;

            lastMousePos = currentMousePos;

            DrawTimer.RequestRedraw();
        }

        private  void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !isDragging)
            {
                lastMousePos = e.Location;
                isDragging = true;
                DrawTimer.RequestRedraw();
                DrawTimer.start();
            }
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
            isDragging = false;
            DrawTimer.stop();
        }


        private void setPictureSize(Point size)
        {
            for (int index = 0; index < _pictureBox.Count; index++)
            {
                PictureBox pictureBox = _pictureBox[index];

                pictureBox.Width = size.X;
                pictureBox.Height = size.Y;

                int row = index / Column;
                int col = index % Column;

                pictureBox.Top = row * size.Y;
                pictureBox.Left = col * size.X;
            }
        }

        private Point getViewSize(int width, int height)
        {
            int count = _pictureBox.Count;
            int columns = Column;
            if (columns > count) 
                columns = count;


            if (count == 0)
                return new Point(width, height);

           

            int rows = (int)Math.Ceiling((double)count / columns);

            int w = width / columns;
            int h = height / rows;

            return new Point(w, h);
        }

        private void ForEachImage(Action<loadImage> action)
        {
            foreach (var img in _loadImage)
                action(img);
        }

        //描画先のサイズを変更
        private void setDisplaySize(float width,float height)
        {
            ForEachImage(img => img.setDisplaySize(width, height));


        }
        //d2d側の画面サイズ更新
        private void OnResize(uint width , uint height)
        {
            ForEachImage(img => img.OnResize(width,height));
        }
        private  void FitImageToWindow()
        {
            ForEachImage(img => img.FitImageToWindow());
        }

        public void AddScalingToPoint(float scale)
        {
            ForEachImage(img => img.AddScalingToPoint(currentMousePos, scale));
            DrawTimer.RequestRedraw();
            DrawTimer.start();
        }
        public void addImgPos(PointF p)
        {
            ForEachImage(img => img.addImgPos(p));
        }

        public void drawImg()
        {
              ForEachImage(img => img.drawImg());
  

        }

        ~ImageController()
        {
            ForEachImage(img => img.Dispose());
        }

        public PictureBox getPictureBoxAt(int i)
        {
            if (i < 0 || i >= _pictureBox.Count)
                return null;

            return _pictureBox[i];
        }
        public void Dispose()
        {
            Dispose(true);
          
            //デストラクタを呼ばせない
            GC.SuppressFinalize(this);
        }


        public  int GetPictureBoxIndex(PictureBox pic)
        {
   
            return _pictureBox.IndexOf(pic);
        }
        public int GetLoadImageIndex(loadImage loadImage)
        {
            return _loadImage.IndexOf(loadImage);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // loadImage を全部解放
                if (_loadImage != null)
                {
                    foreach (var loadImg in _loadImage)
                    {
                        loadImg?.Dispose();
                    }
                    _loadImage.Clear();
                    _loadImage = null;
                }

                foreach (var picture in _pictureBox)
                {

             
                    picture?.Dispose();
                }

                _pictureBox = null;
            }

            _disposed = true;
        }

        public int Count()
        {
            return _pictureBox.Count;
        }

    }
}

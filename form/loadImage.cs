using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static imageViewer.D2DRenderer;
using static imageViewer.ImageController;
using static System.Windows.Forms.AxHost;

namespace imageViewer
{
    class loadImage : IDisposable
    {
        private IntPtr hwnd;        // 描画先のウィンドウハンドル
        private IntPtr bitmapHandle; // DLLで生成されたビットマップのハンドル
        private  D2DRenderer.DrawParams _drawParams;
        protected D2DRenderer.D2D1_SIZE_U _ImgSize;

        private DisplayState _displayState;
        private int maxSize=80000;
        private int minSize = 1;
        private bool _disposed;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            // unmanaged resource 解放
            D2DRenderer.ShutdownRenderer(this.hwnd);

            _disposed = true;
        }

        public struct DisplayState
        {
            public float width;
            public float height;


        }


        public void FitImageToWindow()
        {
            D2DRenderer.D2D1_SIZE_U Imgsize = _ImgSize;
            float scale = Math.Min(_displayState.width / Imgsize.width, _displayState.height / Imgsize.height);

            setScale(scale);

            PointF currentImgSize = new PointF();
            PointF tempSize = new PointF();


            //センダリング
            currentImgSize = getCurrentSize();

            tempSize.X = _displayState.width / 2F - (currentImgSize.X / 2F);
            tempSize.Y = _displayState.height / 2F - (currentImgSize.Y / 2F);
            setImgPos(tempSize);




        }

        public loadImage (IntPtr hwnd)
        {
            this.hwnd = hwnd;
        }

        public bool Initialize(float width,float height,string path)
        {
            
            this.bitmapHandle = IntPtr.Zero;

            int hr = D2DRenderer.InitD2D(hwnd, path);
            if (hr != 0) return false;

            _drawParams = new D2DRenderer.DrawParams();
            _drawParams.posX = 0;
            _drawParams.posY = 0;
            _drawParams.scale = 1.0F;
            /*
            _drawParams.cx = 0;
            _drawParams.cy = 0;
            _drawParams.isScaling = false;
*/
            _ImgSize = getImgSize();


            _displayState = new DisplayState();

            setDisplaySize(width, height);

            return true;
        }
        public void drawImg()
        {

            D2DRenderer.OnPaintwithDirect(this.hwnd,_drawParams);


        }

        public  void OnResize(uint width,uint height)
        {
            D2DRenderer.OnResize(this.hwnd,width, height);


        }


        public void addImgPos(PointF p)
        {
            _drawParams.posX += p.X;
            _drawParams.posY += p.Y;
        }


        protected void setImgPos(PointF p)
        {
            _drawParams.posX = p.X;
            _drawParams.posY = p.Y;
        }

        public PointF getImgPos()
        {
            return new PointF(_drawParams.posX, _drawParams.posY);
        }

        protected void setScale(float s)
        {
            _drawParams.scale = s;
        }


        private D2DRenderer.D2D1_SIZE_U getImgSize()
        {
            return D2DRenderer.getPixelSize(this.hwnd);
        }

        protected PointF getCurrentSize()
        {
            PointF p = new PointF();
            p.X = (_ImgSize.width * _drawParams.scale);
            p.Y = (_ImgSize.height * _drawParams.scale);
            return p;
        }
        private bool canScalingSize(float scale)
        {
            PointF temp= getCurrentSize();
            if(_ImgSize.width * scale > maxSize || _ImgSize.height * scale > maxSize)
            {
                return false;
            }
            if(_ImgSize.width * scale < minSize || _ImgSize.height * scale < minSize)
            {

            }

            return true;
        }


        public void AddScalingToPoint(PointF p, float scale)
        {

            if (scale > 1F && !canScalingSize(scale))
            {
                return;
            }

            float diffX = (p.X- _drawParams.posX) * scale - (p.X - _drawParams.posX);
            float diffY = (p.Y - _drawParams.posY) * scale - (p.Y - _drawParams.posY);


            //  _drawParams.posX -= p.X * (scale - 1);
            _drawParams.posY -= diffY;
            _drawParams.posX -= diffX;

            _drawParams.scale *= scale;


            

        }
        public void setDisplaySize(float width, float height)
        {
            _displayState.width = width;
            _displayState.height = height;
        }
        public PointF getDisplaySize()
        {
            return new PointF(_displayState.width, _displayState.height);
        }

        ~loadImage()
        {
            D2DRenderer.ShutdownRenderer(this.hwnd);
        }

    }
}

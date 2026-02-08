using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace imageViewer
{
    internal class D2DRenderer
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct DrawParams
        {
            public float scale;
            public float posX;
            public float posY;
            /*
            public float cx;
            public float cy;
            public bool isScaling;
        */
            }


        [StructLayout(LayoutKind.Sequential)]
        public struct D2D1_SIZE_U
        {
            public uint width;
            public uint height;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct D2D1_POINT_2F
        {
            public float x;
            public float y;
        }

        //画像の読み込みを実施　これは一回だけで良い

        [DllImport("imagelodar2.dll", CharSet = CharSet.Unicode)]
        public static extern int InitD2D(IntPtr hwnd, string path);

        [DllImport("imagelodar2.dll")]
        public static extern void  OnPaintwithDirect(IntPtr hwnd,DrawParams drawParams);

        [DllImport("imagelodar2.dll")]
        public static extern D2D1_SIZE_U getPixelSize(IntPtr hwnd);

        [DllImport("imagelodar2.dll")]
        public static extern void  ShutdownRenderer(IntPtr hwnd);

        [DllImport("imagelodar2.dll")]
        public static extern void OnResize(IntPtr hwnd, uint width,uint height);



    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace imageViewer
{
    internal class DrawTimer
    {
        private  static System.Windows.Forms.Timer drawTimer;
        private  static  bool needsRedraw = false;

        public  static Action OnDraw;
        public static  void initTimer()
        {
            drawTimer = new System.Windows.Forms.Timer();
            drawTimer.Interval = 12;


            drawTimer.Tick += (s, e2) =>
            {
                if (!needsRedraw) return;

                OnDraw?.Invoke();  
                needsRedraw = false;


            };

        }

        public static bool canRedraw()
        {
            return !needsRedraw;
        }

        public static void RequestRedraw()
        {
            needsRedraw = true;
        }
        public static void start()
        {
            drawTimer.Start();
        }

        public static void stop() { drawTimer.Stop(); }

        
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.UI
{
    public class BitmapWrap
    {

        public static BitmapWrap FromBitmap(Bitmap bmp)
        {
            BitmapWrap ret = new BitmapWrap(bmp);
            return ret;
        }


        public Image image;
        public object Tag;
        public object locker = new object();
        bool isDisposed = false;
        bool isMarkedDisposed = false;       
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }
        public bool IsMarkedDisposed
        {
            get
            {
                return isMarkedDisposed;
            }
            
        }

        public void Dispose()
        {
            isMarkedDisposed = true;
        }
        public BitmapWrap(Image b)
        {
            this.image = b;
        }
        public void DoDispose()
        {
            if (IsDisposed) return;
            if (image == null) return;
            image.Dispose();
            image = null;
            isDisposed = true;
        }


        public static implicit operator Image(BitmapWrap d) => d.image;
        public static implicit operator BitmapWrap(Image b) => new BitmapWrap(b);
        public Size Size
        {
            get
            {
                if (image == null) return new Size(0, 0);
                return image.Size;
            }
        }
        public int Width
        {
            get
            {
                return image.Width;
            }
        }
        public int Height
        {
            get
            {
                return image.Height;
            }
        }
        public void Save(String filename)
        {
            image.Save(filename);
        }
        
    }
}

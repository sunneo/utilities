using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Utilities
{
    public partial class ScreenCapturer : UserControl
    {
        public event EventHandler<List<SubBlockInfo>> BlockChanged;
        /// <summary>
        /// Render size mode enum
        /// </summary>
        public enum RendererSizeMode
        {
            /// <summary>
            /// use original size
            /// </summary>
            None,
            /// <summary>
            /// stretch to target 
            /// </summary>
            Stretch,
            /// <summary>
            /// keep ratio
            /// </summary>
            KeepRatio,
            /// <summary>
            /// resize to fit but clip it.
            /// </summary>
            Zoom
        }
        public Bitmap CurrentScreenImage;

        public class Profile
        {
            public volatile int CompressionLevel = 100;
            public volatile bool EnableCompression = true;
            public RendererSizeMode SizeMode = RendererSizeMode.KeepRatio;
            public static Profile None
            {
                get
                {
                    return new Profile()
                    {
                        CompressionLevel=100,
                        EnableCompression=false,
                        SizeMode= RendererSizeMode.None
                    };
                }
            }
            public static Profile Default
            {
                get
                {
                    return new Profile()
                    {
                        CompressionLevel = 100,
                        EnableCompression = true,
                        SizeMode = RendererSizeMode.KeepRatio
                    };
                }
            }
            public static Profile Normal
            {
                get
                {
                    return new Profile()
                    {
                        CompressionLevel = 75,
                        EnableCompression = true,
                        SizeMode = RendererSizeMode.KeepRatio
                    };
                }
            }
            public static Profile NormalOrigSize
            {
                get
                {
                    return new Profile()
                    {
                        CompressionLevel = 75,
                        EnableCompression = true,
                        SizeMode = RendererSizeMode.None
                    };
                }
            }
            public static Profile NormalStretch
            {
                get
                {
                    return new Profile()
                    {
                        CompressionLevel = 75,
                        EnableCompression = true,
                        SizeMode = RendererSizeMode.Stretch
                    };
                }
            }
        }
        public Profile mCurrentProfile = Profile.Default;
        public Profile CurrentProfile
        {
            get
            {
                return mCurrentProfile;
            }
            set
            {
                mCurrentProfile = value;
                mSizeDimensionGenerated = false;
            }
        }
        /// <summary>
        /// get resized size according to mode
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="dim1"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Size GetSizeByMode(RendererSizeMode mode, Size dim1, Size target)
        {
            if (mode == RendererSizeMode.None) return dim1;
            else if (mode == RendererSizeMode.Stretch) return target;
            else
            {
                double ratio1 = (double)dim1.Width / target.Width;
                double ratio2 = (double)dim1.Height / target.Height;
                double finalRatio = 1.0;
                if (mode == RendererSizeMode.KeepRatio)
                {
                    finalRatio = Math.Max(ratio1, ratio2);
                }
                else if (mode == RendererSizeMode.Zoom)
                {
                    finalRatio = Math.Min(ratio1, ratio2);
                }
                return new Size((int)(dim1.Width / finalRatio), (int)(dim1.Height / finalRatio));
            }
        }
        /// <summary>
        /// resized size.
        /// </summary>
        public Size SizeDimension
        {
            get
            {
                if (!mSizeDimensionGenerated)
                {
                    mSizeDimension = GetSizeByMode(SizeMode, Screen.PrimaryScreen.Bounds.Size, this.Size);
                    mSizeDimensionGenerated = true;
                }
                return mSizeDimension;
            }
        }
        public RendererSizeMode SizeMode
        {
            get
            {
                return CurrentProfile.SizeMode;
            }
            set
            {
                CurrentProfile.SizeMode = value;
                mSizeDimensionGenerated = false;
            }
        }
        /// <summary>
        /// get resized bitmap
        /// </summary>
        /// <returns></returns>
        public Bitmap GetResizedBitmap()
        {
            return new Bitmap(CurrentScreenImage, SizeDimension);
        }
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;
        public static Bitmap CaptureScreen(Bitmap result,bool CaptureMouse)
        {
            if(result == null)
                result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                    g.FillRectangle(Brushes.White, new Rectangle(new Point(Cursor.Position.X + 1, Cursor.Position.Y + 1), new Size(2, 2)));
                    g.FillRectangle(Brushes.Black, new Rectangle(Cursor.Position, new Size(2, 2)));
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
        public static Bitmap CaptureScreen(bool CaptureMouse)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
#if false
                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                IntPtr hdc = g.GetHdc();
                                DrawIcon(hdc, pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
#else
                    g.FillRectangle(Brushes.White, new Rectangle(new Point(Cursor.Position.X+1,Cursor.Position.Y+1), new Size(2, 2)));
                    g.FillRectangle(Brushes.Black, new Rectangle(Cursor.Position, new Size(2, 2)));
#endif
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }
        public ScreenCapturer()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
#region private fields
        Timer timer = new Timer();
        Size mSizeDimension;
        volatile bool mSizeDimensionGenerated = false;
 #endregion
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            timer.Interval = 16;
            timer.Tick += timer_Tick;
            timer.Start();
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            timer.Stop();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            CaptureScreenWorker();
            this.Invalidate();
        }
       
        
       
       
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            mSizeDimensionGenerated = false;
        }
       
        // just paint it.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (DesignMode) return;
            try
            {
                if (CurrentScreenImage != null && !CurrentScreenImage.Size.IsEmpty)
                {
                    e.Graphics.DrawImage(CurrentScreenImage, this.Width / 2 - SizeDimension.Width / 2, this.Height / 2 - SizeDimension.Height / 2, SizeDimension.Width, SizeDimension.Height);
                }
            }
            catch (Exception ee)
            {

            }
        }
        private ImageCodecInfo JpegEncoder
        {
            get
            {
                if (_jgpEncoder == null)
                {
                    _jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                }
                return _jgpEncoder;
            }
        }
        ImageCodecInfo _jgpEncoder;
        MemoryStream CurrentCompressMemoryStream = new MemoryStream();
        private Bitmap VaryQualityLevel(Bitmap bmp1, int level)
        {
            if (bmp1 == null) return null;
            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, level);
            myEncoderParameters.Param[0] = myEncoderParameter;
            MemoryStream memoryStream = CurrentCompressMemoryStream;
            memoryStream.Position = 0;
            bmp1.Save(memoryStream, JpegEncoder, myEncoderParameters);
            memoryStream.Position = 0;
            return (Bitmap)Bitmap.FromStream(memoryStream);
        }
        private String VaryQualityLevelBase64(Bitmap bmp1, int level)
        {
            if (bmp1 == null) return null;
            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, level);
            myEncoderParameters.Param[0] = myEncoderParameter;
            MemoryStream memoryStream = CurrentCompressMemoryStream;
            memoryStream.Position = 0;
            bmp1.Save(memoryStream, JpegEncoder, myEncoderParameters);
            memoryStream.Position = 0;
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        Bitmap Canvas = null;
        public volatile bool AlwaysConvertBase64 = false;
        public volatile bool UseBlockDiff = true;
        public String Base64Content;
        public Size SplitParts = new Size(4,4);
        Dictionary<int, SubBlockInfo> SubBlocks = new Dictionary<int, SubBlockInfo>();
        public class SubBlockInfo:IDisposable
        {

            public int PartId;
            public int Left;
            public int Top;
            public int Width;
            public int Height;
            public Bitmap BMP;
            public String CompressedString;
            public String FormatData()
            {
                if (CompressedString == null) CompressedString = "";
                return String.Format("{0},{1},{2},{3};", Left, Top, Width, Height + CompressedString);
            }


            ~SubBlockInfo()
            {
                if (BMP != null)
                {
                    BMP.Dispose();
                    BMP = null;
                }
                CompressedString = null;
            }
        }

        private List<SubBlockInfo> GenerateUpdatedRegion(Bitmap bmp)
        {
            List<SubBlockInfo> ret = new List<SubBlockInfo>();
            Size splits = SplitParts;
            int PartPerWidth = bmp.Width / splits.Width;
            int PartPerHeight = bmp.Height / splits.Height;
            unsafe
            {
                BitmapData data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                int* IntScan = (int*)data.Scan0;
                for (int i = 0; i < splits.Height; ++i)
                {
                    int y = i * PartPerHeight;
                    int h = PartPerHeight;
                    if (i == splits.Height - 1)
                    {
                        if (y + h > bmp.Height)
                        {
                            h = bmp.Height - y;
                        }
                    }
                    for (int j = 0; j < splits.Width; ++j)
                    {
                        int partId = i * splits.Height + j;
                        int x = j * PartPerWidth;
                        int w = PartPerWidth;
                        if (j == splits.Width - 1)
                        {
                            if (x + w > bmp.Width)
                            {
                                w = bmp.Width - x;
                            }
                        }
                        
                        SubBlockInfo subinfo = null;
                        int len  = w * h;
                        if (!SubBlocks.ContainsKey(partId))
                        {
                            subinfo = new SubBlockInfo();
                            
                            subinfo.PartId = partId;
                            subinfo.Left = x;
                            subinfo.Top = y;
                            subinfo.Width = w;
                            subinfo.Height = h;
                            subinfo.BMP = new Bitmap(w, h);
                            BitmapData subinfoRawData = subinfo.BMP.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bmp.PixelFormat);
                            int* RawData = (int*)subinfoRawData.Scan0;
                            for (int r = 0; r < h; ++r)
                            {
                                for (int c = 0; c < w; ++c)
                                {
                                    int idxTarget = r * h + c;
                                    int idxSrc = (r + i) * data.Width + (j + c);
                                    RawData[idxTarget] = IntScan[idxSrc];
                                }
                            }
                            subinfo.BMP.UnlockBits(subinfoRawData);
                            SubBlocks[partId] = subinfo;
                            ret.Add(subinfo);
                            subinfo.CompressedString = VaryQualityLevelBase64(subinfo.BMP, CurrentProfile.CompressionLevel);
                        }
                        else
                        {
                            
                            SubBlockInfo that = SubBlocks[partId];
                            BitmapData subinfoRawData = subinfo.BMP.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, bmp.PixelFormat);
                            int* RawData = (int*)subinfoRawData.Scan0;
                            bool changed = false;
                            for (int r = 0; r < h; ++r)
                            {
                                for (int c = 0; c < w ; ++c)
                                {
                                    int idxTarget = r * h + c;
                                    int idxSrc = (r + i) * data.Width + (j + c);
                                    if (!changed)
                                    {
                                        if (RawData[idxTarget] !=IntScan[idxSrc])
                                        {
                                            changed = true;
                                        }
                                    }
                                    RawData[idxTarget] = IntScan[idxSrc];
                                }
                            }
                            subinfo.BMP.UnlockBits(subinfoRawData);
                            if (changed)
                            {
                                subinfo.CompressedString = VaryQualityLevelBase64(subinfo.BMP, CurrentProfile.CompressionLevel);
                                ret.Add(subinfo);
                            }
                            
                        }
                    }
                }
                bmp.UnlockBits(data);
                
            }
            return ret;
        }
        private void CaptureScreenWorker()
        {
            Bitmap memoryImage;
#if false
            memoryImage = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Size s = new Size(memoryImage.Width, memoryImage.Height);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            memoryGraphics.CopyFromScreen(0, 0, 0, 0, s);
            Cursor.Current.Draw(memoryGraphics, new Rectangle(Cursor.Position, Cursor.Current.Size));
            memoryGraphics.Dispose();
#else
            if (Canvas == null)
            {
                Canvas = new Bitmap(Screen.PrimaryScreen.Bounds.Width,Screen.PrimaryScreen.Bounds.Height);
            }
            memoryImage = CaptureScreen(Canvas,true);
#endif
            if (CurrentProfile.EnableCompression)
            {

                if (UseBlockDiff && BlockChanged != null)
                {
                    BlockChanged(this, GenerateUpdatedRegion(memoryImage));
                }
                else
                {
                    CurrentScreenImage = VaryQualityLevel(memoryImage, CurrentProfile.CompressionLevel);
                }
            }
            else
            {
                if (UseBlockDiff && BlockChanged != null)
                {
                    BlockChanged(this, GenerateUpdatedRegion(memoryImage));
                }
                else
                {
                    CurrentScreenImage = VaryQualityLevel(memoryImage, CurrentProfile.CompressionLevel);
                }
            }
            
            if (!UseBlockDiff &&  AlwaysConvertBase64)
            {
                this.Base64Content=  Convert.ToBase64String(this.CurrentCompressMemoryStream.ToArray());
            }
        }
    }
}

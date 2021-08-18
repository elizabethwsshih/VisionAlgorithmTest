using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;


using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;


namespace CrossDetectTest1
{

    public partial class Form1 : Form
    {

        // 引用Windows GDI元件，並調用DeleteObject的方法
        //[System.Runtime.InteropServices.DllImport("gdi32.dll")]
        //public static extern bool DeleteObject(IntPtr hObject);
        BitmapDataFast BMPFast = new BitmapDataFast();

        List<IntPoint> EdgeP;
        List<PointF> GoodMapList;
        List<System.Drawing.Point> CrossP;
        Bitmap OriBitmap, OriBitmap2, ProcessBitmap, BlobBitmap, ErosionBitmap;
        int[,] bitmapOverlap, bitmapFilter;
        int w, h;

        int[, ,] OriBitmapArr;
        int[, ,] OriBitmapArr2;

        int[, ,] BmpArr;
        string ImgFileName;
        List<CrossPoint> LineCrossP;
        List<LineSeq> AllLineMap;

        List<PointF> OverlapCenterP;

        List<PointF> CircleCenterP;
        List<_RankCirclePoint> RankCirclePointList;

        PointF[] MapGrid;//mapping網格陣列
        PointF[,] MapGrid2;//525 mapping網格陣列


        List<int> GoodWeldIdxList;//完整銲道list(只存 mapping 網格上的編號)
        List<PointF> GoodWeldCircleP;
        int[, ,] TemplateColor;
        List<System.Drawing.Point> FinalDefectPList = new List<System.Drawing.Point>();

        List<WeldVisionList> WeldVisionTotalList = new List<WeldVisionList>();

        List<PointF> WeldCircleP;
        List<PointF> BlackCircleP;

        Bitmap SaveBMP;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {


                ImgFileName = openFileDialog1.FileName;
                Bitmap FileBitmap = new Bitmap(ImgFileName);

                int PF = -1;
                PixelFormat[] pixelFormatArray = {
                                            PixelFormat.Format1bppIndexed
                                            ,PixelFormat.Format4bppIndexed
                                            ,PixelFormat.Format8bppIndexed
                                            ,PixelFormat.Undefined
                                            ,PixelFormat.DontCare
                                            ,PixelFormat.Format16bppArgb1555
                                            ,PixelFormat.Format16bppGrayScale
                                        };
                foreach (PixelFormat pf in pixelFormatArray)
                {
                    if (FileBitmap.PixelFormat == pf)
                    {
                        PF = 1;
                        break;
                    }

                    else PF = 0;
                }

                if (PF == 1)
                {
                    OriBitmap = new Bitmap(FileBitmap.Width, FileBitmap.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(OriBitmap))
                    {
                        g.DrawImage(FileBitmap, 0, 0);
                    }
                }

                else OriBitmap = new Bitmap(FileBitmap);

                Console.WriteLine("W=" + OriBitmap.Width);
                Console.WriteLine("H=" + OriBitmap.Height);





                //取得灰階影像
                Image<Gray, byte> grayImage = new Image<Gray, byte>(OriBitmap);
                OriBitmap = grayImage.ToBitmap();


                OriBitmapArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
                OriBitmapArr = BMPFast.GetRGBData(OriBitmap);


            }
            pictureBox1.Image = OriBitmap;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();//引用stopwatch物件

            //clock.Reset();//碼表歸零

            //clock.Start();//碼表開始計時



            //EdgeP = new List<IntPoint>();

            //BlobCounter blobCounter = new BlobCounter();
            //Bitmap bb = new Bitmap(b);
            //Graphics g = Graphics.FromImage(bb);

            //Pen RedPen = new Pen(Color.Red, 3);

            //BitmapData bitmapData = b.LockBits(
            //    new Rectangle(0, 0, b.Width, b.Height),
            //    ImageLockMode.ReadWrite, b.PixelFormat);


            //blobCounter.ProcessImage(bitmapData);
            //Blob[] blobs = blobCounter.GetObjectsInformation();


            //for (int i = 0, n = blobs.Length; i < n; i++)
            //{
            //    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
            //    List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

            //    foreach (IntPoint p in corners)
            //    {
            //        //g.DrawEllipse(RedPen, p.X, p.Y, 5, 5);

            //        //if (BmpArr[p.X, p.Y, 0] == 0
            //        //&& BmpArr[p.X, p.Y, 1] == 0
            //        //&& BmpArr[p.X, p.Y, 2] == 0
            //        //&& BmpArr[p.X - 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y, 0] == 0
            //        //&& BmpArr[p.X, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X - 1, p.Y + 1, 0] == 0
            //        // && BmpArr[p.X + 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y - 1, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y + 1, 0] == 0
            //        //&& BmpArr[p.X + 1, p.Y + 1, 0] == 0
            //        //)
            //        //{ 
            //        g.DrawEllipse(RedPen, p.X, p.Y, 5, 5);
            //        EdgeP.Add(p);
            //        //}
            //    }
            //}


            //b.UnlockBits(bitmapData);


            //pictureBox1.Image = bb;

            //RedPen.Dispose();
            //g.Dispose();


            //clock.Stop();//碼錶停止

            ////印出所花費的總豪秒數

            //string result1 = clock.Elapsed.TotalMilliseconds.ToString();
            //Console.WriteLine("result1=" + result1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Bitmap bb = new Bitmap(pictureBox1.Image);
            //Graphics g = Graphics.FromImage(bb);
            //Pen RedPen = new Pen(Color.Red, 3);

            //double dist;
            //List<RankDistList> RankDist = new List<RankDistList>();

            //System.Drawing.Point LineP1, LineP2;
            ////找前三名最短
            //foreach (System.Drawing.Point p1 in CrossP)
            //{


            //    foreach (System.Drawing.Point p2 in CrossP)
            //        foreach (System.Drawing.Point p2 in CrossP)
            //            foreach (System.Drawing.Point p2 in CrossP)
            //    {
            //        if (p1.X != p2.X && p1.Y != p2.Y)
            //        {
            //            dist = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            //            RankDistList rankP = new RankDistList(dist, p1, p2);
            //            RankDist.Add(rankP);

            //        }
            //    }
            //    // g.DrawLine(RedPen, LineP1, LineP2);
            //    RankDist.Sort();

            //    //foreach (RankDistList rankP in RankDist)
            //    //{
            //    //    Console.WriteLine(p1 + ":" + rankP.Dist + "," + rankP.P1 + "," + rankP.P2);
            //    //}
            //}




            //HoughLineTransformation lineTransform = new HoughLineTransformation();


            ////GrayscaleBT709 greyScaleFilter = new GrayscaleBT709();
            ////Bitmap newBmp = greyScaleFilter.Apply((Bitmap)bb.Clone());


            //// apply Hough line transofrm
            //lineTransform.ProcessImage(newBmp);
            //Bitmap houghLineImage = lineTransform.ToBitmap();
            //// get lines using relative intensity
            //HoughLine[] lines = lineTransform.GetLinesByRelativeIntensity(0.1);

            //foreach (HoughLine line in lines)
            //{
            //    // get line's radius and theta values
            //    int r = line.Radius;
            //    double t = line.Theta;

            //    // check if line is in lower part of the image
            //    if (r < 0)
            //    {
            //        t += 180;
            //        r = -r;
            //    }

            //    // convert degrees to radians
            //    t = (t / 180) * Math.PI;

            //    // get image centers (all coordinate are measured relative
            //    // to center)
            //    int w2 = bb.Width / 2;
            //    int h2 = bb.Height / 2;

            //    double x0 = 0, x1 = 0, y0 = 0, y1 = 0;

            //    if (line.Theta != 0)
            //    {
            //        // none-vertical line
            //        x0 = -w2; // most left point
            //        x1 = w2;  // most right point

            //        // calculate corresponding y values
            //        y0 = (-Math.Cos(t) * x0 + r) / Math.Sin(t);
            //        y1 = (-Math.Cos(t) * x1 + r) / Math.Sin(t);
            //    }
            //    else
            //    {
            //        // vertical line
            //        x0 = line.Radius;
            //        x1 = line.Radius;

            //        y0 = h2;
            //        y1 = -h2;
            //    }

            //    // draw line on the image
            //    PointF p1 = new PointF();
            //    PointF p2 = new PointF();
            //    p1.X = (float)(x0 + w2);
            //    p1.Y = (float)(h2 - y0);


            //    p2.X = (float)(x1 + w2);
            //    p2.Y = (float)(h2 - y1);
            //    g.DrawLine(RedPen, p1, p2);
            //}

            //  pictureBox1.Image = bb;
        }

        private void button4_Click(object sender, EventArgs e)
        {

            //System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();//引用stopwatch物件

            //clock.Reset();//碼表歸零

            //clock.Start();//碼表開始計時



            //List<System.Drawing.Point> TmpMidP = new List<System.Drawing.Point>();
            //List<System.Drawing.Point> MidSideP = new List<System.Drawing.Point>();
            //List<System.Drawing.Point> RealMidP = new List<System.Drawing.Point>();
            //CrossP = new List<System.Drawing.Point>();
            //Bitmap bb = new Bitmap(b);
            //Graphics g = Graphics.FromImage(bb);
            //Pen YellowPen = new Pen(Color.Yellow, 3);
            //Pen RedPen = new Pen(Color.Red, 5);



            ////找與最短距離的點之中點
            //double ShortDist, dist;
            //System.Drawing.Point MidP;
            //foreach (IntPoint p1 in EdgeP)
            //{
            //    ShortDist = 100000000;
            //    MidP = new System.Drawing.Point();
            //    foreach (IntPoint p2 in EdgeP)
            //    {

            //        if (p1.X != p2.X && p1.Y != p2.Y)
            //        {
            //            dist = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
            //            if (dist < ShortDist)
            //            {
            //                ShortDist = dist;

            //                MidP.X = (p1.X + p2.X) / 2;
            //                MidP.Y = (p1.Y + p2.Y) / 2;

            //            }
            //        }
            //    }

            //    if (BmpArr[MidP.X, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y, 1] == 0
            //        && BmpArr[MidP.X, MidP.Y, 2] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X - 1, MidP.Y + 1, 0] == 0
            //         && BmpArr[MidP.X + 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y - 1, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y + 1, 0] == 0
            //        && BmpArr[MidP.X + 1, MidP.Y + 1, 0] == 0
            //        )

            //        TmpMidP.Add(MidP);

            //}


            ////System.Drawing.Point pp1 = new System.Drawing.Point();
            ////System.Drawing.Point pp2 = new System.Drawing.Point();

            ////pp1.X = 65;
            ////pp1.Y = 118;

            ////pp2.X = 33;
            ////pp2.Y = 118;

            ////g.DrawLine(RedPen, pp1, pp2);

            ////只選周圍距離四角形角點近者
            //foreach (System.Drawing.Point p3 in TmpMidP)
            //{

            //    foreach (IntPoint p4 in EdgeP)
            //    {
            //        dist = (p3.X - p4.X) * (p3.X - p4.X) + (p3.Y - p4.Y) * (p3.Y - p4.Y);
            //        if (dist < 1000)  //距離依照線寬而定
            //        {
            //            MidSideP.Add(p3);
            //            break;
            //        }

            //    }


            //}
            //foreach (System.Drawing.Point p3 in TmpMidP)
            //{
            //    g.DrawEllipse(RedPen, p3.X, p3.Y, 5, 5);
            //}

            ////找中點
            //foreach (System.Drawing.Point p5 in MidSideP)
            //{
            //    ShortDist = 100000000;
            //    MidP = new System.Drawing.Point();
            //    foreach (System.Drawing.Point p6 in MidSideP)
            //    {

            //        if (p5.X != p6.X || p5.Y != p6.Y)
            //        {
            //            dist = Math.Sqrt((p5.X - p6.X) * (p5.X - p6.X) + (p5.Y - p6.Y) * (p5.Y - p6.Y));
            //            if (dist < ShortDist)
            //            {
            //                ShortDist = dist;
            //                MidP.X = (p5.X + p6.X) / 2;
            //                MidP.Y = (p5.Y + p6.Y) / 2;

            //            }
            //        }

            //    }
            //    g.DrawEllipse(YellowPen, MidP.X, MidP.Y, 3, 3);
            //    CrossP.Add(MidP);
            //}


            //pictureBox1.Image = bb;


            //clock.Stop();//碼錶停止

            ////印出所花費的總豪秒數

            //string result2 = clock.Elapsed.TotalMilliseconds.ToString();
            //Console.WriteLine("result2=" + result2);



        }


        //高效率用指標讀取影像資料
        public static int[, ,] GetRGBData(Bitmap bitImg)
        {
            int height = bitImg.Height;
            int width = bitImg.Width;
            //鎖住Bitmap整個影像內容
            BitmapData bitmapData = bitImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            //取得影像資料的起始位置
            IntPtr imgPtr = bitmapData.Scan0;
            //影像scan的寬度
            int stride = bitmapData.Stride;
            //影像陣列的實際寬度
            int widthByte = width * 3;
            //所Padding的Byte數
            int skipByte = stride - widthByte;
            //設定預定存放的rgb三維陣列
            int[, ,] rgbData = new int[width, height, 3];

            #region 讀取RGB資料
            //注意C#的GDI+內的影像資料順序為BGR, 非一般熟悉的順序RGB
            //因此我們把順序調回原來的陣列順序排放BGR->RGB
            unsafe
            {
                byte* p = (byte*)(void*)imgPtr;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        //B Channel
                        rgbData[i, j, 2] = p[0];
                        p++;
                        //G Channel
                        rgbData[i, j, 1] = p[0];
                        p++;
                        //B Channel
                        rgbData[i, j, 0] = p[0];
                        p++;
                    }
                    p += skipByte;
                }
            }

            //解開記憶體鎖
            bitImg.UnlockBits(bitmapData);

            #endregion

            return rgbData;
        }





        public class RankDistList : IComparable
        {
            public double Dist;
            public System.Drawing.Point P1;
            public System.Drawing.Point P2;

            public RankDistList(double dist, System.Drawing.Point p1, System.Drawing.Point p2)
            {
                this.Dist = dist;
                this.P1 = p1;
                this.P2 = p2;
            }

            public int CompareTo(object obj)
            {
                RankDistList tobeCompared = (RankDistList)obj;
                if (Dist > tobeCompared.Dist)
                    return 1;
                else if (Dist == tobeCompared.Dist)
                    return 0;
                else return -1;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            //Grayscale grayscaleFilter = new Grayscale(0.299, 0.587, 0.114);
            //bitmapGrayscale = grayscaleFilter.Apply(b);
            //pictureBox1.Image = bitmapGrayscale;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // create filter

            //OtsuThreshold filter = new OtsuThreshold();
            //// apply the filter
            //filter.ApplyInPlace(bitmapGrayscale);
            //pictureBox1.Image = bitmapGrayscale;
        }

        private void button7_Click(object sender, EventArgs e)
        {

            //BlobCounter blobCounter = new BlobCounter();
            //Bitmap bb = new Bitmap(bitmapGrayscale);
            //Graphics g = Graphics.FromImage(bb);

            //Pen RedPen = new Pen(Color.Red, 3);




            //// create filter
            //BlobsFiltering filter = new BlobsFiltering();
            //// configure filter
            //filter.CoupledSizeFiltering = true;
            //filter.MinWidth = 15;
            //filter.MinHeight = 15;
            //// apply the filter
            //filter.ApplyInPlace(bitmapGrayscale);

            //BitmapData bitmapData = bitmapGrayscale.LockBits(
            //    new Rectangle(0, 0, bitmapGrayscale.Width, bitmapGrayscale.Height),
            //    ImageLockMode.ReadWrite, bitmapGrayscale.PixelFormat);


            //blobCounter.ProcessImage(bitmapData);
            //Blob[] blobs = blobCounter.GetObjectsInformation();
            //bitmapGrayscale.UnlockBits(bitmapData);


            //int MidX = 0, MidY = 0;
            ////計算中心
            //foreach (Blob WhiteBlob in blobs)
            //{
            //    MidX = 0;
            //    MidY = 0;
            //    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(WhiteBlob);

            //    foreach (IntPoint p in edgePoints)
            //    {
            //        MidX += p.X;
            //        MidY += p.Y;
            //    }

            //    MidX = MidX / edgePoints.Count();
            //    MidY = MidY / edgePoints.Count();
            //    g.DrawEllipse(RedPen, MidX, MidY, 2, 2);
            //}

            //pictureBox1.Image = bb;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ////-----------------------1.開檔& gray level-----------------------
            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "All Files|*.*";
            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{

            //    ImgFileName = openFileDialog1.FileName;

            //    b = new Bitmap(ImgFileName);
            //    this.pictureBox1.Image = b;

            //    BmpArr = new int[b.Width, b.Height, 3];
            //    BmpArr = GetRGBData(b);


            //}

            //System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();//引用stopwatch物件

            //clock.Reset();//碼表歸零

            //clock.Start();//碼表開始計時

            ////20200901的影像是灰階不用轉

            ////Grayscale grayscaleFilter = new Grayscale(0.299, 0.587, 0.114);
            ////bitmapGrayscale = grayscaleFilter.Apply(b);
            //bitmapGrayscale = b;

            ////-----------------------2. threshold-----------------------
            //OtsuThreshold filter = new OtsuThreshold();
            //// apply the filter

            //// create filter
            ////Threshold filter = new Threshold(180);
            //// apply the filter


            //filter.ApplyInPlace(bitmapGrayscale);


            ////-----------------------3. 找出網格交點-----------------------
            //LineCrossP = new List<CrossPoint>();


            //BlobCounter blobCounter = new BlobCounter();
            //Bitmap bb = new Bitmap(bitmapGrayscale);
            //Graphics g = Graphics.FromImage(bb);

            //Pen RedPen = new Pen(Color.Red, 3);



            //// create filter
            //BlobsFiltering filter2 = new BlobsFiltering();
            //// configure filter
            //filter2.CoupledSizeFiltering = true;
            //filter2.MinWidth = 15;
            //filter2.MinHeight = 15;
            //// apply the filter
            //filter2.ApplyInPlace(bitmapGrayscale);

            //BitmapData bitmapData = bitmapGrayscale.LockBits(
            //    new Rectangle(0, 0, bitmapGrayscale.Width, bitmapGrayscale.Height),
            //    ImageLockMode.ReadWrite, bitmapGrayscale.PixelFormat);


            //blobCounter.ProcessImage(bitmapData);
            //Blob[] blobs = blobCounter.GetObjectsInformation();
            //bitmapGrayscale.UnlockBits(bitmapData);


            //int MidX = 0, MidY = 0;
            //CrossPoint tmpP;
            ////計算中心
            //foreach (Blob WhiteBlob in blobs)
            //{
            //    MidX = 0;
            //    MidY = 0;
            //    tmpP = new CrossPoint();
            //    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(WhiteBlob);

            //    foreach (IntPoint p in edgePoints)
            //    {
            //        MidX += p.X;
            //        MidY += p.Y;
            //    }

            //    MidX = MidX / edgePoints.Count();
            //    MidY = MidY / edgePoints.Count();
            //    g.DrawEllipse(RedPen, MidX, MidY, 2, 2);
            //    tmpP.Label = false;
            //    tmpP.CrossP.X = MidX;
            //    tmpP.CrossP.Y = MidY;
            //    LineCrossP.Add(tmpP);
            //}

            //pictureBox1.Image = bb;
            //string result1 = clock.Elapsed.TotalMilliseconds.ToString();
            //Console.WriteLine("result1=" + result1);


        }

        private void button9_Click(object sender, EventArgs e)
        {
            //LineSeq LineSeq = new LineSeq();
            //LineSeq.CrossP = new List<PointF>();
            //PointF tmpP;


            //for (int i = 0; i < LineCrossP.Count(); i++)
            //{
            //    LineCrossP[i].Label = true;
            //    tmpP = new PointF();
            //    LineSeq.CrossP = new List<PointF>();

            //    tmpP.X = LineCrossP[i].CrossP.X;
            //    tmpP.Y = LineCrossP[i].CrossP.Y;
            //    LineSeq.CrossP.Add(tmpP);

            //    tmpP = new PointF();
            //    for (int j = 0; j < LineCrossP.Count(); j++)
            //    {
            //        if (LineCrossP[j].Label == false
            //            && Math.Abs(LineCrossP[i].CrossP.X - LineCrossP[j].CrossP.X) < 20)
            //        {
            //            tmpP.X = LineCrossP[j].CrossP.X;
            //            tmpP.Y = LineCrossP[j].CrossP.Y;
            //            LineSeq.CrossP.Add(tmpP);
            //        }
            //    }

            //    Console.WriteLine("LineSeq=");
            //    for (int k = 0; k < LineSeq.CrossP.Count(); k++)
            //        Console.Write(" ,(" + LineSeq.CrossP[k].X + "," + LineSeq.CrossP[k].Y + ")");
            //    Console.WriteLine("hello!");
            //}
        }
        public class _RankCirclePoint
        {
            public int Idx;
            public PointF CircleP;
        }
        class CrossPoint
        {
            public Boolean Label;
            public PointF CrossP;
        }

        class LineSeq
        {
            public int rank;
            public List<PointF> CrossP;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            BlobCounter blobCounter = new BlobCounter();

            Bitmap tmpBitmap = ProcessBitmap;

            Graphics g = Graphics.FromImage(tmpBitmap);

            Pen BluePen = new Pen(Color.Blue, 5);
            Pen RedPen = new Pen(Color.Red, 10);
            System.Drawing.SolidBrush WhiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);//画刷

            // create filter
            BlobsFiltering filter = new BlobsFiltering();
            // configure filter
            filter.CoupledSizeFiltering = true;
            filter.MinWidth = Convert.ToInt32(textBox1.Text);
            filter.MinHeight = Convert.ToInt32(textBox1.Text);
            // apply the filter
            filter.ApplyInPlace(tmpBitmap);

            BitmapData bitmapData = tmpBitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite, tmpBitmap.PixelFormat);


            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            tmpBitmap.UnlockBits(bitmapData);

            //Rectangle[] RecP = blobCounter.GetObjectsRectangles();
            //g.DrawRectangles(BluePen, RecP);
            Console.WriteLine("cnt=" + blobs.Length);

            int CirSize = Convert.ToInt32(textBox2.Text);

            PointF OverlapCenter = new PointF();
            OverlapCenterP = new List<PointF>();

            if (checkBox1.Checked == true)
            {
                foreach (Blob b in blobs)
                {
                    OverlapCenter.X = b.CenterOfGravity.X;
                    OverlapCenter.Y = b.CenterOfGravity.Y;
                    OverlapCenterP.Add(OverlapCenter);
                    g.FillEllipse(WhiteBrush, b.CenterOfGravity.X - CirSize / 2, b.CenterOfGravity.Y - CirSize / 2,
                        CirSize, CirSize);


                }
            }

            BlobBitmap = ProcessBitmap;
            pictureBox2.Image = ProcessBitmap;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int ColorThresh = 20;
            ProcessBitmap = new Bitmap(OriBitmap);

            int w = OriBitmap.Width;
            int h = OriBitmap.Height;

            int[, ,] OtsuBmpArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
            OtsuBmpArr = BMPFast.GetRGBData(OriBitmap);

            int[] pixelCount = new int[ColorThresh];
            double[] pixelPro = new double[ColorThresh];

            for (int i = 0; i < ColorThresh; i++)
            {
                pixelCount[i] = 0;
                pixelPro[i] = 0;
            }
            //統計灰度級中每個像素在整幅圖像中的個數
            int cnt = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if ((OtsuBmpArr[i, j, 0] >= ColorThresh))
                        cnt++;
                    else
                        pixelCount[(int)OtsuBmpArr[i, j, 0]]++;
                }
            }

            //計算每個像素在整幅圖像中的比例
            for (int i = 0; i < ColorThresh; i++)
            {

                pixelPro[i] = ((double)pixelCount[i] / (double)(w * h - cnt));
            }



            //遍歷灰度級[0,255 ]
            double w0, w1, u0tmp, u1tmp, u0, u1, u,
          deltaTmp, deltaMax = 0;

            int thresholdval = 0;

            for (int i = 0; i < ColorThresh; i++)
            {
                w0 = w1 = u0tmp = u1tmp = u0 = u1 = u = deltaTmp = 0;
                for (int j = 0; j < ColorThresh; j++)
                {
                    if (j <= i) //背景部分
                    {
                        w0 += pixelPro[j];
                        u0tmp += j * pixelPro[j];
                    }
                    else //前景部分
                    {
                        w1 += pixelPro[j];
                        u1tmp += j * pixelPro[j];
                    }
                }
                u0 = u0tmp / w0;
                u1 = u1tmp / w1;
                u = u0tmp + u1tmp;
                deltaTmp =
                   w0 * Math.Pow((u0 - u), 2) + w1 * Math.Pow((u1 - u), 2);
                if (deltaTmp > deltaMax)
                {
                    deltaMax = deltaTmp;
                    thresholdval = i;
                }
            }
            Image<Gray, Byte> src = new Image<Gray, byte>(ProcessBitmap); //Image Class from Emgu.CV
            Image<Gray, Byte> dst = new Image<Gray, byte>(ProcessBitmap);
            CvInvoke.Threshold(src, dst, thresholdval, 255, ThresholdType.Binary);
            ProcessBitmap = dst.ToBitmap();

            pictureBox2.Image = ProcessBitmap;

        }

        private void button14_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            clock.Reset();//碼表歸零
            clock.Start();//碼表開始計時
            //=====================================================================================
            //----------------------------------(一)---------------------------------------------
            //=====================================================================================
            int GrayThresh = 20;
            ProcessBitmap = new Bitmap(OriBitmap);

            int w = OriBitmap.Width;
            int h = OriBitmap.Height;

            int[, ,] OtsuBmpArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
            OtsuBmpArr = BMPFast.GetRGBData(OriBitmap);

            int[] pixelCount = new int[GrayThresh];
            double[] pixelPro = new double[GrayThresh];

            for (int i = 0; i < GrayThresh; i++)
            {
                pixelCount[i] = 0;
                pixelPro[i] = 0;
            }
            //統計灰度級中每個像素在整幅圖像中的個數
            int cnt = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if ((OtsuBmpArr[i, j, 0] >= GrayThresh))
                        cnt++;
                    else
                        pixelCount[(int)OtsuBmpArr[i, j, 0]]++;
                }
            }

            //計算每個像素在整幅圖像中的比例
            for (int i = 0; i < GrayThresh; i++)
            {

                pixelPro[i] = ((double)pixelCount[i] / (double)(w * h - cnt));
            }



            //遍歷灰度級[0,255 ]
            double w0, w1, u0tmp, u1tmp, u0, u1, u,
          deltaTmp, deltaMax = 0;

            int thresholdval = 0;

            for (int i = 0; i < GrayThresh; i++)
            {
                w0 = w1 = u0tmp = u1tmp = u0 = u1 = u = deltaTmp = 0;
                for (int j = 0; j < GrayThresh; j++)
                {
                    if (j <= i) //背景部分
                    {
                        w0 += pixelPro[j];
                        u0tmp += j * pixelPro[j];
                    }
                    else //前景部分
                    {
                        w1 += pixelPro[j];
                        u1tmp += j * pixelPro[j];
                    }
                }
                u0 = u0tmp / w0;
                u1 = u1tmp / w1;
                u = u0tmp + u1tmp;
                deltaTmp =
                   w0 * Math.Pow((u0 - u), 2) + w1 * Math.Pow((u1 - u), 2);
                if (deltaTmp > deltaMax)
                {
                    deltaMax = deltaTmp;
                    thresholdval = i;
                }
            }
            Image<Gray, Byte> src = new Image<Gray, byte>(ProcessBitmap); //Image Class from Emgu.CV
            Image<Gray, Byte> dst = new Image<Gray, byte>(ProcessBitmap);
            //Console.WriteLine("thresholdval=" + thresholdval);
            //int thresholdval = 13;
            CvInvoke.Threshold(src, dst, thresholdval, 255, ThresholdType.Binary);
            ProcessBitmap = dst.ToBitmap();

            //pictureBox2.Image = ProcessBitmap;
            //=====================================================================================
            //----------------------------------(二)---------------------------------------------
            //=====================================================================================
            int[, ,] ProcessArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
            ProcessArr = BMPFast.GetRGBData(ProcessBitmap);


            Bitmap DrawBmp = new Bitmap(OriBitmap);
            Graphics g2 = Graphics.FromImage(DrawBmp);
            Pen RedPen = new Pen(Color.Red, 3);

            int BlackCnt = 0;

            List<WeldCenterInfo> AreaList = new List<WeldCenterInfo>();
            WeldCenterInfo PP = new WeldCenterInfo();


            // 1024,1024 左上角 250,250 內的區域
            for (int j = 0; j < 5; j++)
                for (int i = 0; i < 5; i++)
                {
                    PP = new WeldCenterInfo();
                    PP.WeldCandidate.X = 1024 - 50 * i;
                    PP.WeldCandidate.Y = 1024 - 50 * j;
                    AreaList.Add(PP);
                }

            int count = 0;
            foreach (WeldCenterInfo pp in AreaList)
            {
                BlackCnt = 0;
                for (int y = (int)pp.WeldCandidate.Y; y < pp.WeldCandidate.Y + 50; y++)
                {
                    for (int x = (int)pp.WeldCandidate.X; x < pp.WeldCandidate.X + 50; x++)
                    {
                        if (ProcessArr[x, y, 0] == 0)
                            BlackCnt++;
                    }

                }

                AreaList[count].WeldScore = BlackCnt;
                count++;
            }

            AreaList.Sort((p1, p2) => p2.WeldScore.CompareTo(p1.WeldScore));



            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷
            System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);//画刷

            PointF CoarseCenter = new PointF();
            cnt = 0;
            foreach (WeldCenterInfo pp in AreaList)
            {

                //  g2.FillEllipse(RedBrush, pp.WeldCandidate.X - 5, pp.WeldCandidate.Y - 5, 10, 10);
                if (pp.WeldScore > 40 * 40 * 0.2)
                {
                    cnt++;
                    //g2.FillEllipse(BlueBrush, pp.WeldCandidate.X - 5, pp.WeldCandidate.Y - 5, 10, 10);
                    CoarseCenter.X += (pp.WeldCandidate.X + 25);
                    CoarseCenter.Y += (pp.WeldCandidate.Y + 25);
                }

            }
            CoarseCenter.X = CoarseCenter.X / cnt;
            CoarseCenter.Y = CoarseCenter.Y / cnt;
            //--------------------------------此範圍搜尋圓心點-------------------------------------------------------
            BlackCnt = 0;
            int MaxBlackCnt = 0;
            PointF MaxP = new PointF();
            int LOWboundX = (int)CoarseCenter.X - 40;
            int UPboundX = (int)CoarseCenter.X + 40;
            int LOWboundY = (int)CoarseCenter.Y - 40;
            int UPboundY = (int)CoarseCenter.Y + 40;

            for (int n = LOWboundY; n < UPboundY; n++)
                for (int m = LOWboundX; m < UPboundX; m++)
                {
                    BlackCnt = 0;
                    for (int j = n - 45; j < n + 45; j++)
                        for (int i = m - 45; i < m + 45; i++)
                        {
                            if (ProcessArr[i, j, 0] == 0)
                                BlackCnt++;
                        }
                    if (BlackCnt > 45 * 45 * 0.3 && MaxBlackCnt < BlackCnt)
                    {
                        MaxBlackCnt = BlackCnt;
                        MaxP.X = m;
                        MaxP.Y = n;

                    }
                }
            //-------------------------------------------------------------------------------------------------------
            PointF pf = new PointF();
            BlackCircleP = new List<PointF>();

            for (int j = -3; j < 4; j++)
                for (int i = -3; i < 4; i++)
                {
                    pf = new PointF();

                    g2.FillEllipse(BlueBrush, MaxP.X + 277 * i - 15, MaxP.Y + 277 * j - 15, 30, 30);
                    g2.FillEllipse(RedBrush, MaxP.X + 277 * i + 142 - 15, MaxP.Y + 277 * j + 137 - 15, 30, 30);
                    pf.X = MaxP.X + 277 * i + 142;
                    pf.Y = MaxP.Y + 277 * j + 137;
                    BlackCircleP.Add(pf);

                }
            //=====================================================================================
            // ---------------------------------------------------------------------------------------
            // pictureBox2.Image = DrawBmp;
            //----------------------------------(三)---------------------------------------------
            //=====================================================================================
            Image<Bgr, byte> src2 = new Image<Bgr, byte>(OriBitmap); //Image Class from Emgu.CV
            Image<Bgr, byte> dst2 = new Image<Bgr, byte>(OriBitmap);


            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross,
                new Size(3, 3), new System.Drawing.Point(-1, -1));

            CvInvoke.Erode(src2, dst2, element, new System.Drawing.Point(-1, -1), 3,
                Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));


            ProcessBitmap = dst2.ToBitmap();

            //取得灰階影像

            Image<Gray, byte> grayImage = new Image<Gray, byte>(ProcessBitmap);
            ProcessBitmap = grayImage.Bitmap;
            BradleyLocalThresholding BLfilter = new BradleyLocalThresholding();
            ProcessBitmap = BLfilter.Apply(ProcessBitmap);


            src2 = new Image<Bgr, byte>(ProcessBitmap);
            CvInvoke.Erode(src2, dst2, element, new System.Drawing.Point(-1, -1), 3,
             Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));
            ProcessBitmap = dst2.Bitmap;



            //pictureBox2.Image = ProcessBitmap;
            //=====================================================================================
            //----------------------------------(四)---------------------------------------------
            //=====================================================================================
            //filter blob
            Bitmap DrawBitmap = new Bitmap(OriBitmap);
            Bitmap BlobBitmap = new Bitmap(ProcessBitmap);

            GoodWeldCircleP = new List<PointF>();
            BlobCounter blobCounter = new BlobCounter();
            BlobsFiltering Blobfilter = new BlobsFiltering();
            // configure filter
            Blobfilter.CoupledSizeFiltering = true;
            Blobfilter.MinWidth = 20;
            Blobfilter.MinHeight = 20;
            Blobfilter.MaxHeight = 90;
            Blobfilter.MaxWidth = 90;

            Pen BluePen = new Pen(Color.Blue, 5);
            //Pen RedPen = new Pen(Color.Red, 10);
            //System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷
            Graphics g = Graphics.FromImage(DrawBitmap);

            // apply the filter
            Blobfilter.ApplyInPlace(BlobBitmap);

            BitmapData bitmapData = BlobBitmap.LockBits(
             new Rectangle(0, 0, BlobBitmap.Width, BlobBitmap.Height),
             ImageLockMode.ReadWrite, BlobBitmap.PixelFormat);


            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            BlobBitmap.UnlockBits(bitmapData);


            PointF CircleCenter;
            foreach (Blob b in blobs)
            {

                if (Math.Abs(b.Rectangle.Width - b.Rectangle.Height) <= 20
                     && b.Area > 60 && b.Area < 6800
                    && b.Rectangle.Width >= 30
                    )//2F
                {
                    CircleCenter = new PointF();

                    CircleCenter.X = b.CenterOfGravity.X;
                    CircleCenter.Y = b.CenterOfGravity.Y;

                    double GrayVal = (OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 0]
                                    + OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 1]
                                    + OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 2]) / 3;
                    if (GrayVal > 60)
                    {
                        GoodWeldCircleP.Add(CircleCenter);
                        //    g.FillEllipse(BlueBrush, CircleCenter.X - 15, CircleCenter.Y - 15, 30, 30);
                    }
                }
            }

            // pictureBox2.Image = DrawBitmap;
            //=====================================================================================
            //----------------------------------(五)---------------------------------------------
            //=====================================================================================
            int GridLength = 0;
            double ShortDist = 100000000;
            double ShortDistAvg = 0;
            int AvgCnt = 0;
            double dist;
            foreach (PointF p1 in BlackCircleP)
            {
                ShortDist = 100000;

                foreach (PointF p2 in BlackCircleP)
                {
                    if (p1 != p2)
                    {
                        dist = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
                        if (dist < ShortDist && dist > 200 * 200 && dist < 300 * 300) ShortDist = dist;  //橫向比較短直向比較長所以放寬range
                    }
                }
                ShortDistAvg += ShortDist;
                AvgCnt++;
            }

            ShortDistAvg = Math.Sqrt(ShortDistAvg / AvgCnt);
            GridLength = (int)ShortDistAvg;

            ////-------------------------------------------------------------------------------------

            List<List<PointF>> LineCollect = new List<List<PointF>>();
            List<PointF> GridLine = new List<PointF>();
            for (int i = 0; i < 10; i++)
                LineCollect.Add(GridLine);

            for (int i = 0; i < 10; i++)
                LineCollect[i] = new List<PointF>();

            double MinY = 10000000, MinX = 10000000;

            foreach (PointF p in BlackCircleP)
            {
                //1. 找X最小的點, 在最左邊
                //2. 找Y最小的點,在最上面

                // if (MinX > p.X) MinX = p.X;
                if (MinY > p.Y) MinY = p.Y;

            }

            //歸線&排序

            foreach (PointF p in BlackCircleP)
            {

                //2F
                if (Math.Abs((p.Y - MinY)) <= 20)//第一條橫線
                    LineCollect[0].Add(p);
                else if (Math.Abs((p.Y - (MinY + GridLength))) <= 25)//第2條橫線
                    LineCollect[1].Add(p);
                else if (Math.Abs((p.Y - (MinY + GridLength * 2))) <= 30)//第2條橫線
                    LineCollect[2].Add(p);

                else if (Math.Abs((p.Y - (MinY + GridLength * 3))) <= 35)//第2條橫線
                    LineCollect[3].Add(p);

                else if (Math.Abs((p.Y - (MinY + GridLength * 4))) <= 40)//第2條橫線
                    LineCollect[4].Add(p);

                else if (Math.Abs((p.Y - (MinY + GridLength * 5))) <= 45)//第2條橫線
                    LineCollect[5].Add(p);

                else if (Math.Abs((p.X - (MinY + GridLength * 6))) <= 50)//第2條橫線
                    LineCollect[6].Add(p);

                else if (Math.Abs((p.X - (MinY + GridLength * 7))) <= 50)//第2條橫線
                    LineCollect[7].Add(p);

                else if (Math.Abs((p.X - (MinY + GridLength * 8))) <= 50)//第2條橫線
                    LineCollect[8].Add(p);

                else if (Math.Abs((p.X - (MinY + GridLength * 9))) <= 50)//第2條橫線
                    LineCollect[9].Add(p);

            }
            for (int k = 0; k < 10; k++)
            {
                LineCollect[k].Sort((p1, p2) => p1.X.CompareTo(p2.X));
                //Console.WriteLine("LineCollect" + k + "=" + LineCollect[k].Count());
            }


            ////-------------------------------------------------------------------------------------


            List<List<PointF>> ColumnCollect = new List<List<PointF>>();
            List<PointF> GridColumn = new List<PointF>();
            for (int i = 0; i < 10; i++)
                ColumnCollect.Add(GridColumn);

            for (int i = 0; i < 10; i++)
                ColumnCollect[i] = new List<PointF>();



            foreach (PointF p in BlackCircleP)
            {
                //1. 找X最小的點, 在最左邊
                //2. 找Y最小的點,在最上面
                if (MinX > p.X) MinX = p.X;
            }


            foreach (PointF p in BlackCircleP)//取十條
            {

                //2F
                if (Math.Abs((p.X - MinX)) <= 20)//第一條橫線
                    ColumnCollect[0].Add(p);
                else if (Math.Abs((p.X - (MinX + GridLength))) <= 20)//第2條橫線
                    ColumnCollect[1].Add(p);
                else if (Math.Abs((p.X - (MinX + GridLength * 2))) <= 25)//第2條橫線
                    ColumnCollect[2].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 3))) <= 35)//第2條橫線
                    ColumnCollect[3].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 4))) <= 40)//第2條橫線
                    ColumnCollect[4].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 5))) <= 45)//第2條橫線
                    ColumnCollect[5].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 6))) <= 50)//第2條橫線
                    ColumnCollect[6].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 7))) <= 50)//第2條橫線
                    ColumnCollect[7].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 8))) <= 50)//第2條橫線
                    ColumnCollect[8].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 9))) <= 50)//第2條橫線
                    ColumnCollect[9].Add(p);

            }
            for (int k = 0; k < 10; k++)
            {
                ColumnCollect[k].Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                //  Console.WriteLine("ColumnCollect" + k + "=" + ColumnCollect[k].Count());
            }

            //-------------------------------------------------------------------------------------
            int GridNum = 10;
            MapGrid2 = new PointF[100, 100];
            //1.6.5 by 線組合計算x座標平均做為爛銲道 x 座標y 座標
            MapGrid2 = new PointF[GridNum, GridNum];
            int YAvg = 0;
            ////求每條線的 Y 平均
            for (int k = 0; k < LineCollect.Count(); k++)
            {
                YAvg = 0;
                if (LineCollect[k].Count() > 0)
                {

                    foreach (PointF p in LineCollect[k])
                        YAvg += (int)p.Y;

                    YAvg = YAvg / LineCollect[k].Count();

                    for (int i = 0; i < GridNum; i++)
                    {
                        if (YAvg != 0)
                            MapGrid2[i, k].Y = YAvg;
                    }
                }
            }

            int XAvg = 0;
            //求每條線的 Y 平均
            for (int k = 0; k < ColumnCollect.Count(); k++)
            {
                XAvg = 0;
                if (ColumnCollect[k].Count() > 0)
                {

                    foreach (PointF p in ColumnCollect[k])
                        XAvg += (int)p.X;

                    XAvg = XAvg / ColumnCollect[k].Count();

                    for (int j = 0; j < GridNum; j++)
                    {
                        if (XAvg != 0)
                            MapGrid2[k, j].X = XAvg;
                    }
                }
            }
            ////---------------------------填入好的銲道-----------------------------------------
            ShortDist = 100000000;
            dist = 0;
            PointF ShortP = new PointF();

            for (int j = 0; j < 10; j++)
                for (int i = 0; i < 10; i++)
                {
                    ShortP = new PointF();
                    ShortDist = 100000000;
                    foreach (PointF pp in GoodWeldCircleP)
                    {
                        dist = (MapGrid2[i, j].X - pp.X) * (MapGrid2[i, j].X - pp.X) + (MapGrid2[i, j].Y - pp.Y) * (MapGrid2[i, j].Y - pp.Y);
                        if (dist < 50 * 50 && dist < ShortDist)
                        {
                            ShortDist = dist;
                            ShortP.X = pp.X;
                            ShortP.Y = pp.Y;
                        }
                    }

                    if (ShortP.X == 0 && ShortP.Y == 0)
                    { }
                    else
                    {
                        MapGrid2[i, j].X = ShortP.X;
                        MapGrid2[i, j].Y = ShortP.Y;
                    }

                }
            ////-------------------------------------------------------------------------------------



            //DrawBitmap = new Bitmap(OriBitmap);
            //BlackCircleP = new List<PointF>();

            //g = Graphics.FromImage(DrawBitmap);
            //for (int j = 0; j < 10; j++)
            //    for (int i = 0; i < 10; i++)
            //    {
            //        //排除 0 的, 從黑洞中心拉一個矩形即為銲道初始位置
            //        if (MapGrid2[i, j].X != 0 && MapGrid2[i, j].Y != 0)
            //        {

            //            g.FillEllipse(BlueBrush, MapGrid2[i, j].X - 15, MapGrid2[i, j].Y - 15, 30, 30);
            //        }
            //    }

            //    //===================================================================================
            //    //----------------------------------(六)---------------------------------------------
            //    //===================================================================================

            //------------------------------------------------------
            //1. TEMPLATE訂定
            //------------------------------------------------------
            int TempSize = 64;
            Bitmap TemplateBMP = new Bitmap(TempSize + 8, TempSize + 8);
            g2 = Graphics.FromImage(TemplateBMP);
            Pen BlackPen = new Pen(Color.Black, 1);

            g2.Clear(Color.White);
            g2.DrawEllipse(BlackPen, 5, 5, TempSize, TempSize);

            //讀取 template 黑框圓周點
            int[, ,] TemplateData = BMPFast.GetRGBData(TemplateBMP);

            List<System.Drawing.Point> TemplateEdgeList = new List<System.Drawing.Point>();
            System.Drawing.Point EdgeP;

            for (int j = 0; j < TemplateBMP.Height; j++)
            {
                for (int i = 0; i < TemplateBMP.Width; i++)
                {
                    if (TemplateData[i, j, 0] == 0 && TemplateData[i, j, 1] == 0 && TemplateData[i, j, 2] == 0)
                    {
                        EdgeP = new System.Drawing.Point();
                        EdgeP.X = i;
                        EdgeP.Y = j;
                        TemplateEdgeList.Add(EdgeP);
                    }
                }
            }

            // -------------------------2.Template Mapping------------------------
            int[, ,] ProcessData = BMPFast.GetRGBData(ProcessBitmap);

            //debug 點用

            WeldCenterInfo[,] WeldCenterList = new WeldCenterInfo[7, 7];
            // _TempEdgeDraw = new List<System.Drawing.Point>();


            for (int j = 0; j < 7; j++)
                for (int i = 0; i < 7; i++)
                    WeldCenterList[i, j] = new WeldCenterInfo();
            int EdgeCnt = 0;


            WeldCenterInfo MaxCandidte;
            int MaxScore = 0;
            //    //---------------------------------------
            for (int mmj = 0; mmj < 7; mmj++)
                for (int mmi = 0; mmi < 7; mmi++)
                {
                    MaxCandidte = new WeldCenterInfo();
                    MaxCandidte.WeldScore = 0;
                    MaxScore = 0;
                    if (!GoodWeldCircleP.Exists(p => p.X == MapGrid2[mmi, mmj].X) && !(GoodWeldCircleP.Exists(p => p.Y == MapGrid2[mmi, mmj].Y)))
                    //沒有在理想list的網格點
                    {


                        for (int j = (int)(MapGrid2[mmi, mmj].Y - 15); j < (int)(MapGrid2[mmi, mmj].Y + 15); j++)
                        {
                            for (int i = (int)(MapGrid2[mmi, mmj].X - 15); i < (int)(MapGrid2[mmi, mmj].X + 15); i++)
                            {


                                EdgeCnt = 0;

                                //算出每個點專屬的EGDE範圍 
                                foreach (System.Drawing.Point p in TemplateEdgeList)//每一個 template 的 edge 點
                                {
                                    PointF TT = new PointF();
                                    TT.X = p.X + (i - 37);
                                    TT.Y = p.Y + (j - 37);

                                    if (TT.X > 0 && TT.Y > 0 && TT.X < 2048 && TT.Y < 2048)
                                        if (ProcessData[(int)TT.X, (int)TT.Y, 0] == 0) //TEMPLATE edge 點位移至當前位置, 判斷是否也為邊緣點(黑色)
                                        {
                                            EdgeCnt++;

                                        }


                                }

                                if (EdgeCnt > MaxScore && EdgeCnt > TemplateEdgeList.Count() * 0.9)//一半以上銲道點符合
                                {

                                    MaxScore = EdgeCnt;
                                    MaxCandidte.WeldScore = EdgeCnt;
                                    MaxCandidte.WeldCandidate.X = i;
                                    MaxCandidte.WeldCandidate.Y = j;

                                }




                            }//FOR I
                        }//FOR J


                        if (!(MaxCandidte.WeldCandidate.X == MapGrid2[mmi, mmj].X && MaxCandidte.WeldCandidate.Y == MapGrid2[mmi, mmj].Y))
                        {


                            WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                            WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;
                            WeldCenterList[mmi, mmj].WeldScore = MaxCandidte.WeldScore;

                        }

                    }//end if not goodlist

                    else //理想銲道則不用尋找,直接用原本的
                    {
                        MaxCandidte.WeldScore = 100000;
                        MaxCandidte.WeldCandidate.X = (int)MapGrid2[mmi, mmj].X;
                        MaxCandidte.WeldCandidate.Y = (int)MapGrid2[mmi, mmj].Y;
                        WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                        WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;
                        WeldCenterList[mmi, mmj].WeldScore = MaxCandidte.WeldScore;

                    }


                    if (MaxCandidte.WeldScore == 0)
                    {
                        MaxCandidte.WeldCandidate.X = (int)MapGrid2[mmi, mmj].X;
                        MaxCandidte.WeldCandidate.Y = (int)MapGrid2[mmi, mmj].Y;
                        WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                        WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;

                    }
                }//for map


            clock.Stop();//碼錶停止
            //印出所花費的總豪秒數
            string result1 = clock.Elapsed.TotalMilliseconds.ToString();
            Console.WriteLine("result1=" + result1);


            Pen GreenPen = new Pen(Color.Green, 10);
            System.Drawing.SolidBrush GreenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷

            DrawBitmap = new Bitmap(OriBitmap);
            BlackCircleP = new List<PointF>();

            g = Graphics.FromImage(DrawBitmap);


            for (int j = 0; j < 7; j++)
                for (int i = 0; i < 7; i++)
                {
                    //處理找不到者 :０分，　座標-1
                    string drawString = "";
                    System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 30);
                    System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
                    float x = 150.0F;
                    float y = 50.0F;
                    System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();



                    if (WeldCenterList[i, j].WeldScore < 9999)
                    {
                        if (WeldCenterList[i, j].WeldScore == 0)
                        {
                            drawString = "x";
                            Console.WriteLine("x: " + i + "," + j);
                        }
                        else
                        {
                            drawString = WeldCenterList[i, j].WeldScore.ToString();
                            Console.WriteLine("<9999: " + i + "," + j);
                        }
                        g.DrawString(drawString, drawFont, drawBrush, WeldCenterList[i, j].WeldCandidate.X - 30, WeldCenterList[i, j].WeldCandidate.Y + 40, drawFormat);
                    }

                    if (WeldCenterList[i, j].WeldScore != 0)
                    {
                        g.FillEllipse(GreenBrush, WeldCenterList[i, j].WeldCandidate.X - 15, WeldCenterList[i, j].WeldCandidate.Y - 15, 30, 30);
                        Console.WriteLine(">0: " + i + "," + j);

                    }

                }
            pictureBox2.Image = DrawBitmap;








        }

        private void button13_Click(object sender, EventArgs e)
        {


            if (radioButton1.Checked == true)
                ProcessBitmap = OriBitmap;

            int ThreshVal = Convert.ToInt32(textBox3.Text);
            int GrayVel = 0;

            //bitmapOverlap = new int[OriBitmap.Width, OriBitmap.Height];
            int[, ,] ThreshValArr = new int[OriBitmap.Width, OriBitmap.Height, 3];

            for (int j = 0; j < OriBitmap.Height; j++)
                for (int i = 0; i < OriBitmap.Width; i++)
                {
                    GrayVel = (OriBitmapArr[i, j, 0] + OriBitmapArr[i, j, 1] + OriBitmapArr[i, j, 2]) / 3;
                    if (GrayVel > ThreshVal)
                    {
                        ThreshValArr[i, j, 0] = 255;
                        ThreshValArr[i, j, 1] = 255;
                        ThreshValArr[i, j, 2] = 255;
                    }
                    else
                    {
                        ThreshValArr[i, j, 0] = 0;
                        ThreshValArr[i, j, 1] = 0;
                        ThreshValArr[i, j, 2] = 0;

                    }
                }

            ProcessBitmap = BMPFast.SetRGBData(ThreshValArr);
            pictureBox2.Image = ProcessBitmap;




        }

        private void button11_Click(object sender, EventArgs e)
        {
            Image<Bgr, Byte> src = new Image<Bgr, byte>(ProcessBitmap); //Image Class from Emgu.CV
            Image<Bgr, Byte> dst = new Image<Bgr, byte>(ProcessBitmap);


            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross,
                new Size(3, 3), new System.Drawing.Point(-1, -1));

            CvInvoke.Erode(src, dst, element, new System.Drawing.Point(-1, -1), 3,
                Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));


            ProcessBitmap = dst.ToBitmap();
            ErosionBitmap = dst.ToBitmap();

            pictureBox2.Image = ProcessBitmap;



        }

        private void button15_Click(object sender, EventArgs e)
        {
            GoodMapList = new List<PointF>();

            BlobCounter blobCounter = new BlobCounter();

            Bitmap tmpBitmap = ProcessBitmap;

            Graphics g = Graphics.FromImage(tmpBitmap);

            Pen BluePen = new Pen(Color.Blue, 5);
            Pen RedPen = new Pen(Color.Red, 10);
            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷

            // create filter
            BlobsFiltering filter = new BlobsFiltering();

            filter.ApplyInPlace(tmpBitmap);

            w = tmpBitmap.Width;
            h = tmpBitmap.Height;
            BitmapData bitmapData = tmpBitmap.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadWrite, tmpBitmap.PixelFormat);


            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            tmpBitmap.UnlockBits(bitmapData);


            CircleCenterP = new List<PointF>();

            PointF CircleCenter;
            double dist = 0, short_dist = 10000000;
            int cnt = 0;
            PointF PP = new PointF();
            foreach (Blob b in blobs)
            {
                cnt = 0;
                short_dist = 10000000;

                if ((b.Rectangle.Width / b.Rectangle.Height) < 3 && (b.Rectangle.Height / b.Rectangle.Width) < 3
                    && (b.Area > 35 * 35 * 3.14))
                {
                    CircleCenter = new PointF();

                    CircleCenter.X = b.CenterOfGravity.X;
                    CircleCenter.Y = b.CenterOfGravity.Y;


                    List<IntPoint> edgePts = blobCounter.GetBlobsEdgePoints(b);

                    foreach (IntPoint IP in edgePts)
                    {
                        dist = (IP.X - CircleCenter.X) * (IP.X - CircleCenter.X) + (IP.Y - CircleCenter.Y) * (IP.Y - CircleCenter.Y);

                        if (dist < short_dist) short_dist = dist;

                        if (dist < 50 * 50 && dist > 30 * 30)
                            cnt++;
                    }
                    Console.WriteLine("cnt=" + cnt + ", edgePts=" + edgePts.Count());
                    if (((double)cnt / (double)edgePts.Count()) > 0.3 && short_dist > 30 * 30)
                    {
                        g.FillEllipse(RedBrush, b.CenterOfGravity.X - 10, b.CenterOfGravity.Y - 10, 20, 20);
                        PP.X = b.CenterOfGravity.X;
                        PP.Y = b.CenterOfGravity.Y;
                        GoodMapList.Add(PP);
                    }
                }


            }
            pictureBox2.Image = tmpBitmap;



        }

        private void button16_Click(object sender, EventArgs e)
        {
            SaveBMP = ProcessBitmap;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            string curDir;
            curDir = Directory.GetCurrentDirectory();
            saveFileDialog1.InitialDirectory = curDir;

            saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            saveFileDialog1.Title = "儲存影像檔";
            saveFileDialog1.FilterIndex = 3;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                SaveBMP.Save(saveFileDialog1.FileName);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            Pen BluePen = new Pen(Color.Blue, 5);
            Pen RedPen = new Pen(Color.Red, 10);
            System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷
            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷

            //找最左上角那一顆
            Graphics g = Graphics.FromImage(OriBitmap);

            float MinX = 100000000, MinY = 100000000;
            foreach (PointF pt in BlackCircleP)
            {
                if (pt.X < MinX)
                    MinX = pt.X;
                if (pt.Y < MinY)
                    MinY = pt.Y;
                // g.FillEllipse(RedBrush, pt.X - 15, pt.Y - 15, 30, 30);
            }

            double dist;
            double ShortDist = 1000000;

            for (int j = 0; j < 10; j++)
                for (int i = 0; i < 10; i++)
                {
                    ShortDist = 1000000;
                    foreach (PointF pt in WeldCircleP)//把最接近的好銲道更新進去
                    {
                        if (pt.X > MinX && pt.Y > MinY)
                        {
                            dist = (MapGrid2[i, j].X - pt.X) * (MapGrid2[i, j].X - pt.X) + (MapGrid2[i, j].Y - pt.Y) * (MapGrid2[i, j].Y - pt.Y);
                            if (dist < ShortDist && dist < 70 * 70)
                            {
                                ShortDist = dist;
                                MapGrid2[i, j].X = pt.X;
                                MapGrid2[i, j].Y = pt.Y;
                            }
                        }
                    }
                    if (MapGrid2[i, j].X != 0 && MapGrid2[i, j].Y != 0)
                        g.FillEllipse(RedBrush, MapGrid2[i, j].X - 15, MapGrid2[i, j].Y - 15, 30, 30);

                }

            pictureBox2.Image = OriBitmap;
        }

        private void button18_Click(object sender, EventArgs e)
        {
            //int[, ,] ProcessArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
            //ProcessArr = BMPFast.GetRGBData(ProcessBitmap);

            //filter blob
            Bitmap DrawBitmap = new Bitmap(OriBitmap);
            Bitmap BlobBitmap = new Bitmap(ProcessBitmap);

            GoodWeldCircleP = new List<PointF>();
            BlobCounter blobCounter = new BlobCounter();
            BlobsFiltering Blobfilter = new BlobsFiltering();
            // configure filter
            Blobfilter.CoupledSizeFiltering = true;
            Blobfilter.MinWidth = 20;
            Blobfilter.MinHeight = 20;
            Blobfilter.MaxHeight = 90;
            Blobfilter.MaxWidth = 90;

            Pen BluePen = new Pen(Color.Blue, 5);
            Pen RedPen = new Pen(Color.Red, 10);
            System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷
            Graphics g = Graphics.FromImage(DrawBitmap);

            // apply the filter
            Blobfilter.ApplyInPlace(BlobBitmap);

            BitmapData bitmapData = BlobBitmap.LockBits(
             new Rectangle(0, 0, BlobBitmap.Width, BlobBitmap.Height),
             ImageLockMode.ReadWrite, BlobBitmap.PixelFormat);


            blobCounter.ProcessImage(bitmapData);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            BlobBitmap.UnlockBits(bitmapData);


            PointF CircleCenter;
            foreach (Blob b in blobs)
            {

                if (Math.Abs(b.Rectangle.Width - b.Rectangle.Height) <= 20
                     && b.Area > 60 && b.Area < 6800
                    && b.Rectangle.Width >= 30
                    )//2F
                {
                    CircleCenter = new PointF();

                    CircleCenter.X = b.CenterOfGravity.X;
                    CircleCenter.Y = b.CenterOfGravity.Y;

                    double GrayVal = (OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 0]
                                    + OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 1]
                                    + OriBitmapArr[(int)CircleCenter.X, (int)CircleCenter.Y, 2]) / 3;
                    if (GrayVal > 50)
                    {
                        GoodWeldCircleP.Add(CircleCenter);
                        g.FillEllipse(BlueBrush, CircleCenter.X - 15, CircleCenter.Y - 15, 30, 30);
                    }
                }
            }

            pictureBox2.Image = DrawBitmap;




        }


        private void button19_Click(object sender, EventArgs e)
        {

            int GridLength = 0;
            double ShortDist = 100000000;
            double ShortDistAvg = 0;
            int AvgCnt = 0;
            double dist;
            foreach (PointF p1 in BlackCircleP)
            {
                ShortDist = 100000;

                foreach (PointF p2 in BlackCircleP)
                {
                    if (p1 != p2)
                    {
                        dist = (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
                        if (dist < ShortDist && dist > 200 * 200 && dist < 300 * 300) ShortDist = dist;  //橫向比較短直向比較長所以放寬range
                    }
                }
                ShortDistAvg += ShortDist;
                AvgCnt++;
            }

            ShortDistAvg = Math.Sqrt(ShortDistAvg / AvgCnt);
            GridLength = (int)ShortDistAvg;

            ////-------------------------------------------------------------------------------------

            List<List<PointF>> LineCollect = new List<List<PointF>>();
            List<PointF> GridLine = new List<PointF>();
            for (int i = 0; i < 10; i++)
                LineCollect.Add(GridLine);

            for (int i = 0; i < 10; i++)
                LineCollect[i] = new List<PointF>();

            double MinY = 10000000, MinX = 10000000;

            foreach (PointF p in BlackCircleP)
            {
                //1. 找X最小的點, 在最左邊
                //2. 找Y最小的點,在最上面

                // if (MinX > p.X) MinX = p.X;
                if (MinY > p.Y) MinY = p.Y;

            }

            //歸線&排序
            int prtcnt = 0;
            foreach (PointF p in BlackCircleP)
            {

                //Console.WriteLine(prtcnt + "(" + p.X + "," + p.Y + ")=" + OriBitmapArr[(int)p.X, (int)p.Y, 0]);
                prtcnt++;
                if (OriBitmapArr[(int)p.X, (int)p.Y, 0] > 40)
                {
                    //2F
                    if (Math.Abs((p.Y - MinY)) <= 20)//第一條橫線
                        LineCollect[0].Add(p);
                    else if (Math.Abs((p.Y - (MinY + GridLength))) <= 25)//第2條橫線
                        LineCollect[1].Add(p);
                    else if (Math.Abs((p.Y - (MinY + GridLength * 2))) <= 30)//第2條橫線
                        LineCollect[2].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 3))) <= 35)//第2條橫線
                        LineCollect[3].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 4))) <= 40)//第2條橫線
                        LineCollect[4].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 5))) <= 45)//第2條橫線
                        LineCollect[5].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 6))) <= 50)//第2條橫線
                        LineCollect[6].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 7))) <= 50)//第2條橫線
                        LineCollect[7].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 8))) <= 50)//第2條橫線
                        LineCollect[8].Add(p);

                    else if (Math.Abs((p.Y - (MinY + GridLength * 9))) <= 50)//第2條橫線
                        LineCollect[9].Add(p);
                }

            }
            for (int k = 0; k < 10; k++)
            {
                LineCollect[k].Sort((p1, p2) => p1.X.CompareTo(p2.X));
                //Console.WriteLine("LineCollect" + k + "=" + LineCollect[k].Count());
            }

            // 如果該條線 >40 者<3個則視為空電芯那行
            int cnt = 0;
            for (int k = 0; k < LineCollect.Count; k++)
            {
                cnt = 0;
                for (int i = 0; i < LineCollect[k].Count; i++)
                    if (OriBitmapArr[(int)LineCollect[k][i].X, (int)LineCollect[k][i].Y, 0] > 40) cnt++;

                if (cnt < 3)
                    LineCollect.Remove(LineCollect[k]);
            }

            ////-------------------------------------------------------------------------------------


            List<List<PointF>> ColumnCollect = new List<List<PointF>>();
            List<PointF> GridColumn = new List<PointF>();
            for (int i = 0; i < 10; i++)
                ColumnCollect.Add(GridColumn);

            for (int i = 0; i < 10; i++)
                ColumnCollect[i] = new List<PointF>();



            foreach (PointF p in BlackCircleP)
            {
                //1. 找X最小的點, 在最左邊
                //2. 找Y最小的點,在最上面
                if (MinX > p.X) MinX = p.X;
            }


            foreach (PointF p in BlackCircleP)//取十條
            {  //2F
                if (Math.Abs((p.X - MinX)) <= 20)//第一條橫線
                    ColumnCollect[0].Add(p);
                else if (Math.Abs((p.X - (MinX + GridLength))) <= 20)//第2條橫線
                    ColumnCollect[1].Add(p);
                else if (Math.Abs((p.X - (MinX + GridLength * 2))) <= 25)//第2條橫線
                    ColumnCollect[2].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 3))) <= 35)//第2條橫線
                    ColumnCollect[3].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 4))) <= 40)//第2條橫線
                    ColumnCollect[4].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 5))) <= 45)//第2條橫線
                    ColumnCollect[5].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 6))) <= 50)//第2條橫線
                    ColumnCollect[6].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 7))) <= 50)//第2條橫線
                    ColumnCollect[7].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 8))) <= 50)//第2條橫線
                    ColumnCollect[8].Add(p);

                else if (Math.Abs((p.X - (MinX + GridLength * 9))) <= 50)//第2條橫線
                    ColumnCollect[9].Add(p);


            }
            for (int k = 0; k < 10; k++)
            {
                ColumnCollect[k].Sort((p1, p2) => p1.Y.CompareTo(p2.Y));
                //  Console.WriteLine("ColumnCollect" + k + "=" + ColumnCollect[k].Count());
            }



            //-------------------------------------------------------------------------------------
            int GridNum = 10;
            int ColumnCnt;
            if ((ImgFileName.IndexOf("_3") > 0) || (ImgFileName.IndexOf("_4") > 0))
            {
                ColumnCnt = 1;
                MapGrid2 = new PointF[ColumnCnt, GridNum]; //圖三圖四只有一排
            }
            else
            {

                //1.6.5 by 線組合計算x座標平均做為爛銲道 x 座標y 座標
                MapGrid2 = new PointF[GridNum, GridNum];
                ColumnCnt = ColumnCollect.Count();
            }
            int YAvg = 0;
            ////求每條線的 Y 平均
            for (int k = 0; k < LineCollect.Count(); k++)
            {
                YAvg = 0;
                if (LineCollect[k].Count() > 0)
                {

                    foreach (PointF p in LineCollect[k])
                        YAvg += (int)p.Y;

                    YAvg = YAvg / LineCollect[k].Count();

                    for (int i = 0; i < ColumnCnt; i++)
                    {
                        if (YAvg != 0)
                            MapGrid2[i, k].Y = YAvg;
                    }
                }
            }

            int XAvg = 0;
            //求每條線的 Y 平均


            for (int k = 0; k < ColumnCnt; k++)
            {
                XAvg = 0;
                if (ColumnCollect[k].Count() > 0)
                {

                    foreach (PointF p in ColumnCollect[k])
                        XAvg += (int)p.X;

                    XAvg = XAvg / ColumnCollect[k].Count();

                    for (int j = 0; j < GridNum; j++)
                    {
                        if (XAvg != 0)
                            MapGrid2[k, j].X = XAvg;
                    }
                }
            }
            ////---------------------------填入好的銲道-----------------------------------------
            ShortDist = 100000000;
            dist = 0;
            PointF ShortP = new PointF();

            for (int j = 0; j < 10; j++)
                for (int i = 0; i < ColumnCnt; i++)
                {
                    ShortP = new PointF();
                    ShortDist = 100000000;
                    foreach (PointF pp in GoodWeldCircleP)
                    {
                        dist = (MapGrid2[i, j].X - pp.X) * (MapGrid2[i, j].X - pp.X) + (MapGrid2[i, j].Y - pp.Y) * (MapGrid2[i, j].Y - pp.Y);
                        if (dist < 50 * 50 && dist < ShortDist)
                        {
                            ShortDist = dist;
                            ShortP.X = pp.X;
                            ShortP.Y = pp.Y;
                        }
                    }

                    if (ShortP.X == 0 && ShortP.Y == 0)
                    { }
                    else
                    {
                        MapGrid2[i, j].X = ShortP.X;
                        MapGrid2[i, j].Y = ShortP.Y;
                    }

                }
            ////-------------------------------------------------------------------------------------

            Pen BluePen = new Pen(Color.Blue, 5);
            Pen RedPen = new Pen(Color.Red, 10);
            System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);//画刷
            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷

            Bitmap DrawBitmap = new Bitmap(OriBitmap);
            BlackCircleP = new List<PointF>();

            Graphics g = Graphics.FromImage(DrawBitmap);
            for (int j = 0; j < 10; j++)
                for (int i = 0; i < ColumnCnt; i++)
                {
                    //排除 0 的, 從黑洞中心拉一個矩形即為銲道初始位置
                    if (MapGrid2[i, j].X != 0 && MapGrid2[i, j].Y != 0)
                    {

                        g.FillEllipse(BlueBrush, MapGrid2[i, j].X - 15, MapGrid2[i, j].Y - 15, 30, 30);
                    }
                }



            pictureBox2.Image = DrawBitmap;



        }

        private void button20_Click(object sender, EventArgs e)
        {

            Bitmap newBMP1 = new Bitmap(85, 85);
            Graphics g = Graphics.FromImage(newBMP1);
            Pen BlackPen = new Pen(Color.Black, 1);
            Pen RedPen = new Pen(Color.Red, 5);
            g.Clear(Color.White);
            g.DrawEllipse(BlackPen, 5, 5, 64, 64);
            TemplateColor = new int[newBMP1.Width, newBMP1.Height, 3];
            TemplateColor = GetRGBData(newBMP1);
            g.DrawEllipse(RedPen, 37, 37, 1, 1);

            //Bitmap TemplateBMP = new Bitmap(72, 72);
            //Graphics g2 = Graphics.FromImage(TemplateBMP);
            //Pen BlackPen = new Pen(Color.Black, 1);
            //Pen RedPen = new Pen(Color.Red, 5);
            //g2.Clear(Color.White);
            //g2.DrawEllipse(BlackPen, 5, 5, 64, 64);

            pictureBox2.Image = newBMP1;
            SaveBMP = newBMP1;

        }
        //高效率圖形轉換工具--由陣列設定新的Bitmap
        public static Bitmap SetRGBData(int[, ,] rgbData)
        {
            //宣告Bitmap變數
            Bitmap bitImg;
            int width = rgbData.GetLength(0);
            int height = rgbData.GetLength(1);

            //依陣列長寬設定Bitmap新的物件
            bitImg = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            //鎖住Bitmap整個影像內容
            BitmapData bitmapData = bitImg.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //取得影像資料的起始位置
            IntPtr imgPtr = bitmapData.Scan0;
            //影像scan的寬度
            int stride = bitmapData.Stride;
            //影像陣列的實際寬度
            int widthByte = width * 3;
            //所Padding的Byte數
            int skipByte = stride - widthByte;

            #region 設定RGB資料
            //注意C#的GDI+內的影像資料順序為BGR, 非一般熟悉的順序RGB
            //因此我們把順序調回GDI+的設定值, RGB->BGR
            unsafe
            {
                byte* p = (byte*)(void*)imgPtr;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        //B Channel
                        p[0] = (byte)rgbData[i, j, 2];
                        p++;
                        //G Channel
                        p[0] = (byte)rgbData[i, j, 1];
                        p++;
                        //B Channel
                        p[0] = (byte)rgbData[i, j, 0];
                        p++;
                    }
                    p += skipByte;
                }
            }

            //解開記憶體鎖
            bitImg.UnlockBits(bitmapData);

            #endregion

            return bitImg;
        }
        private void button21_Click(object sender, EventArgs e)
        {
            //------------------------------------------------------
            //1. TEMPLATE訂定
            //------------------------------------------------------
            int TempSize = 64;

            Bitmap TemplateBMP = new Bitmap(TempSize + 8, TempSize + 8);
            Graphics g2 = Graphics.FromImage(TemplateBMP);
            Pen BlackPen = new Pen(Color.Black, 1);
            Pen RedPen = new Pen(Color.Red, 5);
            g2.Clear(Color.White);
            g2.DrawEllipse(BlackPen, 5, 5, TempSize, TempSize);

            //讀取 template 黑框圓周點
            int[, ,] TemplateData = BMPFast.GetRGBData(TemplateBMP);

            List<System.Drawing.Point> TemplateEdgeList = new List<System.Drawing.Point>();
            System.Drawing.Point EdgeP;

            for (int j = 0; j < TemplateBMP.Height; j++)
            {
                for (int i = 0; i < TemplateBMP.Width; i++)
                {
                    if (TemplateData[i, j, 0] == 0 && TemplateData[i, j, 1] == 0 && TemplateData[i, j, 2] == 0)
                    {
                        EdgeP = new System.Drawing.Point();
                        EdgeP.X = i;
                        EdgeP.Y = j;
                        TemplateEdgeList.Add(EdgeP);
                    }
                }
            }

            // -------------------------2.Template Mapping------------------------
            int[, ,] ProcessData = BMPFast.GetRGBData(ProcessBitmap);

            //debug 點用
            int ColumnCnt ;
            int GridNum = 10;
            if ((ImgFileName.IndexOf("_3") > 0) || (ImgFileName.IndexOf("_4") > 0))
            {
                ColumnCnt = 1;
            }
            else
            {
                ColumnCnt = 10;
            }


            WeldCenterInfo[,] WeldCenterList = new WeldCenterInfo[ColumnCnt, GridNum];
            // _TempEdgeDraw = new List<System.Drawing.Point>();


            for (int j = 0; j < GridNum; j++)
                for (int i = 0; i < ColumnCnt; i++)
                    WeldCenterList[i, j] = new WeldCenterInfo();
            int EdgeCnt = 0;


            WeldCenterInfo MaxCandidte;
            int MaxScore = 0;
         
            //    //---------------------------------------
            for (int mmj = 0; mmj < GridNum; mmj++)
                for (int mmi = 0; mmi < ColumnCnt; mmi++)
                {
                    MaxCandidte = new WeldCenterInfo();
                    MaxCandidte.WeldScore = 0;
                    MaxScore = 0;
                    if (!GoodWeldCircleP.Exists(p => p.X == MapGrid2[mmi, mmj].X) && !(GoodWeldCircleP.Exists(p => p.Y == MapGrid2[mmi, mmj].Y)))


                    //沒有在理想list的網格點
                    {

                    //    Console.WriteLine(" MapGrid2[" + mmi + "," + mmj + "]" + MapGrid2[mmi, mmj].X + "," + MapGrid2[mmi, mmj].Y);


                        for (int j = (int)(MapGrid2[mmi, mmj].Y - 15); j < (int)(MapGrid2[mmi, mmj].Y + 15); j++)
                        {
                            for (int i = (int)(MapGrid2[mmi, mmj].X - 15); i < (int)(MapGrid2[mmi, mmj].X + 15); i++)
                            {


                                EdgeCnt = 0;

                                //算出每個點專屬的EGDE範圍 
                                foreach (System.Drawing.Point p in TemplateEdgeList)//每一個 template 的 edge 點
                                {
                                    PointF TT = new PointF();
                                    TT.X = p.X + (i - 37);
                                    TT.Y = p.Y + (j - 37);

                                    if (TT.X > 0 && TT.Y > 0 && TT.X < 2048 && TT.Y < 2048)
                                        if (ProcessData[(int)TT.X, (int)TT.Y, 0] == 0) //TEMPLATE edge 點位移至當前位置, 判斷是否也為邊緣點(黑色)
                                        {
                                            EdgeCnt++;

                                        }


                                }

                                if (EdgeCnt > MaxScore && EdgeCnt > TemplateEdgeList.Count() * 0.95)//一半以上銲道點符合
                                {

                                    MaxScore = EdgeCnt;
                                    MaxCandidte.WeldScore = EdgeCnt;
                                    MaxCandidte.WeldCandidate.X = i;
                                    MaxCandidte.WeldCandidate.Y = j;

                                }




                            }//FOR I
                        }//FOR J


                        if (!(MaxCandidte.WeldCandidate.X == MapGrid2[mmi, mmj].X && MaxCandidte.WeldCandidate.Y == MapGrid2[mmi, mmj].Y))
                        {


                            WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                            WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;
                            WeldCenterList[mmi, mmj].WeldScore = MaxCandidte.WeldScore;

                        }

                    }//end if not goodlist

                    else //理想銲道則不用尋找,直接用原本的
                    {
                        MaxCandidte.WeldScore = 100000;
                        MaxCandidte.WeldCandidate.X = (int)MapGrid2[mmi, mmj].X;
                        MaxCandidte.WeldCandidate.Y = (int)MapGrid2[mmi, mmj].Y;
                        WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                        WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;
                        WeldCenterList[mmi, mmj].WeldScore = MaxCandidte.WeldScore;

                    }


                    if (MaxCandidte.WeldScore == 0)
                    {
                        MaxCandidte.WeldCandidate.X = (int)MapGrid2[mmi, mmj].X;
                        MaxCandidte.WeldCandidate.Y = (int)MapGrid2[mmi, mmj].Y;
                        WeldCenterList[mmi, mmj].WeldCandidate.X = MaxCandidte.WeldCandidate.X;
                        WeldCenterList[mmi, mmj].WeldCandidate.Y = MaxCandidte.WeldCandidate.Y;

                    }
                }//for map



            Pen BluePen = new Pen(Color.Blue, 5);
            Pen GreenPen = new Pen(Color.Green, 10);
            System.Drawing.SolidBrush GreenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green);//画刷
            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷

            Bitmap DrawBitmap = new Bitmap(OriBitmap);
            BlackCircleP = new List<PointF>();

            Graphics g = Graphics.FromImage(DrawBitmap);

            System.Drawing.Point FinalDefectP = new System.Drawing.Point();
            FinalDefectPList = new List<System.Drawing.Point>();

            for (int j = 0; j < GridNum; j++)
                for (int i = 0; i < ColumnCnt; i++)
                {

                    if (WeldCenterList[i, j].WeldCandidate.X != 0 && WeldCenterList[i, j].WeldCandidate.Y != 0)//邊緣點濾掉
                    {
                        //處理找不到者 :０分，　座標-1
                        string drawString = "";
                        System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 30);
                        System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);
                        float x = 150.0F;
                        float y = 50.0F;
                        System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();



                        if (WeldCenterList[i, j].WeldScore < 9999)
                        {
                            if (WeldCenterList[i, j].WeldScore == 0)//真正defect
                            {
                                drawString = "x";
                                FinalDefectP.X = i;
                                FinalDefectP.Y = j;
                                FinalDefectPList.Add(FinalDefectP);
                            }
                            else
                            {
                                drawString = WeldCenterList[i, j].WeldScore.ToString();
                            }
                            g.DrawString(drawString, drawFont, drawBrush, WeldCenterList[i, j].WeldCandidate.X - 30, WeldCenterList[i, j].WeldCandidate.Y + 40, drawFormat);
                        }

                        if (WeldCenterList[i, j].WeldScore != 0)
                        {
                            g.FillEllipse(GreenBrush, WeldCenterList[i, j].WeldCandidate.X - 15, WeldCenterList[i, j].WeldCandidate.Y - 15, 30, 30);

                        }
                    }

                }
            pictureBox2.Image = DrawBitmap;


            //寫檔案
            string SavePath = @"D:\\慧萱\\Code\\A+\\2021\\燃料電池\\20210721\\test-";
            string FileIdx = ImgFileName.Substring(ImgFileName.LastIndexOf("_") + 1, ImgFileName.LastIndexOf(".") - ImgFileName.LastIndexOf("_") - 1);
            StreamWriter sw = new StreamWriter(SavePath + FileIdx + ".txt", false);
            sw.WriteLine("VisionAreaID,PosXID,PosYID");
            if (null != sw)
            {
                foreach (System.Drawing.Point PP in FinalDefectPList)
                {
                    sw.WriteLine(FileIdx + "," + (PP.X + 1) + "," + (PP.Y + 1));

                }
            }
            sw.Close();



        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
        public class WeldCandidateScore
        {
            public PointF WeldCandidate;
            public int WeldScore;
            public System.Drawing.Point MapIdx;

        }

        private void button22_Click(object sender, EventArgs e)
        {
            int[, ,] ProcessArr = new int[OriBitmap.Width, OriBitmap.Height, 3];
            ProcessArr = BMPFast.GetRGBData(ProcessBitmap);


            Bitmap DrawBmp = new Bitmap(OriBitmap);
            Graphics g2 = Graphics.FromImage(DrawBmp);
            Pen RedPen = new Pen(Color.Red, 3);

            int BlackCnt = 0;

            List<WeldCenterInfo> AreaList = new List<WeldCenterInfo>();
            WeldCenterInfo PP = new WeldCenterInfo();

            for (int j = 0; j < 7; j++)
                for (int i = 0; i < 7; i++)
                {
                    PP = new WeldCenterInfo();
                    PP.WeldCandidate.X = 1024 - 40 * i;
                    PP.WeldCandidate.Y = 1024 - 40 * j;
                    AreaList.Add(PP);
                }

            int count = 0;
            foreach (WeldCenterInfo pp in AreaList)
            {
                BlackCnt = 0;
                for (int y = (int)pp.WeldCandidate.Y; y < pp.WeldCandidate.Y + 40; y++)
                {
                    for (int x = (int)pp.WeldCandidate.X; x < pp.WeldCandidate.X + 40; x++)
                    {
                        if (ProcessArr[x, y, 0] == 0)
                            BlackCnt++;
                    }

                }

                AreaList[count].WeldScore = BlackCnt;
                count++;
            }

            AreaList.Sort((p1, p2) => p2.WeldScore.CompareTo(p1.WeldScore));



            System.Drawing.SolidBrush RedBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷
            System.Drawing.SolidBrush BlueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);//画刷

            PointF CoarseCenter = new PointF();
            int cnt = 0;
            foreach (WeldCenterInfo pp in AreaList)
            {

                //  g2.FillEllipse(RedBrush, pp.WeldCandidate.X - 5, pp.WeldCandidate.Y - 5, 10, 10);
                if (pp.WeldScore > 40 * 40 * 0.2)
                {
                    cnt++;
                    //g2.FillEllipse(BlueBrush, pp.WeldCandidate.X - 5, pp.WeldCandidate.Y - 5, 10, 10);
                    CoarseCenter.X += (pp.WeldCandidate.X + 20);
                    CoarseCenter.Y += (pp.WeldCandidate.Y + 20);
                }

            }
            CoarseCenter.X = CoarseCenter.X / cnt;
            CoarseCenter.Y = CoarseCenter.Y / cnt;
            //--------------------------------此範圍搜尋圓心點-------------------------------------------------------
            BlackCnt = 0;
            int MaxBlackCnt = 0;
            PointF MaxP = new PointF();
            int LOWboundX = (int)CoarseCenter.X - 60;
            int UPboundX = (int)CoarseCenter.X + 60;
            int LOWboundY = (int)CoarseCenter.Y - 60;
            int UPboundY = (int)CoarseCenter.Y + 60;

            for (int n = LOWboundY; n < UPboundY; n++)
                for (int m = LOWboundX; m < UPboundX; m++)
                {
                    BlackCnt = 0;
                    for (int j = n - 45; j < n + 45; j++)
                        for (int i = m - 45; i < m + 45; i++)
                        {
                            if (ProcessArr[i, j, 0] == 0)
                                BlackCnt++;
                        }
                    if (BlackCnt > 45 * 45 * 0.3 && MaxBlackCnt < BlackCnt)
                    {
                        MaxBlackCnt = BlackCnt;
                        MaxP.X = m;
                        MaxP.Y = n;

                    }
                }
            //-------------------------------------------------------------------------------------------------------
            PointF pf = new PointF();
            BlackCircleP = new List<PointF>();

            for (int j = -3; j < 4; j++)
                for (int i = -3; i < 4; i++)
                {
                    pf = new PointF();

                    g2.FillEllipse(BlueBrush, MaxP.X + 277 * i - 15, MaxP.Y + 277 * j - 15, 30, 30);
                    g2.FillEllipse(RedBrush, MaxP.X + 277 * i + 142 - 15, MaxP.Y + 277 * j + 137 - 15, 30, 30);
                    pf.X = MaxP.X + 277 * i + 142;
                    pf.Y = MaxP.Y + 277 * j + 137;
                    BlackCircleP.Add(pf);

                }

            //---------------------------------------------------------------------------------------
            pictureBox2.Image = DrawBmp;

        }

        private void button23_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {


                ImgFileName = openFileDialog1.FileName;
                Bitmap FileBitmap = new Bitmap(ImgFileName);

                int PF = -1;
                PixelFormat[] pixelFormatArray = {
                                                PixelFormat.Format1bppIndexed
                                                ,PixelFormat.Format4bppIndexed
                                                ,PixelFormat.Format8bppIndexed
                                                ,PixelFormat.Undefined
                                                ,PixelFormat.DontCare
                                                ,PixelFormat.Format16bppArgb1555
                                                ,PixelFormat.Format16bppGrayScale
                                            };
                foreach (PixelFormat pf in pixelFormatArray)
                {
                    if (FileBitmap.PixelFormat == pf)
                    {
                        PF = 1;
                        break;
                    }

                    else PF = 0;
                }

                if (PF == 1)
                {
                    OriBitmap2 = new Bitmap(FileBitmap.Width, FileBitmap.Height, PixelFormat.Format24bppRgb);
                    using (Graphics g = Graphics.FromImage(OriBitmap2))
                    {
                        g.DrawImage(FileBitmap, 0, 0);
                    }
                }

                else OriBitmap2 = new Bitmap(FileBitmap);


                //取得灰階影像
                Image<Gray, byte> grayImage = new Image<Gray, byte>(OriBitmap2);

                Console.WriteLine("W=" + OriBitmap2.Width);
                Console.WriteLine("H=" + OriBitmap2.Height);

                OriBitmap2 = grayImage.ToBitmap();

                OriBitmapArr2 = new int[OriBitmap2.Width, OriBitmap2.Height, 3];
                OriBitmapArr2 = BMPFast.GetRGBData(OriBitmap2);
            }
            pictureBox2.Image = OriBitmap2;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            //Image<Bgr, Byte> src = new Image<Bgr, byte>(OriBitmap); //Image Class from Emgu.CV
            //Image<Bgr, Byte> dst = new Image<Bgr, byte>(OriBitmap);


            //Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross,
            //    new Size(3, 3), new System.Drawing.Point(-1, -1));

            //CvInvoke.Erode(src, dst, element, new System.Drawing.Point(-1, -1), 3,
            //    Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));


            //ProcessBitmap = dst.ToBitmap();

            ////取得灰階影像

            //Image<Gray, byte> grayImage = new Image<Gray, byte>(ProcessBitmap);
            //ProcessBitmap = grayImage.Bitmap;
            //BradleyLocalThresholding BLfilter = new BradleyLocalThresholding();
            //ProcessBitmap = BLfilter.Apply(ProcessBitmap);


            //src = new Image<Bgr, byte>(ProcessBitmap);
            //CvInvoke.Erode(src, dst, element, new System.Drawing.Point(-1, -1), 3,
            // Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));



            //ProcessBitmap = dst.Bitmap;
            ProcessBitmap = OriBitmap;
            Image<Gray, byte> grayImage = new Image<Gray, byte>(ProcessBitmap);
            ProcessBitmap = grayImage.Bitmap;

            // create filter
            SobelEdgeDetector filter = new SobelEdgeDetector();
            // apply the filter
            filter.ApplyInPlace(ProcessBitmap);


            // create filter
            OtsuThreshold OtsuFilter = new OtsuThreshold();
            // apply the filter
            OtsuFilter.ApplyInPlace(ProcessBitmap);
            // check threshold value
            int t = OtsuFilter.ThresholdValue;


            Image<Gray, Byte> src = new Image<Gray, byte>(ProcessBitmap); //Image Class from Emgu.CV
            Image<Gray, Byte> dst = new Image<Gray, byte>(ProcessBitmap);
            CvInvoke.Threshold(src, dst, t, 255, ThresholdType.Binary);
            ProcessBitmap = dst.ToBitmap();



            Invert InvFilter = new Invert();
            // apply the filter
            InvFilter.ApplyInPlace(ProcessBitmap);

            src = new Image<Gray, byte>(ProcessBitmap); //Image Class from Emgu.CV
            dst = new Image<Gray, byte>(ProcessBitmap);


            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross,
                new Size(3, 3), new System.Drawing.Point(-1, -1));

            CvInvoke.Erode(src, dst, element, new System.Drawing.Point(-1, -1), 3,
                Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));


            ProcessBitmap = dst.ToBitmap();
            src = dst;
            CvInvoke.Erode(src, dst, element, new System.Drawing.Point(-1, -1), 3,
               Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(0, 0, 0));

            ProcessBitmap = dst.ToBitmap();
            pictureBox2.Image = ProcessBitmap;
        }

        private void button25_Click(object sender, EventArgs e)
        {
            //list for 6 files & build 總表 list
            string[] files;
            string SaveFilePath = @"D:\\慧萱\\Code\\A+\\2021\\燃料電池\\20210721";
            List<String> ProcessOKFileName = new List<String>();
            int ImageCnt = 0;
            string line = "";
            string[] sArray;
            WeldVisionTotalList = new List<WeldVisionList>();
            WeldVisionList node;

            while (true)
            {
                files = Directory.GetFiles(SaveFilePath, "*.txt");
                foreach (string file in files) //檔案
                {
                    if (!(ProcessOKFileName.Exists(OKfile => OKfile == file)))
                    {
                        StreamReader str = new StreamReader(file);

                        while ((line = str.ReadLine()) != null)
                        {
                            node = new WeldVisionList();
                            sArray = line.Split(',');

                            node.VisionAreaId = sArray[0];
                            node.VisionWeldXId = sArray[1];
                            node.VisionWeldYId = sArray[2];

                            WeldVisionTotalList.Add(node);
                        }


                        ProcessOKFileName.Add(file);
                        ImageCnt++;
                    }

                    if (ImageCnt == 6) break;
                }
                if (ImageCnt == 6) break;
            }

            string BatterySetPath = @"D:\\慧萱\\文件\\A+\\20210813_電池視覺與銲接溝通規劃\\BatterySetting2.csv";
            string setline = "";
            string VisionArea, VisionWeldXID, VisionWeldYID, WeldArea, WeldObjID, MMname;
            StreamReader swSet = new StreamReader(BatterySetPath);
            while ((setline = swSet.ReadLine()) != null)
            {
                sArray = setline.Split(',');
                VisionArea = sArray[1];
                VisionWeldXID = sArray[2];
                VisionWeldYID = sArray[3];
                WeldArea = sArray[5];
                WeldObjID = sArray[6];
                MMname = sArray[4];

                foreach (WeldVisionList pp in WeldVisionTotalList)
                {
                    if ((VisionArea == pp.VisionAreaId) && (pp.VisionWeldXId == VisionWeldXID) && (pp.VisionWeldYId == VisionWeldYID))
                    {
                        pp.MMname = MMname;
                        pp.WeldWeldObjId = WeldObjID;
                        break;
                    }
                }
            }//END WHILE



            foreach (WeldVisionList pp in WeldVisionTotalList)
            {

                Console.WriteLine(pp.VisionAreaId + "," + pp.VisionWeldXId + "," + pp.VisionWeldYId + "," + pp.MMname + "," + pp.WeldWeldObjId);


            }






        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


    }
}



public class WeldVisionList
{
    public string VisionAreaId;
    public string VisionWeldXId;
    public string VisionWeldYId;
    public string MMname;
    public string WeldWeldObjId;
}






public class WeldCenterInfo
{
    public PointF WeldCandidate;
    public int WeldScore;
    //  public System.Drawing.Point MapIdx;

}
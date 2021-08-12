using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace CrossDetectTest1
{
	class BitmapDataFast
	{
		public  int[, ,] GetRGBData(Bitmap bitImg)
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
		//高效率圖形轉換工具--由陣列設定新的Bitmap
		public Bitmap SetRGBData(int[, ,] rgbData)
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




	}
}

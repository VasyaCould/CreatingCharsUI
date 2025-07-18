using System;
using System.IO.Compression;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Numerics;// SIIIIMD
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.Wasm;
using System.Runtime.Intrinsics.X86;
using SkiaSharp;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace engine
{
    public class Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2()
        {
            this.y = 0;
            this.x = 0;
        }
    }
    public class Vector2int
    {
        public int x;
        public int y;
        public Vector2int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2int()
        {
            this.y = 0;
            this.x = 0;
        }
    }
    public class Color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public Color(byte r, byte g, byte b, byte? a = null)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a ?? (byte)255;
        }
    }
    public class PixArray
    {
        public byte[] img = new byte[4] { 0, 0, 0, 0 };
        public bool rgba;
        public int Width;
        public int Height;

        public PixArray() { }
        public PixArray(int Width, int Height, bool rgba)
        {
            img = new byte[Width * Height * (rgba ? 4 : 3)];
            this.rgba = rgba;
            this.Width = Width;
            this.Height = Height;
        }
        public PixArray(bool rgba) { this.rgba = rgba; }
        public PixArray(string path)
        {
            // try
            // {
            //     using (var image = new MagickImage(path))
            //     {
            //         Width = (int)image.Width;
            //         Height = (int)image.Height;
            //         this.rgba = image.HasAlpha;


            //         // Console.WriteLine(image.GetPixels().ToByteArray(PixelMapping.RGB).Length);

            //         if (rgba) img = image.GetPixels().ToByteArray(PixelMapping.BGRA) ?? new byte[4] { 0, 0, 0, 0 };// magic image is cringe!
            //         if(!rgba) img = image.GetPixels().ToByteArray(PixelMapping.BGR) ?? new byte[3] { 0, 0, 0};
            //     }
            // }
            // catch { }

            // using (Image<Rgba32> image = Image.Load<Rgba32>(path))
            // {
            //     Width = image.Width;
            //     Height = image.Height;
            //     img = new byte[Width * Height * 4];
            //     image.CopyPixelDataTo(img);
            // }
            using (var bmp = SKBitmap.Decode(path))
            {
                img = bmp.Bytes;
                Width = bmp.Width;
                Height = bmp.Height;
                if (bmp.AlphaType == SKAlphaType.Unpremul) rgba = true;
                else rgba = false;
                bgrToRgb(img);
            }
        }
        public static void bgrToRgb(byte[] img)
        {
            for (int i = 0, j = 0; i < img.Length; i += 4, j += 3)
            {
                byte r = img[i + 0];
                byte g = img[i + 1];
                byte b = img[i + 2];

                img[j + 0] = r;
                img[j + 1] = g;
                img[j + 2] = b;
            }
        }
        public void bgraToRgba()
        {
            for (int i = 0, j = 0; i < img.Length; i += 4, j += 3)
            {
                byte r = img[i + 0];
                byte g = img[i + 1];
                byte b = img[i + 2];
                byte a = img[i + 3];

                img[j + 0] = r;
                img[j + 1] = g;
                img[j + 2] = b;
                img[j + 3] = a;
            }
        }

        [Obsolete("(Используйте вместо него PixArray.Merge) Этот метод устарел, он занимает ~100мс. Он выводит текущий pixArray на экран напрямую заменяя его пиксели (возможно) без учета прозрачности, может быть перекрыт и быть перекрытым")]
        public void Show(Vector2int? pos = null, float? rotation = null, bool? fill = null)
        {
            if (fill == true) Array.Fill<byte>(OutputWindow.img.img, 255);
            pos = pos ?? new Vector2int(0, 0);
            for (int x = 1; x + pos.x < OutputWindow.Width && x < this.Width; x++)
            {
                for (int y = 1; y + pos.y < OutputWindow.Height && y < this.Height; y++)
                {
                    if (x + pos.x > 0 && y + pos.y > 0) OutputWindow.img.SetPixel(x + pos.x, y + pos.y, this.GetPixel(x, y));
                }
            }
        }
        /// <summary>
        /// You can use just RGB, not RGBA!!!
        /// </summary>
        /// <param name="imgToMerge"></param>
        /// <param name="imgPos"></param>
        public void MergeRGB(PixArray imgToMerge, Vector2int? imgPos = null)
        {
            imgPos = imgPos ?? new();


            int imgToMergeLayer = imgToMerge.Width * 3;
            int thisImgLayer = this.Width * 3;
            int elsToSkipX = 3 * imgPos.x;
            int numOfEls = imgToMergeLayer;
            if (elsToSkipX + numOfEls > thisImgLayer) numOfEls = thisImgLayer - elsToSkipX;
            if (numOfEls < 1) return;


            if (elsToSkipX >= 0) for (int i = imgPos.y >= 0 ? imgPos.y : 0; i - imgPos.y < imgToMerge.Height && i < this.Height; i++)
                        {
                            Array.Copy(imgToMerge.img, (i - imgPos.y) * imgToMergeLayer, this.img, i * thisImgLayer + elsToSkipX, numOfEls);
                        }

            else if (elsToSkipX * -1 < imgToMergeLayer) for (int i = imgPos.y >= 0 ? imgPos.y : 0; i - imgPos.y < imgToMerge.Height && i < this.Height; i++)
                        {
                            Array.Copy(imgToMerge.img, (i - imgPos.y) * imgToMergeLayer - elsToSkipX, this.img, i * thisImgLayer, numOfEls + elsToSkipX);
                        }
        }
        /// <summary>
        /// SIMD molodes
        /// </summary>
        /// <param name="imgToMerge"></param>
        /// <param name="imgPos"></param>
        public unsafe void MergeRGBA(PixArray imgToMerge, Vector2int? imgPos = null) // simd!!! Это не gpt!!! Это я!!!
        {
            Stopwatch sw = new();
            sw.Restart();
            imgPos = imgPos ?? new();

            if (Ssse3.IsSupported)
            {
                int vectorSize = Vector128<byte>.Count;
                

                int i = 0;

                for (; i < Math.Min(this.img.Length, imgToMerge.img.Length); i += vectorSize)
                {
                    fixed (byte* imgToMergeP = imgToMerge.img)
                    {
                        fixed (byte* thisImgP = this.img)
                        {
                            Vector128<byte> thisVect = Sse2.LoadVector128(thisImgP + i);
                            Vector128<byte> mergeVect = Sse2.LoadVector128(imgToMergeP + i);

                            Vector128<byte> alphaMask = Vector128.Create(
                                (byte)3, (byte)3, (byte)3,     // A0 A0 A0
                                (byte)7, (byte)7, (byte)7,     // A1 A1 A1
                                (byte)11, (byte)11, (byte)11,    // A2 A2 A2
                                (byte)15, (byte)15, (byte)15,    // A3 A3 A3
                                0, 0, 0, 0                       // padding to fill 16 bytes
                            );

                            Vector128<byte> rgbMask = Vector128.Create(
                                (byte)0, (byte)1, (byte)2,     // A0 A0 A0
                                (byte)4, (byte)5, (byte)6,     // A1 A1 A1
                                (byte)8, (byte)9, (byte)10,    // A2 A2 A2
                                (byte)12, (byte)13, (byte)14,    // A3 A3 A3
                                0, 0, 0, 0
                            );

                            Vector128<byte> alphaVectThis = Ssse3.Shuffle(thisVect, alphaMask);
                            Vector128<byte> alphaVectToMerge = Ssse3.Shuffle(mergeVect, alphaMask);

                            Vector128<byte> rgbThis = Ssse3.Shuffle(thisVect, rgbMask);
                            Vector128<byte> rgbMerge = Ssse3.Shuffle(mergeVect, rgbMask);


                        }
                    }
                }
            }





                sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
        public void Clear()
        {
            Array.Fill(this.img, (byte)0);
        }
        // public void rgbaToRgb()
        // {
        //     if (rgba)
        //     {
        //         byte[]? newImg = new byte[Width * Height * 3];

        //         int j = 0;
        //         for (int i = 0; i < img.Length; i++)
        //         {
        //             if ((i + 1) % 4 != 0)
        //             {
        //                 newImg[j++] = img[i];
        //                 // Console.WriteLine(newImg[j]); 
        //             }
        //         }
        //         img = newImg;
        //         newImg = null; //just clearing memory (i know that c# clears memory automatically
        //     }
        // }
        [Obsolete("Не работает")]
        public void ChangeScale(int newWidth, int newHeight)// gpt
        {
            byte[] scaled = new byte[newWidth * newHeight * (rgba ? 4 : 3)];

            float xRatio = (float)Width / newWidth;
            float yRatio = (float)Height / newHeight;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int newIndex = (y * newWidth + x) * (rgba ? 4 : 3);

                    int srcX = (int)(x * xRatio);
                    int srcY = (int)(y * yRatio);
                    int srcIndex = (srcY * Width + srcX) * (rgba ? 4 : 3);

                    for (int c = 0; c < (rgba ? 4 : 3); c++)
                    {
                        scaled[newIndex + c] = img[srcIndex + c];
                    }
                }
            }

            img = scaled;
        }

        public PixArray ScaleImageNearest(int newWidth, int newHeight)
        {
            PixArray newImg = new(newWidth, newHeight, rgba);

            float xRatio = (float)this.Width / newWidth;
            float yRatio = (float)this.Height / newHeight;
            int channels = rgba ? 4 : 3;

            for (int y = 0; y < newHeight; y++)
            {
                int srcY = (int)(y * yRatio);
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = (int)(x * xRatio);

                    int srcIndex = (srcY * this.Width + srcX) * channels;
                    int dstIndex = (y * newWidth + x) * channels;

                    for (int c = 0; c < channels; c++)
                    {
                        newImg.img[dstIndex + c] = this.img[srcIndex + c];
                    }
                }
            }

            return newImg;
        }
        public void DrawLine(Vector2int firstPoint, Vector2int endPoint, Color color)
        {
            int x = endPoint.x - firstPoint.x;
            int y = endPoint.y - firstPoint.y;
            bool xLess = Math.Abs(x) < Math.Abs(y) ? true : false;
            int steps = xLess ? Math.Abs(y) : Math.Abs(x);
            int steps2 = xLess ? x : y;
            for (int i = 0; i < steps; i++)
            {
                if (xLess)
                {
                    if (y < 0) firstPoint.y--;
                    else firstPoint.y++;
                }
                else
                {
                    if (x < 0) firstPoint.x--;
                    else firstPoint.x++;
                }
                if (steps2 != 0 && steps / steps2 != 0 && i % (steps / steps2) == 0)//steps2 != 0) if (i % (steps2 / steps) == 0)
                {
                    if (xLess)
                    {
                        if (x < 0) firstPoint.x--;
                        else firstPoint.x++;
                    }
                    else
                    {
                        if (y < 0) firstPoint.y--;
                        else firstPoint.y++;
                    }
                }
                if (firstPoint.x < OutputWindow.Width && firstPoint.x > 0 && firstPoint.y < OutputWindow.Height && firstPoint.y > 0) this.SetPixel(firstPoint.x, firstPoint.y, color);
            }
            // int x = endPoint.x - firstPoint.x;
            // int y = endPoint.y - firstPoint.y;
            // bool xLess = Math.Abs(x) < Math.Abs(y) ? true : false;
            // int steps = xLess ? y : x;
            // int steps2 = xLess ? x : y;
            // for (int i = 0; i < steps; i++)
            // {

            //     if (xLess) this.SetPixel(firstPoint.x, firstPoint.y++, color);
            //     else this.SetPixel(firstPoint.x++, firstPoint.y, color);
            //     if (i % (steps / steps2) == 0)//steps2 != 0) if (i % (steps2 / steps) == 0)
            //     {
            //         if (xLess) this.SetPixel(firstPoint.x++, firstPoint.y, color);
            //         else this.SetPixel(firstPoint.x, firstPoint.y++, color);
            //     }

            // }
        }

        public int GetIndexR(int x, int y)
        {
            return (x + Width * (y - 1)) * (rgba ? 4 : 3) - 1;
        }
        public int GetIndexG(int x, int y)
        {
            return (x + Width * (y - 1)) * (rgba ? 4 : 3);
        }
        public int GetIndexB(int x, int y)
        {
            return ((x + Width * (y - 1)) * (rgba ? 4 : 3)) + 1;
        }
        public int GetIndexA(int x, int y)
        {
            return rgba ? ((x + Width * (y - 1)) * (rgba ? 4 : 3)) + 2 : (byte)255;
        }
        public Color GetPixel(int x, int y)
        {
            try
            {
                return new Color(
                    img[GetIndexR(x, y)],
                    img[GetIndexG(x, y)],
                    img[GetIndexB(x, y)],
                    rgba ? img[GetIndexA(x, y)] : (byte)255
                );
            }
            catch { return new Color(0, 0, 0, 0); }
        }
        public void SetPixel(int x, int y, Color color)
        {
            img[GetIndexR(x, y)] = color.r;
            img[GetIndexG(x, y)] = color.g;
            img[GetIndexB(x, y)] = color.b;
            if (rgba) img[GetIndexA(x, y)] = color.a;
        }
    }
    // class Image
    // {
    //     public string directory;
    //     public float scale;
    //     public PixArray img;

    //     // public Bitmap img;

    //     // public   

    //     // modifiers
    //     public Image(string directory, float scale)
    //     {
    //         img = new Bitmap(directory);
    //         this.directory = directory;
    //         this.scale = scale;
    //     }
    //     public Image(string directory)
    //     {
    //         img = new Bitmap(directory);
    //         this.directory = directory;
    //         this.scale = 1;
    //     }
    //     public Image(float size)
    //     {
    //         this.directory = "";
    //         this.scale = size;
    //     }
    //     public Image()
    //     {
    //         this.directory = "";
    //         this.scale = 1;
    //     }
    //     //all other
    //     // public void DeleteTransparent()
    //     // {
    //         //bool ok;
    //         //Vector2Int size = new Vector2Int();
    //         //for (int i = 1; i < img.Width; i++)
    //         //{
    //         //    ok = true;
    //         //    for (int i2 = 1; i2 < img.Height; i2++)
    //         //    {
    //         //        if (img.GetPixel(i, i2) != Color.Transparent) ok = false;
    //         //    }
    //         //    // delete column
    //         //    if(ok) size.x++;

    //         //}
    //         //Bitmap newImg = new Bitmap(size.x, size.y);
    //         //size = new Vector2Int();
    //         //for (int i = 1; i < img.Height; i++)
    //         //{
    //         //    for (int i2 = 1; i2 < img.Width; i2++)
    //         //    {
    //         //        if (img.GetPixel(i, i2) != Color.Transparent) size.x++;
    //         //    }
    //         //    // delete strings
    //         //    size.y++;
    //         //    Bitmap newImg = new Bitmap(size.x, size.y);
    //         //}
    //     // }
    //     // public void ImportImg(string? directory = null)
    //     // {
    //     //     try
    //     //     {
    //     //         if (directory != null) img = new Bitmap(directory);
    //     //     }
    //     //     catch { }

    //     //     DeleteTransparent();
    //     // }
    //     public void ShowImg()
    //     {
    //         if (img != null)
    //         {
    //             for (int i = 1; i < img.Width && i < engine.OutputWindow.Width; i++)
    //             {
    //                 for (int i2 = 1; i2 < img.Height && i2 < engine.OutputWindow.Height; i2++)
    //                 {
    //                     engine.OutputWindow.img.SetPixel(x: i, y: i2, img.GetPixel(i, i2));

    //                 }
    //             }
    //         }
    //         // MainThread.form1.pictureBox1.Image = MainThread.form1.canvas;
    //         // MainThread.form1.pictureBox1.Dock = DockStyle.Fill;
    //     }
    //     // public void refresh()
    //     // {
    //     //     PixArray newImage = new(Convert.ToInt32(img.Width * scale) + 1, Convert.ToInt32(img.Height * scale) + 1, img.rgba);
    //     //     for (int iW = 1; iW < img.Width; iW++)
    //     //     {
    //     //         for (int iH = 1; iH < img.Height; iH++)
    //     //         {
    //     //             if (Convert.ToInt32(iW * scale) < newImage.Width && Convert.ToInt32(iH * scale) < newImage.Height)
    //     //             {
    //     //                 newImage.SetPixel(Convert.ToInt32(iW * scale), Convert.ToInt32(iH * scale), img.GetPixel(iW, iH));
    //     //             }

    //     //         }
    //     //     }
    //     //     img = newImage;
    //     // }
    //     // public void rotate() { }
    //     //pubilc static void cah
    // }
    // components
    
}

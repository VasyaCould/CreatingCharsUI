using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using static Win32;

namespace engine
{
    static class OutputWindow// chat gpt
    {
        public static bool scaleForbiden = true;
        public static bool fullScreen = false;
        public static string? windowName;
        // static WndProcDelegate wndProc = WndProc;

        // delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        // {
        //     return DefWindowProc(hWnd, msg, wParam, lParam);//чат gpt
        // }
        static WndProcDelegate wndProc = WndProc;

        delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private const int WM_MOUSEWHEEL = 0x020A;

        static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)// gpt
        {
            switch (msg)
            {
                case WM_MOUSEWHEEL:
                    short delta = (short)((wParam.ToInt64() >> 16) & 0xffff);
                    if (delta > 0)
                    {
                        Input.MouseWheel.up = true;
                        Input.MouseWheel.down = false;
                        Input.MouseWheel.pos = delta;
                        Input.MouseWheel.stand = false;
                    }
                    else if (delta < 0)
                    {
                        Input.MouseWheel.down = true;
                        Input.MouseWheel.up = false;
                        Input.MouseWheel.pos = delta;
                        Input.MouseWheel.stand = false;
                    }
                    break;
            }

            return DefWindowProc(hWnd, msg, wParam, lParam);
        }
        public const int Width = 1920;
        public const int Height = 1080;

        public static IntPtr hwnd;
        static IntPtr hdc;
        static IntPtr memDC;
        static IntPtr dib;
        static IntPtr old;
        // public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static PixArray img = new(Width, Height, false);
        // public static PixArray img = new("k.png");

        static IntPtr bits;


        static void Main()
        {

            MainGameClass.Start();
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            CreateWindowAndBuffer();

            Stopwatch sw = new Stopwatch();

            MSG msg;
            while (true)
            {
                sw.Restart();


                MainGameClass.Update();
                Input.MouseWheel.down = false;
                Input.MouseWheel.up = false;
                Input.MouseWheel.pos = 0;
                Input.MouseWheel.stand = true;

                while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                    if (msg.message == 0x0012)
                    {
                        PostQuitMessage(0);
                        return; // WM_QUIT
                    }
                }



                // for(int i = 0; i < img.img.Length; i++)img.img[i] = ;
                // Копируем RGB -> DIB (24 bpp, BGR)
                Marshal.Copy(img.img, 0, bits, Width * Height * 3); // copying array to GDI buffer

                BitBlt(hdc, 0, 0, Width, Height, memDC, 0, 0, 0x00CC0020); // show

                sw.Stop();
                int sleep = (int)(16 - sw.ElapsedMilliseconds);
                // Console.WriteLine(sw.ElapsedMilliseconds);
                if (sleep > 0) Thread.Sleep(sleep);
                
            }
        }
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        const int GWL_STYLE = -16;
        const long WS_THICKFRAME = 0x00040000;

        static void DisableResize(IntPtr hWnd)
        {
            IntPtr style = GetWindowLongPtr(hWnd, GWL_STYLE);
            style = new IntPtr(style.ToInt64() & ~WS_THICKFRAME);
            SetWindowLongPtr(hWnd, GWL_STYLE, style);
        }

        static void CreateWindowAndBuffer()
        {
            string className = "GDIWindow";

            WNDCLASS wc = new WNDCLASS
            {
                // lpfnWndProc = Marshal.GetFunctionPointerForDelegate((WndProc)WndProcImpl),
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc),
                hInstance = GetModuleHandle(null),
                lpszClassName = className
            };
            RegisterClass(ref wc);

            hwnd = CreateWindowEx(0, className, windowName == null ? "VasyaMolodes" : windowName, 0xCF0000, 100, 100, Width, Height, IntPtr.Zero, IntPtr.Zero, wc.hInstance, IntPtr.Zero);
            ShowWindow(hwnd, 5);
            UpdateWindow(hwnd);

            hdc = GetDC(hwnd);
            memDC = CreateCompatibleDC(hdc);

            BITMAPINFO bmi = new BITMAPINFO();
            bmi.bmiHeader.biSize = Marshal.SizeOf<BITMAPINFOHEADER>();
            bmi.bmiHeader.biWidth = Width;
            bmi.bmiHeader.biHeight = -Height; // топ-даун
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 24;
            bmi.bmiHeader.biCompression = 0; // BI_RGB

            dib = CreateDIBSection(memDC, ref bmi, 0, out bits, IntPtr.Zero, 0);
            old = SelectObject(memDC, dib);

            if (scaleForbiden && !fullScreen) DisableResize(hwnd);
            else if(fullScreen) MakeFullscreenBorderless(hwnd);
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }
        const long WS_POPUP = 0x80000000;
        const uint SWP_FRAMECHANGED = 0x0020;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_SHOWWINDOW = 0x0040;

        static readonly IntPtr HWND_TOP = IntPtr.Zero;

        public static void MakeFullscreenBorderless(IntPtr hWnd)
        {
            GetClientRect(GetDesktopWindow(), out RECT rect);

            IntPtr style = GetWindowLongPtr(hWnd, GWL_STYLE);
            style = new IntPtr((style.ToInt64() & ~0x00CF0000L) | WS_POPUP); // убираем рамки
            SetWindowLongPtr(hWnd, GWL_STYLE, style);

            SetWindowPos(hWnd, HWND_TOP, 0, 0,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top,
                SWP_FRAMECHANGED | SWP_NOZORDER | SWP_SHOWWINDOW);
        }
    
    }
}
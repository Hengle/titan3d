﻿using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MainEditor
{
    class Program
    {
#if PWindow
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif
        [STAThreadAttribute]
        static void Main(string[] args)
        {
#if PWindow
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
#endif

            WeakReference wr = Main_Impl(args);
            while (wr.IsAlive)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }

            while(EngineNS.RHI.CShaderResourceView.NumOfInstance>0)
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
            }
        }
        static bool Run = true;
        static WeakReference Main_Impl(string[] args)
        {
            var cfg = FindArgument(args, "config=");
            Console.WriteLine($"Config={cfg}");
            
            EngineNS.UEngine.StartEngine(new EngineNS.UEngine(), cfg);

            while (true)
            {
                if (EngineNS.UEngine.Instance.Tick() == false)
                    break;
            }
            
            var wr = new WeakReference(EngineNS.UEngine.Instance);
            EngineNS.UEngine.Instance.FinalCleanup();
            return wr;
        }
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public static string[] GetArguments(string[] args, string startWith, char split = '+')
        {
            var types = FindArgument(args, startWith);
            if (types != null)
            {
                return types.Split(split);
            }
            return null;
        }
    }
}

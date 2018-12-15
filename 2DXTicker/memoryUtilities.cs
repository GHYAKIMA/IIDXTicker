using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

/* memoryUtilities 1.0 */

namespace _2DXTicker
{
    internal class MemoryUtilities
    {

        #region "Imports"
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesRead);
        #endregion


        public IntPtr FindWindow(string windowTitle)
        {
            return FindWindow(null, windowTitle);
        }

        public uint GetWindowPID(IntPtr hwnd)
        {
            GetWindowThreadProcessId(hwnd, out uint processID);
            return processID;
        }

        public IntPtr OpenProcess(uint processID)
        {
            return OpenProcess(0x1F0FFF, 1, processID); ;
        }

        /* Added Exceptions */
        public IntPtr GetModuleAddress(uint processID, string moduleName)
        {
            try
            {
                foreach (ProcessModule pMod in Process.GetProcessById((int)processID).Modules)
                {
                    if (pMod.ModuleName == moduleName)
                    {
                        return pMod.BaseAddress;
                    }
                    pMod.Dispose();
                }
                PerformanceCounter.CloseSharedResources();
                GC.Collect();
            } 
            catch (Exception ex)
            {
                if (ex is System.ComponentModel.Win32Exception || ex is DllNotFoundException )
                {
                    throw new Exception(ex.Message);
                }
            }
            return IntPtr.Zero;
        }

        public byte[] ReadProcessMemory(IntPtr pHandle,IntPtr memoryAddress, uint bytesToRead, out int bytesRead)
        {
            byte[] buffer = new byte[(int)bytesToRead];
            IntPtr numberOfBytesRead = IntPtr.Zero;
            ReadProcessMemory(pHandle, memoryAddress, buffer, bytesToRead, out numberOfBytesRead);
            bytesRead = numberOfBytesRead.ToInt32();
            return buffer;
        }
    }
}






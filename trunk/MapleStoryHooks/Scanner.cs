using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace MapleStoryHooks
{
    public class Scanner
    {
        /// <summary>
        /// ReadProcessMemory
        /// 
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            UInt32 dwSize,
            ref UInt32 lpNumberOfBytesRead
            );


        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);


        private IntPtr mHandle;
        private int mSize;

        private byte[] mBuffer;

        public Scanner(IntPtr pHandle, int pSize) {
            this.mHandle = pHandle;
            this.mSize = pSize;
        }        
        
        private bool DumpMemory()
        {
            try
            {
                mBuffer = new byte[mSize];
                Marshal.Copy(mHandle, mBuffer, 0, mSize);

                return true;
            }
            catch (Exception e)
            {
                System.IO.File.WriteAllText(@"C:Users\Raz\Desktop\ASDF.txt", e.Message);
                return false;
            }
        }

        private bool MaskCheck(int nOffset, string pattern)
        {
            // Loop the pattern and compare to the mask and dump.

            for (int i = 0; i < pattern.Length; i+= 2)
            {
                string val = pattern.Substring(i, 2);

                if (val == "??")
                    continue;

                if (Convert.ToByte(val, 16) != mBuffer[nOffset + i])
                {
                    return false;
                }
            }

            return true;
        }
       
        public IntPtr FindPattern(string pattern, int nOffset)
        {
            try
            {
                if (this.mBuffer == null || this.mBuffer.Length == 0)
                {
                    if (!this.DumpMemory())
                    {
                        return IntPtr.Zero;
                    }
                }

                if (pattern.Length % 2 != 0)
                {
                    return IntPtr.Zero;
                }

                for (int i = 0; i < this.mBuffer.Length; i++)
                {
                    if (this.MaskCheck(i, pattern))
                    {
                        return new IntPtr(mHandle.ToInt32() + (i + nOffset));
                    }
                }

                return IntPtr.Zero;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

       

    }
}
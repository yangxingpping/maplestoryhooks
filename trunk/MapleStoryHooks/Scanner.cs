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

        public Scanner(int pSize)
        {
            this.mHandle = GetModuleHandle(null);
            this.mSize = pSize;
        }

        public Scanner(IntPtr pHandle, int pSize) {
            this.mHandle = pHandle;
            this.mSize = pSize;
        }

        public byte[] Buffer
        {
            get
            {
                if (mBuffer == null)
                {
                    DumpMemory();
                }
                return mBuffer;
            }
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
                return false;
            }
        }

        private bool MaskCheck(int nOffset, string pattern)
        {
            for (int i = 0; i < pattern.Length / 2; i++)
            {
                string val = pattern.Substring(i * 2, 2);

                if (val == "??")
                    continue;

                int value = Convert.ToInt32(val, 16);
                if (value != mBuffer[nOffset + i])
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
                pattern = pattern.Replace("-", "");
                pattern = pattern.Replace(" ", "");

                if (this.mBuffer == null || this.mBuffer.Length == 0)
                {
                    DumpMemory();
                }

                if (pattern.Length % 2 != 0)
                {
                    return (IntPtr)(-1);
                }


                for (int i = 0; i < this.mBuffer.Length - pattern.Length/2; i++)
                {

                    if (this.MaskCheck(i, pattern))
                    {
                        return new IntPtr(mHandle.ToInt32() + (i + nOffset));
                    }
                }
                return (IntPtr)(-2);
            }
            catch
            {
                return (IntPtr)(-3);
            }
        }

        public string FindPatterAsHex(string pattern, int nOffset)
        {
            return Convert.ToString(FindPattern(pattern, nOffset).ToInt32(), 16).ToUpper();
        }
    }
}
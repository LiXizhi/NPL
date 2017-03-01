using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NPLProject
{
    class NativeMethods
    {
        [DllImport("User32", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);
    }
}

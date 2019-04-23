using System;
using System.Runtime.InteropServices;

namespace JudgeW32.Interop
{
    /// <summary>
    /// Blittable version of Windows BOOL type. It is convenient in situations where
    /// manual marshalling is required, or to avoid overhead of regular bool marshalling.
    /// </summary>
    /// <remarks>
    /// Some Windows APIs return arbitrary integer values although the return type is defined
    /// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
    /// or bResult == BOOL.FALSE .
    /// </remarks>
    public enum BOOL : int
    {
        FALSE = 0,
        TRUE = 1,
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct SecurityAttributes
    {
        public uint nLength;
        public IntPtr lpSecurityDescriptor;
        public BOOL bInheritHandle;
    }
}

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace JudgeW32.Interop
{
    public enum StdHandle : int
    {
        Input = -10,
        Output = -11,
        Error = -12
    }

    public static partial class Kernel32
    {
        const string Dll = "kernel32.dll";

        [DllImport(Dll, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(Dll, SetLastError = true)]
        public static extern uint WaitForSingleObject(SafeProcessHandle hHandle, uint dwMilliseconds);

        [DllImport(Dll, SetLastError = true)]
        public static extern SafeFileHandle GetStdHandle(
            StdHandle nStdHandle
        );

        [DllImport(Dll, SetLastError = true)]
        public static extern bool DuplicateHandle(
            SafeProcessHandle hSourceProcessHandle,
            SafeProcessHandle hSourceHandle,
            SafeProcessHandle hTargetProcessHandle,
            out SafeWaitHandle lpTargetHandle,
            uint dwDesiredAccess,
            bool bInheritHandle,
            uint dwOptions
        );

        [DllImport(Dll, SetLastError = true)]
        public static extern bool DuplicateHandle(
            SafeProcessHandle hSourceProcessHandle,
            SafeFileHandle hSourceHandle,
            SafeProcessHandle hTargetProcessHandle,
            out SafeFileHandle lpTargetHandle,
            uint dwDesiredAccess,
            bool bInheritHandle,
            uint dwOptions
        );

        [DllImport(Dll, SetLastError = true)]
        public static extern bool CreatePipe(
            out SafeFileHandle hReadPipe,
            out SafeFileHandle hWritePipe,
            ref SecurityAttributes lpPipeAttributes,
            int nSize
        );
    }
}

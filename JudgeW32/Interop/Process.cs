using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JudgeW32.Interop
{
    public class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeThreadHandle() : base(true) { }
        public SafeThreadHandle(IntPtr handle) : base(true) { this.handle = handle; }
        public SafeThreadHandle(bool owns) : base(owns) { }
        public SafeThreadHandle(IntPtr handle, bool owns) : base(owns) { this.handle = handle; }

        protected override bool ReleaseHandle()
        {
            return Kernel32.CloseHandle(handle);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ProcessInformation
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StartupInfo
    {
        public int cb;
        public IntPtr lpReserved;
        public IntPtr lpDesktop;
        public IntPtr lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential, Size = 72)]
    public struct ProcessMemoryCounters
    {
        public uint cb;
        public uint PageFaultCount;
        public ulong PeakWorkingSetSize;
        public ulong WorkingSetSize;
        public ulong QuotaPeakPagedPoolUsage;
        public ulong QuotaPagedPoolUsage;
        public ulong QuotaPeakNonPagedPoolUsage;
        public ulong QuotaNonPagedPoolUsage;
        public ulong PagefileUsage;
        public ulong PeakPagefileUsage;
    }

    public class Psapi
    {
        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool GetProcessMemoryInfo(
            SafeProcessHandle hProcess,
            out ProcessMemoryCounters counters,
            uint size
        );
    }

    public static partial class Kernel32
    {
        [DllImport(Dll, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        public static extern bool CreateProcessW(
            string lpApplicationName,
            [In] StringBuilder lpCommandLine,
            ref SecurityAttributes procSecAttrs,
            ref SecurityAttributes threadSecAttrs,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            ref ProcessInformation lpProcessInformation
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(SafeThreadHandle hThread);
    }
}

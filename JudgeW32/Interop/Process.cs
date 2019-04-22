using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

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

    [Flags]
    public enum CreateProcessFlags : int
    {
        CreateBreakawayFromJob = 0x01000000,
        CreateDefaultErrorMode = 0x04000000,
        CreateNewConsole = 0x00000010,
        CreateNewProcessGroup = 0x00000200,
        CreateNoWindow = 0x08000000,
        CreateProtectedProcess = 0x00040000,
        CreatePreserveCodeAuthzLevel = 0x02000000,
        CreateSeparateWowVDM = 0x00000800,
        CreateSharedWowVDM = 0x00001000,
        CreateSuspended = 0x00000004,
        CreateUnicodeEnvironment = 0x00000400,
        DebugOnlyThisProcess = 0x00000002,
        DebugProcess = 0x00000001,
        DetachedProcess = 0x00000008,
        ExtendedStartupInfoPresent = 0x00080000,
        InheritParentAffinity = 0x00010000
    }

    [Flags]
    public enum StartFlag : uint
    {
        UseShowWindow = 0x00000001,
        UseSize = 0x00000002,
        UsePosition = 0x00000004,
        UseCountChars = 0x00000008,
        UseFillAttribute = 0x00000010,
        RunFullScreen = 0x00000020,
        ForceOnFeedback = 0x00000040,
        ForceOffFeedback = 0x00000080,
        UseStdHandles = 0x00000100,
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
        public StartFlag dwFlags;
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

    internal partial class HandleOptions
    {
        internal const int DuplicateSameAccess = 2;
        internal const int StillActive = 0x00000103;
        internal const int TokenAdjustPrivileges = 0x20;
    }

    public sealed class ProcessWaitHandle : WaitHandle
    {
        internal ProcessWaitHandle(SafeProcessHandle processHandle)
        {
            SafeWaitHandle waitHandle = null;
            SafeProcessHandle currentProcHandle = Interop.Kernel32.GetCurrentProcess();
            bool succeeded = Interop.Kernel32.DuplicateHandle(
                currentProcHandle,
                processHandle,
                currentProcHandle,
                out waitHandle,
                0,
                false,
                HandleOptions.DuplicateSameAccess);

            if (!succeeded)
            {
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            this.SetSafeWaitHandle(waitHandle);
        }
    }

    public static partial class Kernel32
    {
        [DllImport(Dll, SetLastError = true)]
        public static extern SafeProcessHandle GetCurrentProcess();

        [DllImport(Dll, CharSet = CharSet.Unicode, SetLastError = true, BestFitMapping = false)]
        public static unsafe extern bool CreateProcessW(
            string lpApplicationName,
            [In] StringBuilder lpCommandLine,
            SecurityAttributes *procSecAttrs,
            SecurityAttributes *threadSecAttrs,
            bool bInheritHandles,
            CreateProcessFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref StartupInfo lpStartupInfo,
            ref ProcessInformation lpProcessInformation
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(SafeThreadHandle hThread);
    }
}

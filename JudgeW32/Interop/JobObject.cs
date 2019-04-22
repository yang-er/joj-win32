using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;

namespace JudgeW32.Interop
{
    public class SafeJobObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeJobObjectHandle() : base(true) { }
        public SafeJobObjectHandle(IntPtr handle) : base(true) { this.handle = handle; }
        public SafeJobObjectHandle(bool owns) : base(owns) {}
        public SafeJobObjectHandle(IntPtr handle, bool owns) : base(owns) { this.handle = handle; }

        protected override bool ReleaseHandle()
        {
            return Kernel32.CloseHandle(handle);
        }
    }

    [Flags]
    public enum UIRestrictions : uint
    {
        Desktop = 0x00000040,
        DisplaySettings = 0x00000010,
        ExitWindows = 0x00000080,
        GlobalAtoms = 0x00000020,
        Handles = 0x00000001,
        ReadClipboard = 0x00000002,
        SystemParameters = 0x00000008,
        WriteClipboard = 0x00000004,
    }

    public struct JobObjectBasicUiRestrictions
    {
        public UIRestrictions UIRestrictionsClass;
    }

    public enum JobObjectInfoClass
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11
    }

    [Flags]
    public enum JobObjectLimit : uint
    {
        // Basic Limits
        Workingset = 0x00000001,
        ProcessTime = 0x00000002,
        JobTime = 0x00000004,
        ActiveProcess = 0x00000008,
        Affinity = 0x00000010,
        PriorityClass = 0x00000020,
        PreserveJobTime = 0x00000040,
        SchedulingClass = 0x00000080,

        // Extended Limits
        ProcessMemory = 0x00000100,
        JobMemory = 0x00000200,
        DieOnUnhandledException = 0x00000400,
        BreakawayOk = 0x00000800,
        SilentBreakawayOk = 0x00001000,
        KillOnJobClose = 0x00002000,
        SubsetAffinity = 0x00004000,

        // Notification Limits
        JobReadBytes = 0x00010000,
        JobWriteBytes = 0x00020000,
        RateControl = 0x00040000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public JobObjectLimit LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public long Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IoCounters
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobObjectExtendedLimitInformation
    {
        public JobObjectBasicLimitInformation BasicLimitInformation;
        public IoCounters IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    public static partial class Kernel32
    {
        [DllImport(Dll, SetLastError = true)]
        public static unsafe extern bool SetInformationJobObject(
            SafeJobObjectHandle hJob,
            JobObjectInfoClass JobObjectInfoClass,
            void* lpJobObjectInfo,
            int cbJobObjectInfoLength);

        [DllImport(Dll, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AssignProcessToJobObject(
            SafeJobObjectHandle hJob,
            SafeProcessHandle hProcess
        );

        [DllImport(Dll, SetLastError = true)]
        public static extern bool IsProcessInJob(
            SafeProcessHandle hProcess,
            SafeJobObjectHandle hJob,
            out bool bResult
        );

        [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static unsafe extern SafeJobObjectHandle CreateJobObjectW(
            [In] SecurityAttributes *lpJobAttributes,
            string lpName
        );

        [DllImport(Dll, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool TerminateJobObject(
            [In] SafeJobObjectHandle hJob,
            [In] uint uExitCode
        );
    }
}

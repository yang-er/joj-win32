using JudgeW32.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JudgeW32.Kernel
{
    public class JobObject : IDisposable
    {
        private readonly SafeJobObjectHandle mHandle;

        public unsafe JobObject(string name = null)
        {
            mHandle = Kernel32.CreateJobObjectW(null, name);
        }

        public void AssignProcess(SafeProcessHandle handle)
        {
            var result = Kernel32.AssignProcessToJobObject(mHandle, handle);
            if (!result) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public unsafe void SetBasicLimit(JobObjectBasicLimitInformation limit)
        {
            var result = Kernel32.SetInformationJobObject(
                mHandle,
                JobObjectInfoClass.BasicLimitInformation,
                &limit,
                sizeof(JobObjectBasicLimitInformation)
            );

            if (!result) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public unsafe void SetExtendedLimit(ref JobObjectExtendedLimitInformation limit)
        {
            var copy = limit;
            
            var result = Kernel32.SetInformationJobObject(
                mHandle,
                JobObjectInfoClass.ExtendedLimitInformation,
                &copy,
                sizeof(JobObjectExtendedLimitInformation)
            );

            if (!result) throw new Win32Exception(Marshal.GetLastWin32Error());
            limit = copy;
        }

        public unsafe void SetUIRestrictions(UIRestrictions @class)
        {
            var limit = new JobObjectBasicUiRestrictions
            {
                UIRestrictionsClass = @class
            };

            var result = Kernel32.SetInformationJobObject(
                mHandle,
                JobObjectInfoClass.BasicUIRestrictions,
                &limit,
                sizeof(JobObjectBasicUiRestrictions)
            );

            if (!result) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        public void Terminate(uint exitCode)
        {
            Kernel32.TerminateJobObject(mHandle, exitCode);
        }

        public void Dispose()
        {
            mHandle.Close();
        }
    }
}

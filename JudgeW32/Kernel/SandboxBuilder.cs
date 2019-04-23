using JudgeW32.Interop;
using System;

namespace JudgeW32.Kernel
{
    public class SandboxBuilder
    {
        private uint ProcessMemoryLimit;
        private int UserTimeLimit;
        private uint ProcessCounts;

        public JobObjectLimit LimitFlag { get; private set; }

        public UIRestrictions LimitFunction { get; private set; }

        public SandboxBuilder()
        {
            LimitFlag |= JobObjectLimit.KillOnJobClose;
            LimitFlag |= JobObjectLimit.DieOnUnhandledException;
        }

        public JobObject Build()
        {
            var jobObject = new JobObject();

            var lim = new JobObjectExtendedLimitInformation
            {
                BasicLimitInformation = new JobObjectBasicLimitInformation
                {
                    LimitFlags = LimitFlag,
                    ActiveProcessLimit = ProcessCounts,
                    PerProcessUserTimeLimit = UserTimeLimit,
                },

                ProcessMemoryLimit = new UIntPtr(ProcessMemoryLimit),
            };

            jobObject.SetExtendedLimit(ref lim);
            jobObject.SetUIRestrictions(LimitFunction);
            return jobObject;
        }

        public SandboxBuilder Memory(int memKb)
        {
            LimitFlag |= JobObjectLimit.ProcessMemory;
            ProcessMemoryLimit = (uint)memKb << 10;
            return this;
        }

        public SandboxBuilder UserTime(int timeMs)
        {
            LimitFlag |= JobObjectLimit.ProcessTime;
            UserTimeLimit = timeMs * 10000;
            return this;
        }

        public SandboxBuilder ProcessCount(int cnt)
        {
            LimitFlag |= JobObjectLimit.ActiveProcess;
            ProcessCounts = (uint)cnt;
            return this;
        }

        public SandboxBuilder ForbidSysCall(bool isNano = false)
        {
            LimitFunction = UIRestrictions.ReadClipboard
                          | UIRestrictions.WriteClipboard
                          | UIRestrictions.Handles
                          | UIRestrictions.GlobalAtoms
                          | UIRestrictions.ExitWindows
                          | UIRestrictions.SystemParameters;

            if (!isNano)
            {
                LimitFunction |= UIRestrictions.Desktop
                               | UIRestrictions.DisplaySettings;
            }

            return this;
        }
    }
}

using JudgeBase.Pipeline;
using JudgeW32.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JudgeW32.Kernel
{
    public class Process : IDisposable, IIOPort
    {
        private bool isDisposed = false;
        private readonly SafeProcessHandle proc;
        private readonly Pipe stdin, stdout, stderr;

        public Stream StandardOutput { get; }

        public Stream StandardError { get; }

        public Stream StandardInput { get; }

        public Process(SafeProcessHandle hProc)
        {
            proc = hProc ?? throw new ArgumentNullException(nameof(hProc));
        }

        public Process(SafeProcessHandle hProc,
            Pipe @in, Pipe @out, Pipe @err, Encoding enc) : this(hProc)
        {
            stdin = @in;
            stdout = @out;
            stderr = @err;

            StandardInput = @in is null ? null : stdin.GetParentAsStream();
            StandardOutput = @out is null ? null : stdout.GetParentAsStream();
            StandardError = @err is null ? null : stderr.GetParentAsStream();
        }

        public bool Terminate(int exitCode = 0)
        {
            return Kernel32.TerminateProcess(proc.DangerousGetHandle(), exitCode);
        }

        public bool WaitForExit(int exitTime = -1)
        {
            using (var wait = new ProcessWaitHandle(proc))
            {
                return wait.WaitOne(exitTime);
            }
        }

        public int GetExitCode()
        {
            if (!Kernel32.GetExitCodeProcess(proc, out int ec))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return ec;
        }

        public (long creation, long exit, long kernel, long user) GetProcessTimes()
        {
            if (!Kernel32.GetProcessTimes(proc,
                    out long t1, out long t2,
                    out long t3, out long t4))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return (t1, t2, t3, t4);
        }

        public long GetPeakMemory()
        {
            var pmc = new ProcessMemoryCounters();
            pmc.cb = Marshal.SizeOf<ProcessMemoryCounters>();
            if (!Psapi.GetProcessMemoryInfo(proc, ref pmc, pmc.cb))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return (long)pmc.PeakWorkingSetSize;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                StandardInput?.Dispose();
                StandardOutput?.Dispose();
                StandardError?.Dispose();
                stdin?.Dispose();
                stdout?.Dispose();
                stderr?.Dispose();
                proc?.Dispose();
            }
        }
    }
}

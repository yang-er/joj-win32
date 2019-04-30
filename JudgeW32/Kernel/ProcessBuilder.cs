using JudgeW32.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace JudgeW32.Kernel
{
    public class ProcessBuilder
    {
        JobObject hJobObject;
        CreateProcessFlags flags;
        string workDir, env, fileName;
        StartFlag stf;
        StringBuilder args;
        Encoding encoding;
        bool useIn, useOut, useErr;

        public ProcessBuilder()
        {
            flags |= CreateProcessFlags.CreateUnicodeEnvironment;
            stf |= StartFlag.UseStdHandles;
            encoding = Encoding.UTF8;
        }

        public ProcessBuilder AssignTo(JobObject jobObject)
        {
            hJobObject = jobObject;
            return this;
        }

        public ProcessBuilder UseEnvironment(string Env)
        {
            env = Env;
            return this;
        }

        public ProcessBuilder UseExecutable(string exe)
        {
            fileName = exe;
            return this;
        }

        public ProcessBuilder UseArgument(string args)
        {
            this.args = new StringBuilder(args);
            return this;
        }

        public ProcessBuilder UseEncoding(Encoding enc)
        {
            encoding = enc;
            return this;
        }

        public ProcessBuilder UseWorkingDir(string dir)
        {
            workDir = dir;
            return this;
        }

        public ProcessBuilder UseStdStream(
            bool @in, bool @out, bool @err)
        {
            useIn = @in;
            useOut = @out;
            useErr = @err;
            return this;
        }

        public unsafe Process Build(JobObject jobObject = null)
        {
            var jo = jobObject ?? hJobObject;
            var flg = flags;
            if (jo != null)
                flg |= CreateProcessFlags.CreateSuspended;

            var si = new StartupInfo
            {
                cb = Marshal.SizeOf<StartupInfo>(),
                dwFlags = stf
            };

            var pi = new ProcessInformation();
            Pipe stdin = null, stdout = null, stderr = null;

            // Set the redirect of standard io handles
            if ((stf & StartFlag.UseStdHandles) != 0)
            {
                SafeFileHandle useIn, useOut, useErr;

                if (this.useIn)
                {
                    stdin = new Pipe(true);
                    useIn = stdin.ChildrenHandle;
                }
                else
                {
                    useIn = Kernel32.GetStdHandle(StdHandle.Input);
                }

                if (this.useOut)
                {
                    stdout = new Pipe(false);
                    useOut = stdout.ChildrenHandle;
                }
                else
                {
                    useOut = Kernel32.GetStdHandle(StdHandle.Output);
                }

                if (this.useErr)
                {
                    stderr = new Pipe(false);
                    useErr = stderr.ChildrenHandle;
                }
                else
                {
                    useErr = Kernel32.GetStdHandle(StdHandle.Error);
                }

                si.hStdInput = useIn.DangerousGetHandle();
                si.hStdOutput = useOut.DangerousGetHandle();
                si.hStdError = useErr.DangerousGetHandle();
            }

            SecurityAttributes sc = new SecurityAttributes();
            fixed (char* envPtr = env)
            {
                var res = Kernel32.CreateProcessW(fileName, args,
                    &sc, &sc, true, flg,
                    IntPtr.Zero, workDir, ref si, ref pi);
                if (!res) throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var proc = new SafeProcessHandle(pi.hProcess, true);

            if (jo != null)
            {
                jo.AssignProcess(proc);
                Kernel32.ResumeThread(pi.hThread);
            }

            Kernel32.CloseHandle(pi.hThread);
            return new Process(proc, stdin, stdout, stderr, encoding);
        }
    }
}

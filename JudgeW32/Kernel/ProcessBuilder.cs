using JudgeW32.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace JudgeW32.Kernel
{
    public class ProcessBuilder : IDisposable
    {
        JobObject hJobObject;
        ProcessInformation pi;
        CreateProcessFlags flags;
        string workDir, env, fileName;
        StartFlag stf;
        Pipe stdin, stdout, stderr;
        AsyncStreamReader errReader, outReader;
        StreamWriter inWriter;
        StringBuilder Out, Err, args;
        bool useIn;
        SafeProcessHandle proc;
        Action<string> outRed, errRed;
        Action<string, StringBuilder> pipeRedirect;
        Encoding encoding;

        public StreamWriter Input => inWriter;

        public ProcessBuilder()
        {
            flags |= CreateProcessFlags.CreateUnicodeEnvironment;
            pipeRedirect = (s, sb) => sb.AppendLine(s);
            outRed = s => pipeRedirect(s, Out);
            errRed = s => pipeRedirect(s, Err);
        }

        #region Building A Process

        public ProcessBuilder AssignTo(JobObject jobObject)
        {
            if (proc != null) throw new InvalidOperationException();
            hJobObject = jobObject;
            flags |= CreateProcessFlags.CreateSuspended;
            return this;
        }

        public ProcessBuilder UsePipeRedirect(Action<string, StringBuilder> action)
        {
            if (proc != null) throw new InvalidOperationException();
            pipeRedirect = action ?? throw new ArgumentNullException(nameof(action));
            return this;
        }

        public ProcessBuilder UsePipeRedirect(Action<string> outPipe, Action<string> errPipe)
        {
            if (proc != null) throw new InvalidOperationException();
            outRed = outPipe ?? throw new ArgumentNullException(nameof(outPipe));
            errRed = errPipe ?? throw new ArgumentNullException(nameof(errPipe));
            return this;
        }

        public ProcessBuilder UseEnvironment(string Env)
        {
            if (proc != null) throw new InvalidOperationException();
            env = Env;
            return this;
        }

        public ProcessBuilder UseExecutable(string exe)
        {
            if (proc != null) throw new InvalidOperationException();
            fileName = exe;
            return this;
        }

        public ProcessBuilder UseArgument(string args)
        {
            if (proc != null) throw new InvalidOperationException();
            this.args = new StringBuilder(args);
            return this;
        }

        public ProcessBuilder UseEncoding(Encoding enc)
        {
            if (proc != null) throw new InvalidOperationException();
            encoding = enc;
            return this;
        }

        public ProcessBuilder UseWorkingDir(string dir)
        {
            if (proc != null) throw new InvalidOperationException();
            workDir = dir;
            return this;
        }

        public ProcessBuilder UseStdRedirect(
            bool @in, StringBuilder @out, StringBuilder err)
        {
            if (proc != null) throw new InvalidOperationException();
            stf |= StartFlag.UseStdHandles;
            useIn = @in; Out = @out; Err = err;
            return this;
        }

        #endregion

        public unsafe void Build()
        {
            var si = new StartupInfo();
            si.cb = Marshal.SizeOf<StartupInfo>();
            si.dwFlags = stf;

            if (proc != null) throw new InvalidOperationException();

            // Set the redirect of standard io handles
            if ((stf & StartFlag.UseStdHandles) != 0)
            {
                SafeFileHandle useIn, useOut, useErr;

                if (this.useIn)
                {
                    stdin = new Pipe(true);
                    useIn = stdin.ChildrenHandle;
                    inWriter = new StreamWriter(stdin.GetParentAsStream(), encoding, 4096);
                }
                else
                {
                    useIn = Kernel32.GetStdHandle(StdHandle.Input);
                }

                if (Out != null)
                {
                    stdout = new Pipe(false);
                    useOut = stdout.ChildrenHandle;
                    outReader = new AsyncStreamReader(stdout.GetParentAsStream(), outRed, encoding);
                }
                else
                {
                    useOut = Kernel32.GetStdHandle(StdHandle.Output);
                }

                if (Err != null)
                {
                    stderr = new Pipe(false);
                    useErr = stderr.ChildrenHandle;
                    errReader = new AsyncStreamReader(stderr.GetParentAsStream(), errRed, encoding);
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
                    &sc, &sc, true, flags,
                    IntPtr.Zero, workDir, ref si, ref pi);
                if (!res) throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            proc = new SafeProcessHandle(pi.hProcess, true);

            if (hJobObject != null)
            {
                hJobObject.AssignProcess(proc);
                Kernel32.ResumeThread(pi.hThread);
            }

            if ((stf & StartFlag.UseStdHandles) != 0)
            {
                outReader?.BeginReadLine();
                errReader?.BeginReadLine();
            }
        }

        public bool Terminate(int exitCode = 0)
        {
            if (proc == null) throw new InvalidOperationException();
            return Kernel32.TerminateProcess(pi.hProcess, exitCode);
        }

        public bool WaitForExit(int exitTime = -1)
        {
            if (proc == null) throw new InvalidOperationException();
            using (var wait = new ProcessWaitHandle(proc))
            {
                return wait.WaitOne(exitTime);
            }
        }

        public void Dispose()
        {
            errReader?.Dispose();
            outReader?.Dispose();
            inWriter?.Dispose();
            stdin?.Dispose();
            stdout?.Dispose();
            stderr?.Dispose();
            proc?.Dispose();
        }
    }
}

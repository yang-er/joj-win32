using JudgeW32.Interop;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;

namespace JudgeW32.Kernel
{
    public class Pipe : IDisposable
    {
        public bool InOut { get; }

        public SafeFileHandle ParentHandle { get; }

        public SafeFileHandle ChildrenHandle { get; }

        private FileStream mParentStream;

        public Pipe(bool parentInput)
        {
            InOut = parentInput;
            InternalCreatePipe(out var p, out var c, parentInput);
            ParentHandle = p;
            ChildrenHandle = c;
        }

        public FileStream GetParentAsStream()
        {
            if (mParentStream != null) return mParentStream;
            return mParentStream = new FileStream(ParentHandle,
                InOut ? FileAccess.Write : FileAccess.Read, 4096);
        }

        public void Dispose()
        {
            mParentStream?.Dispose();
            ParentHandle.Close();
            ChildrenHandle.Close();
        }

        private static void CreatePipeWithSecurityAttributes(
            out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe,
            ref SecurityAttributes lpPipeAttributes, int nSize)
        {
            bool ret = Kernel32.CreatePipe(
                out hReadPipe,
                out hWritePipe,
                ref lpPipeAttributes,
                nSize);

            if (!ret || hReadPipe.IsInvalid || hWritePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        private static void InternalCreatePipe(
            out SafeFileHandle parentHandle, out SafeFileHandle childHandle,
            bool parentInputs)
        {
            var securityAttributesParent = new Interop.SecurityAttributes();
            securityAttributesParent.bInheritHandle = Interop.BOOL.TRUE;

            SafeFileHandle hTmp = null;

            try
            {
                if (parentInputs)
                {
                    CreatePipeWithSecurityAttributes(
                        out childHandle, out hTmp,
                        ref securityAttributesParent, 0);
                }
                else
                {
                    CreatePipeWithSecurityAttributes(
                        out hTmp, out childHandle,
                        ref securityAttributesParent, 0);
                }

                // Duplicate the parent handle to be non-inheritable so that the child process 
                // doesn't have access. This is done for correctness sake, exact reason is unclear.
                // One potential theory is that child process can do something brain dead like 
                // closing the parent end of the pipe and there by getting into a blocking situation
                // as parent will not be draining the pipe at the other end anymore. 
                SafeProcessHandle currentProcHandle = Kernel32.GetCurrentProcess();

                if (!Kernel32.DuplicateHandle(
                    currentProcHandle, hTmp,
                    currentProcHandle, out parentHandle,
                    0, false, HandleOptions.DuplicateSameAccess))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (hTmp != null && !hTmp.IsInvalid)
                {
                    hTmp.Dispose();
                }
            }
        }
    }
}

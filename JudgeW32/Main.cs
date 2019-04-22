using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using BasicLimit = JudgeW32.Interop.JobObjectLimit;

namespace JudgeW32
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            Interop.Wer.AddExcludedApplication("test_memlimit.exe", false);

            using (var jo = new JobObject())
            {
                var limit = new Interop.JobObjectExtendedLimitInformation
                {
                    BasicLimitInformation = new Interop.JobObjectBasicLimitInformation
                    {
                        LimitFlags = BasicLimit.ActiveProcess
                                   | BasicLimit.DieOnUnhandledException
                                   | BasicLimit.ProcessMemory
                                   | BasicLimit.KillOnJobClose
                                   | BasicLimit.ProcessTime,
                        ActiveProcessLimit = 2,
                        PerProcessUserTimeLimit = 100000 * 10000,
                    },

                    ProcessMemoryLimit = new UIntPtr(800 << 20)
                };

                jo.SetExtendedLimit(ref limit);
                jo.SetUIRestrictions(Interop.UIRestrictions.ExitWindows);

                var si = new Interop.StartupInfo();
                si.cb = Marshal.SizeOf<Interop.StartupInfo>();

                var pi = new Interop.ProcessInformation();

                var sc = new Interop.SecurityAttributes();

                var res = Interop.Kernel32.CreateProcessW(
                    "C:\\windows\\system32\\notepad.exe", null,
                    null, null, true, Interop.CreateProcessFlags.CreateSuspended, IntPtr.Zero, null, ref si, ref pi);
                if (!res) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                //si.dwFlags = Interop.StartFlag.UseStdHandles;


                //var cmd = System.Diagnostics.Process.Start("test_memlimit.exe");
                var proc = new Microsoft.Win32.SafeHandles.SafeProcessHandle(pi.hProcess, true);
                var thread = new Interop.SafeThreadHandle(pi.hThread, true);
                jo.AssignProcess(proc);
                Interop.Kernel32.ResumeThread(thread);
                using (var wait = new Interop.ProcessWaitHandle(proc))
                {
                    wait.WaitOne(1000);
                }
                jo.Terminate(0);
                //Console.WriteLine($"Exit Code: 0x{item.ExitCode:x}");
                //Console.WriteLine($"Total Time: {item.UserProcessorTime.TotalMilliseconds}ms");
            }
        }


        private static string GetEnvironmentVariablesBlock(IDictionary<string, string> sd)
        {
            // get the keys
            string[] keys = new string[sd.Count];
            sd.Keys.CopyTo(keys, 0);

            // sort both by the keys
            // Windows 2000 requires the environment block to be sorted by the key
            // It will first converting the case the strings and do ordinal comparison.

            // We do not use Array.Sort(keys, values, IComparer) since it is only supported
            // in System.Runtime contract from 4.20.0.0 and Test.Net depends on System.Runtime 4.0.10.0
            // we workaround this by sorting only the keys and then lookup the values form the keys.
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);

            // create a list of null terminated "key=val" strings
            StringBuilder stringBuff = new StringBuilder();
            for (int i = 0; i < sd.Count; ++i)
            {
                stringBuff.Append(keys[i]);
                stringBuff.Append('=');
                stringBuff.Append(sd[keys[i]]);
                stringBuff.Append('\0');
            }
            // an extra null at the end that indicates end of list will come from the string.
            return stringBuff.ToString();
        }
    }
}

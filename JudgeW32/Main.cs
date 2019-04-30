using JudgeW32.Kernel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JudgeW32
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Interop.Wer.AddExcludedApplication("test_memlimit.exe", false);

            var sandboxBuilder = new SandboxBuilder()
                .UserTime(100000)
                .ProcessCount(2)
                .Memory(800 << 10)
                .ForbidSysCall();

            var procBuilder = new ProcessBuilder()
                // .UseStdStream(true, true, true)
                .UseWorkingDir("C:\\")
                .UseArgument("cmd /k dir")
                .UseEncoding(Encoding.GetEncoding(936))
                .UseExecutable("C:\\windows\\system32\\cmd.exe");

            using (var jo = sandboxBuilder.Build())
            using (var proc = procBuilder.Build(jo))
            {
                proc.WaitForExit(10000);
                jo.Terminate(unchecked((uint)-1));
                int code = proc.GetExitCode();
                long mem = proc.GetPeakMemory();
                var st = proc.GetProcessTimes();
                Console.WriteLine($"Exit code 0x{code:x}, peak mem {mem / 1024}k, user time {st.user/1000}ms");
            }
        }
    }
}

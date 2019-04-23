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
            var encoding = Encoding.GetEncoding(936);

            var sandboxBuilder = new SandboxBuilder()
                .UserTime(100000)
                .ProcessCount(2)
                .Memory(800 << 10)
                .ForbidSysCall();

            using (var jo = sandboxBuilder.Build())
            {
                using (var proc = new ProcessBuilder())
                {
                    proc.UsePipeRedirect((s, _) => Console.WriteLine(s))
                        .UseStdRedirect(true, new StringBuilder(), new StringBuilder())
                        .UseWorkingDir("C:\\")
                        .AssignTo(jo)
                        .UseArgument("cmd /k dir")
                        .UseEncoding(Encoding.GetEncoding(936))
                        .UseExecutable("C:\\windows\\system32\\cmd.exe")
                        .Build();

                    Task.Delay(1000).ContinueWith(t =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            proc.Input.WriteLine("Hello~");
                        }

                        proc.Input.Flush();
                        proc.Input.Close();
                    });

                    proc.WaitForExit(10000);
                    jo.Terminate(0);
                }
            }
        }
    }
}

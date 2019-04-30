using JudgeBase.Pipeline;
using JudgeW32.Pipeline;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JudgeW32
{
    public class Program
    {
        private static async Task Reading(Stream sr, CancellationToken ct)
        {
            try
            {
                //await Task.Yield();
                byte[] buf = new byte[10000];
                
                while (true)
                {
                    int len = await sr.ReadAsync(buf, 0, 10000, ct).ConfigureAwait(false);
                    Console.WriteLine(Encoding.GetEncoding(936).GetString(buf, 0, len));
                }
            }
            catch
            {

            }

        }

        private static async Task Writing(Stream sw, CancellationToken ct)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
                sb.Append("hello\n");
            var buf = Encoding.UTF8.GetBytes(sb.ToString());

            await sw.WriteAsync(buf, 0, buf.Length, ct);
            await sw.FlushAsync(ct);
            sw.Close();
        }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Interop.Wer.AddExcludedApplication("test_memlimit.exe", false);

            var sbopt = new SandboxOptions
            {
                ForbidSystemCall = true,
                MemoryLimit = 800 << 10,
                ProcessCountLimit = 1,
                TimeLimit = 1000,
                ExecutableFile = "C:\\windows\\system32\\cmd.exe",
                Arguments = "cmd /k dir",
                WorkingDirectory = "C:\\",
            };

            var ppl = new JobObjectPipeline(null);
            ppl.Options = sbopt;
            ppl.UseIOPort(Writing, Reading, Reading);

            ppl.ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}

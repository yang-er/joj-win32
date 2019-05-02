using JudgeBase.Judger;
using JudgeBase.Pipeline;
using JudgeBase.Problems;
using JudgeBase.Request;
using JudgeW32.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JudgeW32
{
    public abstract class InteractiveJudger : IJudger
    {
        readonly SemaphoreSlim _in, _out;
        CancellationTokenSource _final;
        protected readonly Queue<string> _queue;

        private CancellationToken LinkWith(CancellationToken cts)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(_final.Token, cts).Token;
        }

        public InteractiveJudger()
        {
            _out = new SemaphoreSlim(0);
            _final = new CancellationTokenSource();
            _queue = new Queue<string>();
        }

        public async Task PourInputData(Stream sw, CancellationToken cts)
        {
            await Task.Yield();
            cts = LinkWith(cts);
            var w = new StreamWriter(sw);

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    await _out.WaitAsync(cts);
                    var toWrite = _queue.Dequeue();
                    await w.WriteLineAsync(toWrite);
                    await w.FlushAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Here the operations are all cancelled.
                // So we have to catch it and let Taks.WhenAll goes.
            }
            finally
            {
                _final.Cancel();
                w.Close();
                sw.Close();
            }
        }

        public async Task FetchOutputData(Stream sr, CancellationToken cts)
        {
            await Task.Yield();
            cts = LinkWith(cts);
            var r = new StreamReader(sr);
            string ln;

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    ln = await r.ReadLineAsync();
                    await InputReceived(ln);
                }
            }
            catch (OperationCanceledException)
            {
                // Here the operations are all cancelled.
                // So we have to catch it and let Taks.WhenAll goes.
            }
            finally
            {
                _final.Cancel();
                r.Close();
                sr.Close();
            }
        }

        public void NotifyOutput(string output)
        {
            _queue.Enqueue(output);
            _out.Release();
        }

        public void CancelJudge()
        {
            _final.Cancel();
            //_final = null;
        }

        public abstract Task InputReceived(string input);

        public abstract Verdict GetFirstGlanceVerdict();

        public abstract void SetupWith(TestCase tc);
    }

    public class DemoJudger : InteractiveJudger
    {
        public DemoJudger()
        {
            NotifyOutput("233");
        }

        public override Verdict GetFirstGlanceVerdict()
        {
            throw new NotImplementedException();
        }

        public override async Task InputReceived(string ln)
        {
            Console.WriteLine("OUT: " + ln);
            if (ln.StartsWith("? "))
            {
                if (!int.TryParse(ln.Substring(2), out var _num))
                    CancelJudge();
                NotifyOutput($"{_num}");
            }
            else if (ln.StartsWith("! "))
            {
                CancelJudge();
            }

            await Task.CompletedTask;
        }

        public override void SetupWith(TestCase tc)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
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
                ExecutableFile = "a.exe",
            };

            var ppl = new JobObjectPipeline(null);
            ppl.UseJudger(new DemoJudger());
            ppl.Options = sbopt;

            ppl.ExecuteAsync().GetAwaiter().GetResult();
        }
    }
}

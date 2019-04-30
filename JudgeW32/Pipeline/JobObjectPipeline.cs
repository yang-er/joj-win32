using JudgeBase.Pipeline;
using JudgeW32.Kernel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JudgeW32.Pipeline
{
    public class JobObjectPipeline : SandboxPipeline
    {
        public JobObjectPipeline(ILogger<JobObjectPipeline> logger) : base(logger)
        {

        }

        public override async Task ExecuteAsync()
        {
            var jenv = new SandboxBuilder();

            if (Options.ForbidSystemCall)
                jenv.ForbidSysCall();
            if (Options.TimeLimit > 0)
                jenv.UserTime(Options.TimeLimit);
            if (Options.MemoryLimit > 0)
                jenv.Memory(Options.MemoryLimit);
            if (Options.ProcessCountLimit > 0)
                jenv.ProcessCount(Options.ProcessCountLimit);

            var penv = new ProcessBuilder();
            penv.UseExecutable(Options.ExecutableFile);
            if (Options.Arguments != null)
                penv.UseArgument(Options.Arguments);
            if (Options.WorkingDirectory != null)
                penv.UseWorkingDir(Options.WorkingDirectory);
            if (IOPorts.Count > 0)
                penv.UseStdStream(true, true, true);

            var cts = new CancellationTokenSource();

            using (var jobObj = jenv.Build())
            using (var proc = penv.Build(jobObj))
            {
                IOPort = proc;
                var stats = IOPorts.Concat(Middlewares)
                    .Select(func => func(cts.Token)).ToArray();

                proc.WaitForExit(Math.Max(Options.TimeLimit * 10, 10000));
                jobObj.Terminate(unchecked((uint)Interop.ErrorCode.QuotaExceeded));
                IOPort = null;
            }
        }
    }
}

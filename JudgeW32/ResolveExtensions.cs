using JudgeBase.Pipeline;
using JudgeW32.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace JudgeBase
{
    public static class ResolveForWin32Extensions
    {
        public static void UseWindowsSupport(this IServiceCollection that)
        {
            that.AddScoped<SandboxPipeline, JobObjectPipeline>();
        }
    }
}

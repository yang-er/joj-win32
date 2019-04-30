using JudgeW32.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace JudgeBase
{
    public static class ResolveForWin32Extensions
    {
        public static IServiceCollection AddWindowsPipeline(this IServiceCollection that)
        {
            return PipelineExtensions.AddPipeline<JobObjectPipeline>(that);
        }
    }
}

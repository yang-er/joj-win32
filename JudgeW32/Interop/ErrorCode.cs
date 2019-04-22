namespace JudgeW32.Interop
{
    /// <summary>
    /// 表示Windows程序出现问题的错误代码。
    /// </summary>
    public enum ErrorCode : int
    {
        /// <summary>
        /// 0xC0000094: 整数除0
        /// </summary>
        IntegerDividedByZero = -1073741676,

        /// <summary>
        /// 0xC0000374: 段错误
        /// </summary>
        SegmentFault = -1073740940,

        /// <summary>
        /// 0xC0000005: 访问越界
        /// </summary>
        AccessViolation = -1073741819,

        /// <summary>
        /// 0xC00000FD: 堆栈溢出
        /// </summary>
        StackOverflow = -1073741571,

        /// <summary>
        /// 0xC0000135: 未找到动态链接库
        /// </summary>
        DllNotFound = -1073741515,
    }
}

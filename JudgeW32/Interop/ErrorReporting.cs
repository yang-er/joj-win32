using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JudgeW32.Interop
{
    /// <summary>
    /// Windows错误报告所用API。
    /// 本质上是操作注册表HKCU或HKLM。
    /// </summary>
    public static class Wer
    {
        const string Dll = "wer.dll";
        const int E_ACCESSDENIED = -2147024891;

        [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int WerAddExcludedApplication(string pwzExeName, bool bAllUsers);

        [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int WerRemoveExcludedApplication(string pwzExeName, bool bAllUsers);

        /// <summary>
        /// 添加排除在外的程序。
        /// </summary>
        /// <param name="exeName">程序的文件名</param>
        /// <param name="allUsers">是否为全部用户添加</param>
        /// <exception cref="Win32Exception" />
        public static void AddExcludedApplication(string exeName, bool allUsers)
        {
            if (exeName is null) throw new ArgumentNullException(nameof(exeName));
            var hResult = WerAddExcludedApplication(exeName, allUsers);
            if (hResult == E_ACCESSDENIED) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        /// <summary>
        /// 删除排除在外的程序。
        /// </summary>
        /// <param name="exeName">程序的文件名</param>
        /// <param name="allUsers">是否为全部用户添加</param>
        /// <exception cref="Win32Exception" />
        public static void RemoveExcludedApplication(string exeName, bool allUsers)
        {
            if (exeName is null) throw new ArgumentNullException(nameof(exeName));
            var hResult = WerRemoveExcludedApplication(exeName, allUsers);
            if (hResult == E_ACCESSDENIED) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }
}

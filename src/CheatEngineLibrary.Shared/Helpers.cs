using System.Globalization;

namespace CheatEngine;

static partial class CheatEngineLibrary // 助手类函数
{
    /// <summary>
    /// 获取当前所有正在运行的进程列表
    /// </summary>
    /// <param name="action">委托调用每一行进程信息</param>
    public static void GetProcessList2(Action<ReadOnlySpan<char>> action)
    {
        GetProcessList(out var processes);

        var s = processes.AsSpan();
        var split = s.Split("\r\n");
        while (split.MoveNext())
        {
            var it = s[split.Current];
            action(it);
        }
    }

    /// <summary>
    /// 获取当前进程的模块列表
    /// </summary>
    /// <param name="withSystemModules">是否包含系统模块</param>
    /// <param name="action">委托调用每一行模块信息</param>
    public static void GetModuleList2(bool withSystemModules, Action<ReadOnlySpan<char>> action)
    {
        GetModuleList(withSystemModules, out var modules);

        var s = modules.AsSpan();
        var split = s.Split("\r\n");
        while (split.MoveNext())
        {
            var it = s[split.Current];
            action(it);
        }
    }

    /// <summary>
    /// 尝试从进程信息行中解析出进程标识符（PID）
    /// </summary>
    /// <param name="processListLine">进程信息行</param>
    /// <param name="pid">进程标识符（PID）</param>
    /// <returns>如果成功解析出 PID，则返回 <see langword="true"/>；否则返回 <see langword="false"/></returns>
    public static bool TryGetProcessId(ReadOnlySpan<char> processListLine, out int pid)
    {
        processListLine = processListLine.Trim();
        processListLine = processListLine.TrimStart('0');

        var index = processListLine.IndexOf('-');
        if (index > 0)
        {
            processListLine = processListLine[..index];
        }
        return int.TryParse(processListLine, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out pid);
    }

    /// <summary>
    /// 从进程信息行中提取出十六进制格式的 PID 字符串
    /// </summary>
    /// <param name="processListLine">进程信息行</param>
    /// <returns></returns>
    public static string? GetHexProcessId(ReadOnlySpan<char> processListLine)
    {
        processListLine = processListLine.Trim();

        var split = processListLine.Split('-');
        while (split.MoveNext())
        {
            var it = processListLine[split.Current];
            bool isAllAsciiHexDigit = true;
            for (int i = 0; i < it.Length; i++)
            {
                var it2 = it[i];
                if (!char.IsAsciiHexDigit(it2))
                {
                    isAllAsciiHexDigit = false;
                    break;
                }
            }

            if (isAllAsciiHexDigit && it.Length == 8)
            {
                return new string(it);
            }
        }

        return null;
    }
}

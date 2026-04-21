using System.Runtime.InteropServices;

namespace CheatEngine;

/// <summary>
/// 为 cheatengine-library 导出的公共 API 提供托管包装
/// </summary>
/// <remarks>
/// 该包装公开了进程、虚拟表和内存扫描器功能
/// </remarks>
static unsafe partial class CheatEngineLibrary // 提供 ce-lib${arch}.dll 的平台调用 (P/Invoke)
{
    const string DllName = "ce-lib";

    /// <summary>
    /// 获取当前所有正在运行的进程列表
    /// </summary>
    /// <param name="processes">接收进程列表</param>
    [LibraryImport(DllName, EntryPoint = "IGetProcessList")]
    internal static partial void GetProcessList(
        [MarshalAs(UnmanagedType.BStr)] out string processes);

    /// <summary>
    /// 获取当前进程的模块列表
    /// </summary>
    /// <param name="withSystemModules">是否包含系统模块</param>
    /// <param name="modules">接收模块列表</param>
    [LibraryImport(DllName, EntryPoint = "IGetModuleList")]
    internal static partial void GetModuleList(
        [MarshalAs(UnmanagedType.Bool)] bool withSystemModules,
        [MarshalAs(UnmanagedType.BStr)] out string modules);

    /// <summary>
    /// 打开指定 ProcessId 的进程，并清空 Virtual Cheat Table 
    /// </summary>
    /// <param name="pid">8 位十六进制字符串形式的进程标识符</param>
    [LibraryImport(DllName, EntryPoint = "IOpenProcess")]
    internal static partial void OpenProcess(
        [MarshalAs(UnmanagedType.BStr)] string pid);

    [LibraryImport("oleaut32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint SysAllocStringLen(
        char* psz,
        uint len);

    [LibraryImport(DllName, EntryPoint = "IOpenProcess")]
    private static partial void OpenProcess(ushort* pid);

    /// <inheritdoc cref="OpenProcess(string)"/>
    internal static void OpenProcess(int pid)
    {
        const int Length = 8;
        char* chars = stackalloc char[Length];

        uint value = unchecked((uint)pid);

        for (int i = Length - 1; i >= 0; i--)
        {
            uint digit = value & 0xF;
            chars[i] = (char)(digit < 10 ? '0' + digit : 'A' + (digit - 10));
            value >>= 4;
        }

        nint bstr = SysAllocStringLen(chars, 8);
        try
        {
            Buffer.MemoryCopy(chars, (void*)bstr, Length * sizeof(char), Length * sizeof(char));
            OpenProcess((ushort*)bstr);
        }
        finally
        {
            Marshal.FreeBSTR(bstr);
        }
    }

    /// <summary>
    /// 清空 Virtual Cheat Table 
    /// </summary>
    [LibraryImport(DllName, EntryPoint = "IResetTable")]
    internal static partial void ResetTable();

    /// <summary>
    /// 向 Virtual Cheat Table 中添加一段 Auto Assembler 脚本
    /// </summary>
    /// <param name="name">脚本名称</param>
    /// <param name="script">脚本文本内容</param>
    [LibraryImport(DllName, EntryPoint = "IAddScript")]
    internal static partial void AddScript(
        [MarshalAs(UnmanagedType.BStr)] string name,
        [MarshalAs(UnmanagedType.BStr)] string script);

    /// <summary>
    /// 激活或停用 Virtual Cheat Table 中的脚本或内存记录
    /// </summary>
    /// <param name="id">记录索引</param>
    /// <param name="activate">设为 <see langword="true"/> 表示激活，设为 <see langword="false"/> 表示停用</param>
    /// <remarks>
    /// 对脚本而言，激活表示注入或移除脚本；对内存记录而言，激活表示冻结或取消冻结
    /// </remarks>
    [LibraryImport(DllName, EntryPoint = "IActivateRecord")]
    internal static partial void ActivateRecord(
        int id,
        [MarshalAs(UnmanagedType.Bool)] bool activate);

    /// <summary>
    /// 从 Virtual Cheat Table 中移除脚本或地址记录
    /// </summary>
    /// <param name="id">从 0 开始的记录索引</param>
    [LibraryImport(DllName, EntryPoint = "IRemoveRecord")]
    internal static partial void RemoveRecord(
        int id);

    /// <summary>
    /// 对 Virtual Cheat Table 中所有已激活的地址应用冻结操作
    /// </summary>
    /// <remarks>
    /// 应在地址已被添加、赋值并激活后由定时器周期性调用
    /// </remarks>
    [LibraryImport(DllName, EntryPoint = "IApplyFreeze")]
    internal static partial void ApplyFreeze();

    /// <summary>
    /// 将指定地址添加到 Virtual Cheat Table 中
    /// </summary>
    /// <param name="initialaddress">格式为 $XXXXXXXXXXXXXXXX 的地址字符串</param>
    /// <param name="vartype">与该地址关联的变量类型</param>
    [LibraryImport(DllName, EntryPoint = "IAddAddressManually")]
    internal static partial void AddAddressManually(
        [MarshalAs(UnmanagedType.BStr)] string initialaddress,
        TVariableType vartype);

    /// <summary>
    /// 读取虚拟表中指定索引地址的当前值
    /// </summary>
    /// <param name="id">记录索引</param>
    /// <param name="value">接收当前值</param>
    [LibraryImport(DllName, EntryPoint = "IGetValue")]
    internal static partial void GetValue(
        int id,
        [MarshalAs(UnmanagedType.BStr)] out string value);

    /// <summary>
    /// 向虚拟表中指定索引的地址写入一个值
    /// </summary>
    /// <param name="id">记录索引</param>
    /// <param name="value">要写入的值</param>
    /// <param name="freezer">设置值时，是否将该记录按冻结项的方式更新</param>
    [LibraryImport(DllName, EntryPoint = "ISetValue")]
    internal static partial void SetValue(
        int id,
        [MarshalAs(UnmanagedType.BStr)] string value,
        [MarshalAs(UnmanagedType.Bool)] bool freezer);

    /// <summary>
    /// 读取指定地址，并返回该地址指向的值
    /// </summary>
    /// <param name="address">格式为 $XXXXXXXXXXXXXXXX 的地址字符串</param>
    /// <param name="vartype">要读取的变量类型</param>
    /// <param name="showashexadecimal">返回值是否按十六进制格式化</param>
    /// <param name="showAsSigned">返回值是否按有符号数格式化</param>
    /// <param name="bytesize">本机读取器使用的字节大小</param>
    /// <param name="value">接收格式化后的值</param>
    [LibraryImport(DllName, EntryPoint = "IProcessAddress")]
    internal static partial void ProcessAddress(
        [MarshalAs(UnmanagedType.BStr)] string address,
        TVariableType vartype,
        [MarshalAs(UnmanagedType.Bool)] bool showashexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool showAsSigned,
        int bytesize,
        [MarshalAs(UnmanagedType.BStr)] out string value);

    /// <summary>
    /// 初始化内存扫描器
    /// </summary>
    /// <param name="handle">传给本机扫描器的宿主窗口句柄</param>
    /// <remarks>
    /// 对同一个扫描器实例，这个方法只应调用一次
    /// </remarks>
    [LibraryImport(DllName, EntryPoint = "IInitMemoryScanner")]
    internal static partial void InitMemoryScanner(
        int handle);

    /// <summary>
    /// 开始一次新的扫描
    /// </summary>
    [LibraryImport(DllName, EntryPoint = "INewScan")]
    internal static partial void NewScan();

    /// <summary>
    /// 配置扫描时包含哪些内存区域
    /// </summary>
    /// <param name="scanWritable">控制是否扫描可写页面</param>
    /// <param name="scanExecutable">控制是否扫描可执行页面</param>
    /// <param name="scanCopyOnWrite">控制是否扫描写时复制页面</param>
    [LibraryImport(DllName, EntryPoint = "IConfigScanner")]
    internal static partial void ConfigScanner(
        TScanRegionPreference scanWritable,
        TScanRegionPreference scanExecutable,
        TScanRegionPreference scanCopyOnWrite);

    /// <summary>
    /// 按指定条件启动第一次内存扫描
    /// </summary>
    /// <param name="scanOption">要执行的扫描模式</param>
    /// <param name="variableType">要搜索的变量类型</param>
    /// <param name="roundingtype">浮点扫描使用的舍入模式</param>
    /// <param name="scanvalue1">主扫描值</param>
    /// <param name="scanvalue2">次扫描值，用于范围类扫描</param>
    /// <param name="startaddress">扫描起始地址，包含边界</param>
    /// <param name="stopaddress">扫描结束地址，包含边界</param>
    /// <param name="hexadecimal">扫描值是否按十六进制解释</param>
    /// <param name="binaryStringAsDecimal">二进制字符串输入是否按十进制解释</param>
    /// <param name="unicode">字符串扫描是否使用 Unicode 文本</param>
    /// <param name="casesensitive">字符串扫描是否区分大小写</param>
    /// <param name="fastscanmethod">要使用的快速扫描模式</param>
    /// <param name="fastscanparameter">快速扫描参数，例如对齐值或尾数字过滤条件</param>
    [LibraryImport(DllName, EntryPoint = "IFirstScan")]
    internal static partial void FirstScan(
        TScanOption scanOption,
        TVariableType variableType,
        TRoundingType roundingtype,
        [MarshalAs(UnmanagedType.BStr)] string scanvalue1,
        [MarshalAs(UnmanagedType.BStr)] string scanvalue2,
        [MarshalAs(UnmanagedType.BStr)] string startaddress,
        [MarshalAs(UnmanagedType.BStr)] string stopaddress,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        TFastScanMethod fastscanmethod,
        [MarshalAs(UnmanagedType.BStr)] string fastscanparameter);

    /// <summary>
    /// 按指定的下一次扫描条件继续当前扫描
    /// </summary>
    /// <param name="scanOption">要执行的下一次扫描模式</param>
    /// <param name="roundingtype">浮点扫描使用的舍入模式</param>
    /// <param name="scanvalue1">主比较值</param>
    /// <param name="scanvalue2">次比较值，用于范围类扫描</param>
    /// <param name="hexadecimal">扫描值是否按十六进制解释</param>
    /// <param name="binaryStringAsDecimal">二进制字符串输入是否按十进制解释</param>
    /// <param name="unicode">字符串扫描是否使用 Unicode 文本</param>
    /// <param name="casesensitive">字符串扫描是否区分大小写</param>
    /// <param name="percentage">比较值是否应按百分比处理</param>
    /// <param name="compareToSavedScan">是否与已保存扫描结果比较，而不是与上一次结果比较</param>
    /// <param name="savedscanname">当启用 <paramref name="compareToSavedScan"/> 时使用的已保存扫描名称</param>
    [LibraryImport(DllName, EntryPoint = "INextScan")]
    internal static partial void NextScan(
        TScanOption scanOption,
        TRoundingType roundingtype,
        [MarshalAs(UnmanagedType.BStr)] string scanvalue1,
        [MarshalAs(UnmanagedType.BStr)] string scanvalue2,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        [MarshalAs(UnmanagedType.Bool)] bool percentage,
        [MarshalAs(UnmanagedType.Bool)] bool compareToSavedScan,
        [MarshalAs(UnmanagedType.BStr)] string savedscanname);

    /// <summary>
    /// 获取当前扫描找到的地址数量
    /// </summary>
    /// <returns>找到的地址总数</returns>
    [LibraryImport(DllName, EntryPoint = "ICountAddressesFound")]
    internal static partial long CountAddressesFound();

    /// <summary>
    /// 从当前结果列表中读取一组地址和值
    /// </summary>
    /// <param name="index">要读取的结果列表索引</param>
    /// <param name="address">接收地址</param>
    /// <param name="value">接收格式化后的值</param>
    /// <remarks>
    /// 本机结果列表按 1024 个地址为一页做缓冲，并围绕请求的索引进行加载
    /// </remarks>
    [LibraryImport(DllName, EntryPoint = "IGetAddress")]
    internal static partial void GetAddress(
        long index,
        [MarshalAs(UnmanagedType.BStr)] out string address,
        [MarshalAs(UnmanagedType.BStr)] out string value);

    /// <summary>
    /// 在扫描完成后初始化结果列表
    /// </summary>
    /// <param name="vartype">结果列表公开的变量类型</param>
    /// <param name="varlength">结果列表使用的变量长度</param>
    /// <param name="hexadecimal">值是否按十六进制格式化</param>
    /// <param name="signed">值是否按有符号数格式化。</param>
    /// <param name="binaryasdecimal">二进制值是否按十进制格式化</param>
    /// <param name="unicode">字符串值是否按 Unicode 处理</param>
    /// <remarks>
    /// 这个方法只应调用一次，并且应在扫描完成回调之后调用
    /// </remarks>
    [LibraryImport(DllName, EntryPoint = "IInitFoundList")]
    internal static partial void InitFoundList(
        TVariableType vartype,
        int varlength,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool signed,
        [MarshalAs(UnmanagedType.Bool)] bool binaryasdecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode);

    /// <summary>
    /// 重置当前结果列表中的缓存值
    /// </summary>
    [LibraryImport(DllName, EntryPoint = "IResetValues")]
    internal static partial void ResetValues();

    /// <summary>
    /// 将结果列表重定位到指定索引附近，以便刷新内部分页缓存。
    /// </summary>
    /// <param name="index">要作为新基准的结果索引。</param>
    [LibraryImport(DllName, EntryPoint = "IRebaseAddressList")]
    internal static partial void RebaseAddressList(
        int index);

    /// <summary>
    /// 获取当前扫描结果关联的二进制大小
    /// </summary>
    /// <returns>本机扫描器返回的当前二进制大小</returns>
    [LibraryImport(DllName, EntryPoint = "IGetBinarySize")]
    internal static partial int GetBinarySize();
}
using System.Runtime.InteropServices;

namespace CheatEngine;

/// <summary>
/// 提供本机函数导出定义
/// </summary>
public static unsafe partial class DllExport
{
    [LibraryImport("oleaut32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint SysAllocStringLen(
        char* psz,
        uint len);

    [UnmanagedCallersOnly(EntryPoint = "GetProcessList")]
    internal static void GetProcessList(
        ushort** processes)
    {
        if (processes == null)
        {
            return;
        }

        CheatEngineLibrary.GetProcessList(processes);
    }

    [UnmanagedCallersOnly(EntryPoint = "ResetTable")]
    public static void ResetTable()
    {
        CheatEngineLibrary.ResetTable();
    }

    [UnmanagedCallersOnly(EntryPoint = "AddScript")]
    public static void AddScript(
        ushort* name,
        ushort* script)
    {
        CheatEngineLibrary.AddScript(name, script);
    }

    [UnmanagedCallersOnly(EntryPoint = "ActivateRecord")]
    public static void ActivateRecord(
        int id,
        [MarshalAs(UnmanagedType.Bool)] bool activate)
    {
        CheatEngineLibrary.ActivateRecord(id, activate);
    }

    [UnmanagedCallersOnly(EntryPoint = "RemoveRecord")]
    public static void RemoveRecord(
        int id)
    {
        CheatEngineLibrary.RemoveRecord(id);
    }

    [UnmanagedCallersOnly(EntryPoint = "ApplyFreeze")]
    public static void ApplyFreeze()
    {
        CheatEngineLibrary.ApplyFreeze();
    }

    [UnmanagedCallersOnly(EntryPoint = "AddAddressManually")]
    public static void AddAddressManually(
        ushort* initialaddress,
        TVariableType vartype)
    {
        CheatEngineLibrary.AddAddressManually(initialaddress, vartype);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetValue")]
    public static void GetValue(
        int id,
        ushort** value)
    {
        CheatEngineLibrary.GetValue(id, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "SetValue")]
    public static void SetValue(
        int id,
        ushort* value,
        [MarshalAs(UnmanagedType.Bool)] bool freezer)
    {
        CheatEngineLibrary.SetValue(id, value, freezer);
    }

    [UnmanagedCallersOnly(EntryPoint = "ProcessAddress")]
    internal static void ProcessAddress(
        ushort* address,
        TVariableType vartype,
        [MarshalAs(UnmanagedType.Bool)] bool showashexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool showAsSigned,
        int bytesize,
        ushort** value)
    {
        CheatEngineLibrary.ProcessAddress(address, vartype, showashexadecimal, showAsSigned, bytesize, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "InitMemoryScanner")]
    internal static void InitMemoryScanner(
        int handle)
    {
        CheatEngineLibrary.InitMemoryScanner(handle);
    }

    [UnmanagedCallersOnly(EntryPoint = "NewScan")]
    public static void NewScan()
    {
        CheatEngineLibrary.NewScan();
    }

    [UnmanagedCallersOnly(EntryPoint = "ConfigScanner")]
    public static void ConfigScanner(
        TScanRegionPreference scanWritable,
        TScanRegionPreference scanExecutable,
        TScanRegionPreference scanCopyOnWrite)
    {
        CheatEngineLibrary.ConfigScanner(scanWritable, scanExecutable, scanCopyOnWrite);
    }

    [UnmanagedCallersOnly(EntryPoint = "FirstScan")]
    public static void FirstScan(
        TScanOption scanOption,
        TVariableType variableType,
        TRoundingType roundingtype,
        ushort* scanvalue1,
        ushort* scanvalue2,
        ushort* startaddress,
        ushort* stopaddress,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        TFastScanMethod fastscanmethod,
        ushort* fastscanparameter)
    {
        CheatEngineLibrary.FirstScan(scanOption, variableType, roundingtype, scanvalue1, scanvalue2, startaddress, stopaddress, hexadecimal, binaryStringAsDecimal, unicode, casesensitive, fastscanmethod, fastscanparameter);
    }

    [UnmanagedCallersOnly(EntryPoint = "NextScan")]
    public static void NextScan(
        TScanOption scanOption,
        TRoundingType roundingtype,
        ushort* scanvalue1,
        ushort* scanvalue2,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        [MarshalAs(UnmanagedType.Bool)] bool percentage,
        [MarshalAs(UnmanagedType.Bool)] bool compareToSavedScan,
        ushort* savedscanname)
    {
        CheatEngineLibrary.NextScan(scanOption, roundingtype, scanvalue1, scanvalue2, hexadecimal, binaryStringAsDecimal, unicode, casesensitive, percentage, compareToSavedScan, savedscanname);
    }

    [UnmanagedCallersOnly(EntryPoint = "CountAddressesFound")]
    public static long CountAddressesFound()
    {
        return CheatEngineLibrary.CountAddressesFound();
    }

    [UnmanagedCallersOnly(EntryPoint = "GetAddress")]
    public static void GetAddress(
        long index,
        ushort** address,
        ushort** value)
    {
        CheatEngineLibrary.GetAddress(index, address, value);
    }

    [UnmanagedCallersOnly(EntryPoint = "InitFoundList")]
    public static void InitFoundList(
        TVariableType vartype,
        int varlength,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool signed,
        [MarshalAs(UnmanagedType.Bool)] bool binaryasdecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode)
    {
        CheatEngineLibrary.InitFoundList(vartype, varlength, hexadecimal, signed, binaryasdecimal, unicode);
    }

    [UnmanagedCallersOnly(EntryPoint = "ResetValues")]
    public static void ResetValues()
    {
        CheatEngineLibrary.ResetValues();
    }

    [UnmanagedCallersOnly(EntryPoint = "RebaseAddressList")]
    public static void RebaseAddressList(
        int index)
    {
        CheatEngineLibrary.RebaseAddressList(index);
    }

    [UnmanagedCallersOnly(EntryPoint = "GetBinarySize")]
    public static int GetBinarySize()
    {
        return CheatEngineLibrary.GetBinarySize();
    }
}

static unsafe partial class CheatEngineLibrary
{
    [LibraryImport(DllName, EntryPoint = "IGetProcessList")]
    internal static partial void GetProcessList(ushort** processes);

    [LibraryImport(DllName, EntryPoint = "IAddScript")]
    internal static partial void AddScript(
        ushort* name,
        ushort* script);

    [LibraryImport(DllName, EntryPoint = "IAddAddressManually")]
    internal static partial void AddAddressManually(
        ushort* initialaddress,
        TVariableType vartype);

    [LibraryImport(DllName, EntryPoint = "IGetValue")]
    internal static partial void GetValue(
        int id,
        ushort** value);

    [LibraryImport(DllName, EntryPoint = "ISetValue")]
    internal static partial void SetValue(
        int id,
        ushort* value,
        [MarshalAs(UnmanagedType.Bool)] bool freezer);

    [LibraryImport(DllName, EntryPoint = "IProcessAddress")]
    internal static partial void ProcessAddress(
        ushort* address,
        TVariableType vartype,
        [MarshalAs(UnmanagedType.Bool)] bool showashexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool showAsSigned,
        int bytesize,
        ushort** value);

    [LibraryImport(DllName, EntryPoint = "IFirstScan")]
    internal static partial void FirstScan(
        TScanOption scanOption,
        TVariableType variableType,
        TRoundingType roundingtype,
        ushort* scanvalue1,
        ushort* scanvalue2,
        ushort* startaddress,
        ushort* stopaddress,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        TFastScanMethod fastscanmethod,
        ushort* fastscanparameter);

    [LibraryImport(DllName, EntryPoint = "INextScan")]
    internal static partial void NextScan(
        TScanOption scanOption,
        TRoundingType roundingtype,
        ushort* scanvalue1,
        ushort* scanvalue2,
        [MarshalAs(UnmanagedType.Bool)] bool hexadecimal,
        [MarshalAs(UnmanagedType.Bool)] bool binaryStringAsDecimal,
        [MarshalAs(UnmanagedType.Bool)] bool unicode,
        [MarshalAs(UnmanagedType.Bool)] bool casesensitive,
        [MarshalAs(UnmanagedType.Bool)] bool percentage,
        [MarshalAs(UnmanagedType.Bool)] bool compareToSavedScan,
        ushort* savedscanname);

    [LibraryImport(DllName, EntryPoint = "IGetAddress")]
    internal static partial void GetAddress(
        long index,
        ushort** address,
        ushort** value);
}
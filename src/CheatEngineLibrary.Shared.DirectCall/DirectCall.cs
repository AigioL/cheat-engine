using System.Reflection;
using System.Runtime.InteropServices;

namespace CheatEngine;

static partial class CheatEngineLibrary // 负责加载与当前进程架构匹配的 ce-lib${arch}.dll，直接使用 Pascal 生成的本机库
{
    static readonly Lock SyncRoot = new();
    static nint libraryHandle;

    static CheatEngineLibrary()
    {
        NativeLibrary.SetDllImportResolver(typeof(CheatEngineLibrary).Assembly, ResolveLibraryImport);
    }

    static string GetPlatformLibraryName()
    {
#if TARGET_X64
        return $"{DllName}64";
#elif TARGET_X86
        return $"{DllName}32";
#else
        return Environment.Is64BitProcess ? $"{DllName}64" : $"{DllName}32";
#endif
    }

    static nint ResolveLibraryImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, DllName, StringComparison.InvariantCultureIgnoreCase))
        {
            return IntPtr.Zero;
        }

        lock (SyncRoot)
        {
            if (libraryHandle != IntPtr.Zero)
            {
                return libraryHandle;
            }

            if (NativeLibrary.TryLoad(GetPlatformLibraryName(), assembly, searchPath, out libraryHandle))
            {
                return libraryHandle;
            }

            return IntPtr.Zero;
        }
    }

    static void EnsureLoaded()
    {
        if (ResolveLibraryImport(DllName, typeof(CheatEngineLibrary).Assembly, null) == IntPtr.Zero)
        {
            throw new DllNotFoundException($"Unable to load {GetPlatformLibraryName()}.dll.");
        }
    }

    /// <summary>
    /// 加载与当前进程架构匹配的本机 Cheat Engine 库
    /// </summary>
    public static void LoadEngine()
    {
        EnsureLoaded();
    }

    /// <summary>
    /// 释放已加载的本机 Cheat Engine 库句柄
    /// </summary>
    public static void UnloadEngine()
    {
        return; // 会导致死锁，禁止卸载本机库

        //lock (SyncRoot)
        //{
        //    if (libraryHandle == IntPtr.Zero)
        //    {
        //        return;
        //    }

        //    NativeLibrary.Free(libraryHandle);
        //    libraryHandle = IntPtr.Zero;
        //}
    }
}

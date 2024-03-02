using System.Runtime.InteropServices;
using Engine.Handlers;

namespace Survivalcraft.WindowsLauncher.HandlerImplements;

public class UtilitiesServicesCollection : IUtilitiesHandler
{
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryInfo mi);
    //Define the information structure of memory
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryInfo
    {
        public uint dwLength; //Current structure size
        public uint dwMemoryLoad; //Current memory utilization
        public ulong ullTotalPhys; //Total physical memory size
        public ulong ullAvailPhys; //Available physical memory size
        public ulong ullTotalPageFile; //Total Exchange File Size
        public ulong ullAvailPageFile; //Total Exchange File Size
        public ulong ullTotalVirtual; //Total virtual memory size
        public ulong ullAvailVirtual; //Available virtual memory size
        public ulong ullAvailExtendedVirtual; //Keep this value always zero
    }
    private static MemoryInfo GetMemoryStatus()
    {
        MemoryInfo memoryInfo = new();
        memoryInfo.dwLength = (uint)Marshal.SizeOf(memoryInfo);
        GlobalMemoryStatusEx(ref memoryInfo);
        return memoryInfo;
    }
    private static double GetMemoryAvailable()
    {
        var memoryStatus = GetMemoryStatus();
        var memoryAvailable = (long)memoryStatus.ullAvailPhys;
        return memoryAvailable;
    }
    public long GetTotalAvailableMemory() => (int)GetMemoryAvailable();
}
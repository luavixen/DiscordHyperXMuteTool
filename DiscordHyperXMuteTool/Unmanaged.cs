using System;
using System.Runtime.InteropServices;

namespace DiscordHyperXMuteTool
{
    internal static class Unmanaged
    {
        public const string DllName = "DiscordHyperXMuteMonitorUnmanaged";

        [DllImport(DllName, CharSet = CharSet.Unicode)]
        public static extern void Debug(string message);

        [DllImport(DllName)]
        public static extern IntPtr Allocate(IntPtr size);
        [DllImport(DllName)]
        public static extern void Free(IntPtr pointer);

        [DllImport(DllName)]
        public static extern IntPtr StringifyError(int error);

        [DllImport(DllName)]
        public static extern void SendKeyboardEvent(Keycode keycode, bool isKeyDown);

        [DllImport(DllName)]
        public static extern IntPtr InjectMonitorIntoNgenuityProcess(int processID);

        [DllImport(DllName)]
        public static extern bool ExplorerTrayIsHookValid();
        [DllImport(DllName)]
        public static extern bool ExplorerTrayRemoveHook();
        [DllImport(DllName)]
        public static extern IntPtr ExplorerTraySetHook();

        [DllImport(DllName)]
        public static extern IntPtr RegisterToolMessageWindowClass();

        public delegate bool ToolWindowProcedureDelegate(IntPtr window, int message, long wparam, long lparam);

        [DllImport(DllName)]
        public static extern IntPtr CreateToolMessageWindow(ToolWindowProcedureDelegate procedure, out IntPtr window);
        [DllImport(DllName)]
        public static extern bool DestroyToolMessageWindow(IntPtr window);

        public static void ConvertError(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
            {
                string message = Marshal.PtrToStringUni(pointer); Free(pointer);
                throw new Exception(message.Trim());
            }
        }
    }
}

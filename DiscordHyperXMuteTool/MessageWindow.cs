using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static DiscordHyperXMuteTool.Unmanaged;

namespace DiscordHyperXMuteTool
{
    internal sealed class MessageWindow : IDisposable
    {
        static MessageWindow()
        {
            ConvertError(RegisterToolMessageWindowClass());
        }

        private readonly IntPtr _window;
        private readonly ToolWindowProcedureDelegate _procedure;

        public MessageWindow()
        {
            ConvertError(CreateToolMessageWindow(_procedure = OnMessage, out _window));
        }

        ~MessageWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            DestroyToolMessageWindow(_window);
            GC.SuppressFinalize(this);
        }

        private static void ExecuteOnMainThread(Action action)
        {
            var timer = new Timer();
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                timer.Dispose();
                var actionDelegate = action;
                action = null;
                actionDelegate.Invoke();
            };
            timer.Interval = 1;
            timer.Start();
        }

        private const int PingMessage = 0x96c0;
        private const int MicrophoneMessage = 0x96c1;

        [Flags]
        private enum PingFlags : long
        {
            None = 0,
            PingFromNgenuity = 0b000010,
            PingFromExplorer = 0b000011,
            PingMicrophoneDisconnected = 0b001000,
            PingMicrophoneConnected = 0b001100,
            PingMicrophoneUnmuted = 0b100000,
            PingMicrophoneMuted = 0b110000,
        }

        private const int WM_COPYDATA = 0x004a;

        private const long NotifyIconDataType = 0x6a85;

        private bool OnMessage(IntPtr window, int message, long wparam, long lparam)
        {
            switch (message)
            {
            case PingMessage:
                OnPingMessage(wparam, lparam);
                return true;
            case MicrophoneMessage:
                OnMicrophoneMessage(wparam, lparam);
                return true;
            case WM_COPYDATA:
                OnCopyDataMessage(wparam, lparam);
                return true;
            default:
                return false;
            }
        }

        private void OnPingMessage(long wparam, long lparam)
        {
            // wparam is the sender's process ID
            // lparam is a bitfield that contains:
            // PingFromNgenuity | PingFromExplorer - which monitor sent the ping?
            // PingMicrophoneDisconnected | PingMicrophoneConnected - is the microphone connected?
            // PingMicrophoneUnmuted | PingMicrophoneMuted - is the microphone muted?
            //
            // If lparam doesn't have a valid structure (eg. it's 0), ignore it!
            // Ngenuity pings must have the microphone fields
            // Explorer pings should have ONLY the PingFromExplorer field
            //
            // Note that pings are only sent by the injected Ngenuity monitor, I decided against implementing it for the explorer hook

            if (wparam == 0 || lparam == 0)
            {
                return;
            }

            var flags = (PingFlags)lparam;

            if (flags.HasFlag(PingFlags.PingFromNgenuity))
            {
                bool isMicrophoneConnected;
                if (flags.HasFlag(PingFlags.PingMicrophoneConnected))
                {
                    isMicrophoneConnected = true;
                }
                else if (flags.HasFlag(PingFlags.PingMicrophoneDisconnected))
                {
                    isMicrophoneConnected = false;
                }
                else
                {
                    // Missing microphone connection status; ignore the message
                    return;
                }

                bool isMicrophoneMuted;
                if (flags.HasFlag(PingFlags.PingMicrophoneMuted))
                {
                    isMicrophoneMuted = true;
                }
                else if (flags.HasFlag(PingFlags.PingMicrophoneUnmuted))
                {
                    isMicrophoneMuted = false;
                }
                else
                {
                    // Missing microphone mute status; ignore the message
                    return;
                }

                ExecuteOnMainThread(() =>
                {
                    Program.Manager.HandleNgenuityPing((int)wparam, isMicrophoneConnected, isMicrophoneMuted);
                });
            }
        }

        private void OnMicrophoneMessage(long wparam, long lparam)
        {
            // wparam is the microphone's device GUID compressed into a long
            // lparam is either 0 for unmuted or 1 for muted
            // If wparam is 0 or lparam is not 0 or 1, ignore it!

            if (wparam == 0 || !(lparam == 0 || lparam == 1))
            {
                return;
            }

            ExecuteOnMainThread(() =>
            {
                Program.Manager.HandleNgenuityMicrophoneMuted(wparam, lparam != 0);
            });
        }

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private void OnCopyDataMessage(long wparam, long lparam)
        {
            // wparam is 0/NULL always
            // lparam is a pointer to a COPYDATASTRUCT
            // If lparam is 0 or the COPYDATASTRUCT is invalid, ignore it!
            //
            // This message is sent by the explorer tray hook whenever Discord changes its tray icon
            // The COPYDATASTRUCT contains a valid BMP bitmap image of the tray icon in 24 or 32-bit RGB format
            // The dwData field is always NotifyIconDataType (ignore if it's not)

            if (wparam != 0 || lparam == 0)
            {
                return;
            }

            COPYDATASTRUCT copyDataDetails = Marshal.PtrToStructure<COPYDATASTRUCT>(new IntPtr(lparam));

            if (copyDataDetails.dwData != (IntPtr)NotifyIconDataType)
            {
                return;
            }
            if (copyDataDetails.cbData <= 0 || copyDataDetails.lpData == IntPtr.Zero)
            {
                return;
            }

            var iconBitmapBytes = new byte[copyDataDetails.cbData];
            Marshal.Copy(copyDataDetails.lpData, iconBitmapBytes, 0, copyDataDetails.cbData);

            ExecuteOnMainThread(() =>
            {
                Program.Manager.HandleDiscordNotifyIconChange(iconBitmapBytes);
            });
        }
    }
}

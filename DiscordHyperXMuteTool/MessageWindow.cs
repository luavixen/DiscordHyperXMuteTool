using System;
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

        private const int PingMessage = 0x96c0;
        private const int MicrophoneMessage = 0x96c1;
        
        private const long PingFromNgenuity           = 0b000010;
        private const long PingFromExplorer           = 0b000011;
        private const long PingMicrophoneDisconnected = 0b001000;
        private const long PingMicrophoneConnected    = 0b001100;
        private const long PingMicrophoneUnmuted      = 0b100000;
        private const long PingMicrophoneMuted        = 0b110000;

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

            Debug($"Ping: {wparam} 0x{lparam:X16}\n");

            // TODO: Implement handling of ping messages, update the state accordingly and tie this code into a separate system that issue keypresses as appropriate
        }

        private void OnMicrophoneMessage(long wparam, long lparam)
        {
            // wparam is the microphone's device GUID compressed into a long
            // lparam is either 0 for unmuted or 1 for muted
            // If wparam is 0 or lparam is not 0 or 1, ignore it!

            Debug($"Microphone: 0x{wparam:X16} 0x{lparam:X16}\n");

            // TODO: Tie this into a system that manages the state properly and issues keypresses as appropriate, replace this placeholder code
            Program.State.MicrophoneStatus = lparam != 0 ? MicrophoneStatus.Muted : MicrophoneStatus.Unmuted;
            Program.State.Update();
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

            Debug($"WM_COPYDATA: 0x{wparam:X16} 0x{lparam:X16}\n");

            // TODO: Copy the tray icon image from the COPYDATASTRUCT into a byte[] then asynchronously compare it to the known Discord tray icon images
            // TODO: This should be part of the same central system that controls state updates and issues keypresses as previously mentioned
        }
    }
}

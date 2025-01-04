using System;
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
        private const int NotifyIconMessage = 0x96c2;
        
        private const long PingFromNgenuity           = 0b000010;
        private const long PingFromExplorer           = 0b000011;
        private const long PingMicrophoneDisconnected = 0b001000;
        private const long PingMicrophoneConnected    = 0b001100;
        private const long PingMicrophoneUnmuted      = 0b100000;
        private const long PingMicrophoneMuted        = 0b110000;

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
            case NotifyIconMessage:
                OnNotifyIconMessage(wparam, lparam);
                return true;
            default:
                return false;
            }
        }

        private void OnPingMessage(long wparam, long lparam)
        {
            Debug($"Ping: {wparam} {lparam:X16}\n");
        }

        private void OnMicrophoneMessage(long wparam, long lparam)
        {
            Debug($"Microphone: {wparam:X16} {lparam:X16}\n");
            Program.State.MicrophoneStatus = lparam != 0 ? MicrophoneStatus.Muted : MicrophoneStatus.Unmuted;
            Program.State.Update();
        }

        private void OnNotifyIconMessage(long wparam, long lparam)
        {
        }
    }
}

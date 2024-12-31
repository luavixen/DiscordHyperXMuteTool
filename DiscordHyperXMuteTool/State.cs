using System;
using System.Drawing;
using DiscordHyperXMuteTool.Properties;

namespace DiscordHyperXMuteTool
{
    internal enum ConnectionStatus
    {
        Unknown,
        Disconnected,
        Connecting,
        Connected,
    }
    internal enum MicrophoneStatus
    {
        Unknown,
        Disconnected,
        Muted,
        Unmuted,
    }

    internal sealed class State : Reactive<State>, IDisposable
    {
        public ConnectionStatus NgenuityStatus = ConnectionStatus.Unknown;
        public ConnectionStatus DiscordStatus = ConnectionStatus.Unknown;
        public MicrophoneStatus MicrophoneStatus = MicrophoneStatus.Unknown;

        private readonly Image _ngenuityEnabledImage, _ngenuityDisabledImage;
        private readonly Image _discordEnabledImage, _discordDisabledImage;
        private readonly Image _microphoneUnmutedImage, _microphoneMutedImage, _microphoneDisabledImage;

        public State()
        {
            _ngenuityEnabledImage = Resources.NgenuityEnabledIcon.ToBitmap();
            _ngenuityDisabledImage = Resources.NgenuityDisabledIcon.ToBitmap();
            _discordEnabledImage = Resources.DiscordEnabledIcon.ToBitmap();
            _discordDisabledImage = Resources.DiscordDisabledIcon.ToBitmap();
            _microphoneUnmutedImage = Resources.MicUnmutedIcon.ToBitmap();
            _microphoneMutedImage = Resources.MicMutedIcon.ToBitmap();
            _microphoneDisabledImage = Resources.MicDisabledIcon.ToBitmap();
        }

        public void Dispose()
        {
            _ngenuityEnabledImage?.Dispose();
            _ngenuityDisabledImage?.Dispose();
            _discordEnabledImage?.Dispose();
            _discordDisabledImage?.Dispose();
            _microphoneUnmutedImage?.Dispose();
            _microphoneMutedImage?.Dispose();
            _microphoneDisabledImage?.Dispose();
        }

        private static string GetConnectionStatusMessage(ConnectionStatus status, string name)
        {
            switch (status)
            {
                case ConnectionStatus.Unknown: return $"\u2026 {name} unknown";
                case ConnectionStatus.Disconnected: return $"\u2717 {name} disconnected";
                case ConnectionStatus.Connecting: return $"\u2026 {name} connecting";
                case ConnectionStatus.Connected: return $"\u2713 {name} connected";
                default: throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        private static Image GetConnectionStatusImage(ConnectionStatus status, Image enabledImage, Image disabledImage)
        {
            return status == ConnectionStatus.Connected ? enabledImage : disabledImage;
        }

        public string NgenuityStatusText => GetConnectionStatusMessage(NgenuityStatus, "NGENUITY");
        public Image NgenuityStatusImage => GetConnectionStatusImage(NgenuityStatus, _ngenuityEnabledImage, _ngenuityDisabledImage);

        public string DiscordStatusText => GetConnectionStatusMessage(DiscordStatus, "Discord");
        public Image DiscordStatusImage => GetConnectionStatusImage(DiscordStatus, _discordEnabledImage, _discordDisabledImage);

        public string MicrophoneStatusText
        {
            get
            {
                switch (MicrophoneStatus)
                {
                case MicrophoneStatus.Unknown: return "Microphone unknown";
                case MicrophoneStatus.Disconnected: return "Microphone disconnected";
                case MicrophoneStatus.Muted: return "Microphone muted";
                case MicrophoneStatus.Unmuted: return "Microphone unmuted";
                default: throw new ArgumentOutOfRangeException(nameof(MicrophoneStatus), MicrophoneStatus, null);
                }
            }
        }
        public Image MicrophoneStatusImage
        {
            get
            {
                if (MicrophoneStatus == MicrophoneStatus.Muted) return _microphoneMutedImage;
                if (MicrophoneStatus == MicrophoneStatus.Unmuted) return _microphoneUnmutedImage;
                return _microphoneDisabledImage;
            }
        }
    }
}

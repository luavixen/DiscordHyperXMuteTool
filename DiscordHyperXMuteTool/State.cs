using System;
using System.Drawing;
using DiscordHyperXMuteTool.Properties;

namespace DiscordHyperXMuteTool
{
    internal enum MicrophoneStatus
    {
        Unknown,
        Disconnected,
        Unmuted,
        Muted,
    }

    internal enum NgenuityStatus
    {
        Unknown,
        NotRunning,
        Disconnected,
        Injecting,
        Injected,
    }
    internal enum DiscordStatus
    {
        Unknown,
        NotRunning,
        Disconnected,
        Hooked,
        Normal,
        Muted,
        Defeaned,
    }

    internal sealed class State : Reactive<State>, IDisposable
    {
        public MicrophoneStatus MicrophoneStatus = MicrophoneStatus.Unknown;

        public NgenuityStatus NgenuityStatus = NgenuityStatus.Unknown;
        public DiscordStatus DiscordStatus = DiscordStatus.Unknown;

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

        public string NgenuityProgramStatusText
        {
            get
            {
                switch (NgenuityStatus)
                {
                case NgenuityStatus.Unknown: return "\u2026 NGENUITY unknown";
                case NgenuityStatus.NotRunning: return "\u2717 NGENUITY not running";
                case NgenuityStatus.Disconnected: return "\u2717 NGENUITY disconnected";
                case NgenuityStatus.Injecting: return "\u2026 NGENUITY injecting";
                case NgenuityStatus.Injected: return "\u2713 NGENUITY injected";
                default: throw new ArgumentOutOfRangeException(nameof(NgenuityStatus), NgenuityStatus, null);
                }
            }
        }
        public Image NgenuityProgramStatusImage
        {
            get
            {
                return NgenuityStatus == NgenuityStatus.Injected ? _ngenuityEnabledImage : _ngenuityDisabledImage;
            }
        }

        public string NgenuityMicStatusText
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
        public Image NgenuityMicStatusImage
        {
            get
            {
                if (MicrophoneStatus == MicrophoneStatus.Muted) return _microphoneMutedImage;
                if (MicrophoneStatus == MicrophoneStatus.Unmuted) return _microphoneUnmutedImage;
                return _microphoneDisabledImage;
            }
        }

        public string DiscordProgramStatusText
        {
            get
            {
                switch (DiscordStatus)
                {
                case DiscordStatus.Unknown: return "\u2026 Discord unknown";
                case DiscordStatus.NotRunning: return "\u2717 Discord not running";
                case DiscordStatus.Disconnected: return "\u2717 Discord disconnected";
                case DiscordStatus.Hooked:
                case DiscordStatus.Normal:
                case DiscordStatus.Muted:
                case DiscordStatus.Defeaned: return "\u2713 Discord connected";
                default: throw new ArgumentOutOfRangeException(nameof(DiscordStatus), DiscordStatus, null);
                }
            }
        }
        public Image DiscordProgramStatusImage
        {
            get
            {
                return DiscordStatus >= DiscordStatus.Hooked ? _discordEnabledImage : _discordDisabledImage;
            }
        }

        public string DiscordMicStatusText
        {
            get
            {
                switch (DiscordStatus)
                {
                case DiscordStatus.Unknown:
                case DiscordStatus.NotRunning:
                case DiscordStatus.Disconnected:
                case DiscordStatus.Hooked: return "Microphone disabled";
                case DiscordStatus.Normal: return "Microphone unmuted";
                case DiscordStatus.Muted: return "Microphone muted";
                case DiscordStatus.Defeaned: return "Microphone deafened";
                default: throw new ArgumentOutOfRangeException(nameof(DiscordStatus), DiscordStatus, null);
                }
            }
        }
        public Image DiscordMicStatusImage
        {
            get
            {
                switch (DiscordStatus)
                {
                case DiscordStatus.Unknown:
                case DiscordStatus.NotRunning:
                case DiscordStatus.Disconnected:
                case DiscordStatus.Hooked: return _microphoneDisabledImage;
                case DiscordStatus.Normal: return _microphoneUnmutedImage;
                case DiscordStatus.Muted:
                case DiscordStatus.Defeaned: return _microphoneMutedImage;
                default: throw new ArgumentOutOfRangeException(nameof(DiscordStatus), DiscordStatus, null);
                }
            }
        }
    }
}

using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;

namespace DiscordHyperXMuteTool
{
    internal class Manager : IDisposable
    {
        private Timer _timer;

        public void Start()
        {
            if (_timer != null)
            {
                throw new InvalidOperationException("Manager already started");
            }

            _timer = new Timer();
            _timer.Tick += Update;
            _timer.Interval = 3000;
            _timer.Start();

            Setup();
        }

        private Process _ngenuityProcess = null;
        private Process _discordProcess = null;
        private bool _ngenuityProcessIsRunning = false;
        private bool _discordProcessIsRunning = false;

        private int _ngenuityInjectedProcessID = -1;

        private void DisposeProcesses()
        {
            if (_ngenuityProcess != null)
            {
                _ngenuityProcess.Dispose();
                _ngenuityProcess = null;
            }
            if (_discordProcess != null)
            {
                _discordProcess.Dispose();
                _discordProcess = null;
            }
        }

        private void FindProcesses()
        {
            DisposeProcesses();

            Process ngenuityProcess = null;
            Process discordProcess = null;

            var processes = Process.GetProcesses()
                .Select(process => (Process: process, Name: process.ProcessName))
                .ToList();

            var discordProcessName = Program.Settings.DiscordProcessName;

            foreach (var (process, name) in processes)
            {
                if (ngenuityProcess == null && string.Equals(name, "NGenuity2Helper", StringComparison.OrdinalIgnoreCase))
                {
                    ngenuityProcess = process;
                }
                if (discordProcess == null && string.Equals(name, discordProcessName, StringComparison.OrdinalIgnoreCase))
                {
                    discordProcess = process;
                }
            }

            if (discordProcess == null)
            {
                foreach (var (process, name) in processes)
                {
                    if (
                        discordProcess == null &&
                        name.IndexOf(discordProcessName, StringComparison.OrdinalIgnoreCase) == 0 &&
                        name.IndexOf("MuteTool", StringComparison.OrdinalIgnoreCase) < 0
                    ) {
                        discordProcess = process;
                        break;
                    }
                }
            }

            foreach (var (process, _) in processes)
            {
                if (process != ngenuityProcess && process != discordProcess)
                {
                    process.Dispose();
                }
            }

            _ngenuityProcess = ngenuityProcess;
            _discordProcess = discordProcess;
            _ngenuityProcessIsRunning = ngenuityProcess != null && !ngenuityProcess.HasExited;
            _discordProcessIsRunning = discordProcess != null && !discordProcess.HasExited;
        }

        private void Setup()
        {
            // TODO
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            DisposeProcesses();
        }

        private void Update(object sender, EventArgs e)
        {
            FindProcesses();

            if (Program.Settings.Enabled)
            {
                if (Program.Settings.SyncWithDiscord)
                {
                    if (!Unmanaged.ExplorerTrayIsHookValid())
                    {
                        Program.Debug("Setting explorer tray hook...");
                        Unmanaged.ConvertError(Unmanaged.ExplorerTraySetHook());
                    }

                    if (_discordProcessIsRunning)
                    {
                        if (Program.State.DiscordStatus < DiscordStatus.Hooked)
                        {
                            Program.State.DiscordStatus = DiscordStatus.Hooked;
                        }
                    }
                    else
                    {
                        Program.State.DiscordStatus = DiscordStatus.NotRunning;
                    }
                }
                else
                {
                    if (Unmanaged.ExplorerTrayIsHookValid())
                    {
                        Program.Debug("Removing explorer tray hook...");
                        Unmanaged.ExplorerTrayRemoveHook();
                    }

                    Program.State.DiscordStatus =
                        _discordProcessIsRunning ? DiscordStatus.Disconnected : DiscordStatus.NotRunning;
                }

                if (_ngenuityProcessIsRunning)
                {
                    int ngenuityProcessID = _ngenuityProcess.Id;
                    if (ngenuityProcessID != _ngenuityInjectedProcessID)
                    {
                       Program.Debug($"Injecting into NGenuity process {ngenuityProcessID}...");
                        Unmanaged.ConvertError(Unmanaged.InjectMonitorIntoNgenuityProcess(ngenuityProcessID));
                        _ngenuityInjectedProcessID = ngenuityProcessID;
                    }
                    Program.State.NgenuityStatus = NgenuityStatus.Injected;
                }
                else
                {
                    Program.State.NgenuityStatus = NgenuityStatus.NotRunning;
                }
            }
            else
            {
                if (Unmanaged.ExplorerTrayIsHookValid())
                {
                    Program.Debug("Removing explorer tray hook due to being disabled...");
                    Unmanaged.ExplorerTrayRemoveHook();
                }

                Program.State.DiscordStatus =
                    _discordProcessIsRunning ? DiscordStatus.Disconnected : DiscordStatus.NotRunning;

                if (_ngenuityProcessIsRunning && _ngenuityProcess.Id == _ngenuityInjectedProcessID)
                {
                    Program.State.NgenuityStatus = NgenuityStatus.Injected;
                }
                else
                {
                    Program.State.NgenuityStatus = _ngenuityProcessIsRunning ? NgenuityStatus.Disconnected : NgenuityStatus.NotRunning;
                }
            }

            Program.State.Update();
        }

        public void HandleNgenuityPing(int processID, bool isMicrophoneConnected, bool isMicrophoneMuted)
        {
            bool? wasMicrophoneMuted = null;
            if (Program.State.MicrophoneStatus == MicrophoneStatus.Unmuted) wasMicrophoneMuted = false;
            if (Program.State.MicrophoneStatus == MicrophoneStatus.Muted) wasMicrophoneMuted = true;

            bool? isMicrophoneMutedOrNull = null;

            Program.State.NgenuityStatus = NgenuityStatus.Injected;

            if (isMicrophoneConnected)
            {
                Program.State.MicrophoneStatus = isMicrophoneMuted ? MicrophoneStatus.Muted : MicrophoneStatus.Unmuted;
                isMicrophoneMutedOrNull = isMicrophoneMuted;
            }
            else
            {
                Program.State.MicrophoneStatus = MicrophoneStatus.Disconnected;
            }

            Program.State.Update();

            if (wasMicrophoneMuted != isMicrophoneMutedOrNull)
            {
                OnNgenuityMicrophoneChanged(wasMicrophoneMuted, isMicrophoneMutedOrNull);
            }
        }

        public void HandleNgenuityMicrophoneMuted(long microphoneID, bool isMicrophoneMuted)
        {
            bool? wasMicrophoneMuted = null;
            if (Program.State.MicrophoneStatus == MicrophoneStatus.Unmuted) wasMicrophoneMuted = false;
            if (Program.State.MicrophoneStatus == MicrophoneStatus.Muted) wasMicrophoneMuted = true;

            Program.State.NgenuityStatus = NgenuityStatus.Injected;
            Program.State.MicrophoneStatus = isMicrophoneMuted ? MicrophoneStatus.Muted : MicrophoneStatus.Unmuted;
            Program.State.Update();

            if (wasMicrophoneMuted != isMicrophoneMuted)
            {
                OnNgenuityMicrophoneChanged(wasMicrophoneMuted, isMicrophoneMuted);
            }
        }

        private static void SendMuteKey()
        {
            Program.Debug("Sending mute key!");
            Unmanaged.SendKeyboardEvent(Program.Settings.OnMuteKey, true);
            Unmanaged.SendKeyboardEvent(Program.Settings.OnMuteKey, false);
        }
        private static void SendUnmuteKey()
        {
            Program.Debug("Sending unmute key!");
            Unmanaged.SendKeyboardEvent(Program.Settings.OnUnmuteKey, true);
            Unmanaged.SendKeyboardEvent(Program.Settings.OnUnmuteKey, false);
        }

        private void OnNgenuityMicrophoneChanged(bool? wasMicrophoneMuted, bool? isMicrophoneMuted)
        {
            if (wasMicrophoneMuted == false && isMicrophoneMuted == true)
            {
                if (Program.Settings.SyncWithDiscord)
                {
                    if (Program.State.DiscordStatus < DiscordStatus.Normal)
                    {
                        // Do nothing!
                    }
                    else if (Program.State.DiscordStatus < DiscordStatus.Muted)
                    {
                        SendMuteKey();
                    }
                }
                else
                {
                    SendMuteKey();
                }

                return;
            }

            if (wasMicrophoneMuted == true && isMicrophoneMuted == false)
            {
                if (Program.Settings.SyncWithDiscord)
                {
                    if (Program.State.DiscordStatus < DiscordStatus.Normal)
                    {
                        // Do nothing!
                    }
                    else if (Program.State.DiscordStatus == DiscordStatus.Muted)
                    {
                        SendUnmuteKey();
                    }
                }
                else
                {
                    SendUnmuteKey();
                }

                return;
            }
        }

        public void HandleDiscordNotifyIconChange(byte[] iconBitmapBytes)
        {
            using (var image = DiscordTrayIcon.ByteArrayToBitmap(iconBitmapBytes))
            {
                switch (DiscordTrayIcon.FindClosestImageType(image))
                {
                case DiscordTrayIcon.ImageType.Default:
                case DiscordTrayIcon.ImageType.DefaultPing:
                case DiscordTrayIcon.ImageType.VoiceInactive:
                case DiscordTrayIcon.ImageType.VoiceActive:
                    Program.State.DiscordStatus = DiscordStatus.Normal;
                    break;
                case DiscordTrayIcon.ImageType.Muted:
                    Program.State.DiscordStatus = DiscordStatus.Muted;
                    break;
                case DiscordTrayIcon.ImageType.Deafened:
                    Program.State.DiscordStatus = DiscordStatus.Defeaned;
                    break;
                default:
                    Program.State.DiscordStatus = DiscordStatus.Unknown;
                    break;
                }
            }
        }
    }
}

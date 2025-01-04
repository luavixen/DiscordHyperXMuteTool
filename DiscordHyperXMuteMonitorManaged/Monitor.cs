using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace DiscordHyperXMuteMonitorManaged
{
    internal sealed class Monitor
    {

        [DllImport("kernel32.dll")]
        private static extern void OutputDebugString(string message);
        private static void Debug(string message)
        {
            OutputDebugString($"[DiscordHyperXMuteMonitor NGENUITY] {message}");
        }


        private static Thread _thread = null;
        private static readonly object _threadLock = new object();

        private static int Start(string argument)
        {
            lock (_threadLock)
            {
                if (_thread != null && _thread.IsAlive)
                {
                    return 0;
                }
                try
                {
                    _thread = new Thread(new Monitor().Main)
                    {
                        Name = "DiscordHyperXMuteMonitor",
                        IsBackground = true
                    };
                    _thread.Start();
                    return 0;
                }
                catch (Exception cause)
                {
                    _thread = null;
                    Debug($"Monitor thread failed to start: {cause}\n");
                    return -1;
                }
            }
        }


        private const string ToolWindowClassName = "DiscordHyperXMuteTool_MessageWindow";

        private const int PingMessage = 0x96c0;
        private const int MicrophoneMessage = 0x96c1;
        private const int NotifyIconMessage = 0x96c2;

        private const long PingFromNgenuity           = 0b000010;
        private const long PingFromExplorer           = 0b000011;
        private const long PingMicrophoneDisconnected = 0b001000;
        private const long PingMicrophoneConnected    = 0b001100;
        private const long PingMicrophoneUnmuted      = 0b100000;
        private const long PingMicrophoneMuted        = 0b110000;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string className, string windowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SendNotifyMessage(IntPtr window, int message, IntPtr wparam, IntPtr lparam);

        private static bool SendMessageToTool(int message, long wparam, long lparam)
        {
            var window = FindWindow(ToolWindowClassName, null);
            if (window == IntPtr.Zero) return false;

            var success = SendNotifyMessage(window, message, new IntPtr(wparam), new IntPtr(lparam));
            if (!success)
            {
                Debug($"SendNotifyMessage failed {Marshal.GetLastWin32Error()}\n");
                return false;
            }

            return true;
        }


        private Type[] _ngenuityExportedTypes;

        private void CollectExportedTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "NGenuity2Helper")
                {
                    _ngenuityExportedTypes = assembly.GetExportedTypes();
                    return;
                }
            }
            throw new Exception("NGenuity2Helper assembly not found");
        }
        private Type FindExportedType(string name)
        {
            foreach (Type type in _ngenuityExportedTypes)
            {
                if (type.FullName == name) return type;
            }
            throw new Exception($"Type {name} not found in NGenuity2Helper assembly");
        }


        private IEnumerable<dynamic> _ngenuityDevices;
        private dynamic _ngenuityDeviceTypeMicrophone;

        private void Main()
        {
            try
            {
                CollectExportedTypes();

                Type HyperXCenter = FindExportedType("NGenuity2.Devices.HyperXCenter");
                Type HyperXDeviceType = FindExportedType("NGenuity2.Devices.HyperXDeviceType");

                dynamic HyperXCenter_Center = HyperXCenter.GetProperty("Center").GetValue(null);
                dynamic HyperXCenter_Devices = HyperXCenter.GetProperty("Devices").GetValue(HyperXCenter_Center);

                dynamic HyperXDeviceType_Microphone = HyperXDeviceType.GetField("Microphone").GetValue(null);

                _ngenuityDevices = HyperXCenter_Devices as IEnumerable<dynamic>;
                _ngenuityDeviceTypeMicrophone = HyperXDeviceType_Microphone as dynamic;

                long ticks = 0;

                while (true)
                {
                    bool success = Update(ticks++);
                    if (!success) break;
                    Thread.Sleep(100);
                }

                Debug("Exiting, bye!\n");
            }
            catch (Exception cause)
            {
                Debug($"Unhandled exception: {cause}\n");
            }
        }

        private static int GetProcessId() => Process.GetCurrentProcess().Id;

        private static long CompressGuid(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();

            long low = BitConverter.ToInt64(bytes, 0);
            long high = BitConverter.ToInt64(bytes, 8);

            long result = low ^ high;
            return result != 0 ? result : 1;
        }

        private IEnumerable<dynamic> GetMicrophones()
        {
            foreach (dynamic device in _ngenuityDevices)
            {
                if (device.DeviceType == _ngenuityDeviceTypeMicrophone)
                {
                    yield return device;
                }
            }
        }


        private static readonly Dictionary<Guid, bool?> _microphoneMuteStates = new Dictionary<Guid, bool?>();

        private bool Update(long ticks)
        {
            foreach (dynamic device in GetMicrophones())
            {
                bool? mutedOld; _microphoneMuteStates.TryGetValue(device.ID, out mutedOld);
                bool? mutedNew = device.MicrophoneMuted;
                if (mutedOld != mutedNew)
                {
                    _microphoneMuteStates[device.ID] = mutedNew;
                    OnMicrophoneMuted(device, mutedNew == true);
                }
                /*
                Debug(
                    $"Microphone {device.ID} {device.ToString()}\n" +
                    $"Name: {device.Name}\n" +
                    $"Version: {device.Version}\n" +
                    $"MicrophoneAvailable: {device.MicrophoneAvailable}\n" +
                    $"SoundMuted: {device.SoundMuted}\n" +
                    $"MicrophoneMuted: {device.MicrophoneMuted}\n" +
                    $"SoundVolume: {device.SoundVolume}\n" +
                    $"MicrophoneVolume: {device.MicrophoneVolume}\n"
                );
                */
            }

            if (ticks % 10 == 0)
            {
                bool success = OnPing();
                if (!success) return false;
            }

            return true;
        }

        private bool OnMicrophoneMuted(dynamic device, bool muted)
        {
            long id = CompressGuid(device.ID);
            return SendMessageToTool(MicrophoneMessage, id, muted ? 1 : 0);
        }

        private bool OnPing()
        {
            long flags = PingFromNgenuity;

            var device = GetMicrophones().FirstOrDefault();
            if (device != null)
            {
                flags |= PingMicrophoneConnected;
                flags |= device.MicrophoneMuted ? PingMicrophoneMuted : PingMicrophoneUnmuted;
            }
            else
            {
                flags |= PingMicrophoneDisconnected;
            }

            return SendMessageToTool(PingMessage, GetProcessId(), flags);
        }

    }
}

#region NGenuity2Helper relevant exported types
/*
namespace NGenuity2.Devices
{
    public enum HyperXDeviceType
    {
        Unknown = 0,
        Keyboard = 1,
        Mouse = 2,
        Mousepad = 4,
        Headset = 8,
        DRAM = 16, // 0x00000010
        Microphone = 32, // 0x00000020
        Webcam = 33, // 0x00000021
        Composite = 128, // 0x00000080
    }

    public abstract class HyperXDevice
    {
        public enum ControlBySoftware
        {
            NGENUITY,
            OLS
        }

        // Properties
        public HyperXDeviceType DeviceType { get; set; }
        public HyperXDeviceModel Model { get; set; }
        public ushort VendorID { get; set; }
        public ushort ProductID { get; set; }
        public int PairID { get; set; }
        public string DeviceID { get; protected set; }
        public string SecondaryDeviceID { get; protected set; }
        public string Name { get; internal set; }
        public bool Connected { get; protected set; }
        public bool IsSimulator { get; internal set; }
        public bool Synchronizing { get; protected set; }
        public DriverInstallationState DriverInstallationState { get; set; }
        public bool DriverActivated { get; set; }
        public virtual bool IsOpened { get; }
        public virtual bool IsValidDevice { get; }
        public ColorSkins ColorSkin { get; set; }
        public bool IsWirelessProduct { get; set; }
        public string UniqueID { get; set; }
        public bool IsDongle { get; set; }
        public Dictionary<int, long> ExtraProperties { get; protected set; }
        public int OnboardProfileId { get; set; }
        public Guid ActivedPresetGUID { get; set; }
        public Guid[] SyncedPresets { get; }
        public byte CurrentPresetID { get; set; }
        public int ProfileSlots { get; protected set; }
        public int LowBatteryThreshold { get; set; }
        public int LowBatteryThresholdMin { get; set; }
        public int LowBatteryThresholdMax { get; set; }
        public ChargingStatus ChargingStatus { get; set; }
        public int Battery { get; set; }
        public bool Sleeping { get; internal set; }
        public int AutoPowerOff { get; internal set; }
        public int CountryCode { get; set; }
        public List<KeyMap> Keys { get; protected set; }
        public List<KeyMap> FnKeys { get; protected set; }
        public KeyCode FunctionLockKey { get; set; }
        public bool FunctionLockEnabled { get; set; }
        public Color FunctionLockColor { get; set; }
        public virtual bool SpeakerAvailable { get; set; }
        public virtual bool MicrophoneAvailable { get; set; }
        public virtual bool SoundMuted { get; set; }
        public virtual bool MicrophoneMuted { get; set; }
        public virtual bool SidetoneMuted { get; set; }
        public virtual float SoundVolume { get; set; }
        public virtual float MicrophoneVolume { get; set; }
        public virtual float SidetoneVolume { get; set; }
        public bool Linked { get; set; }
        public bool CanLink { get; set; }
        public bool CapPollingRate { get; protected set; }
        public bool CapGameMode { get; protected set; }
        public bool CapSensorDPIs { get; protected set; }
        public bool CapKeyAssignments { get; protected set; }
        public virtual bool ProductNeedUpdate { get; protected set; }
        public int Version { get; set; }
        public bool DFUNeedReboot { get; set; }
        public bool NeedFactoryReset { get; set; }
        public bool Busy { get; set; }
        public bool Pairing { get; set; }
        public int IOTimeout { get; set; }
        public int FramePerSecond { get; protected set; }
        public int UIFramePerSecond { get; set; }
        public virtual bool UpgradeMode { get; protected set; }
        public virtual string RequiredVersions { get; set; }
        public HyperXDevice.ControlBySoftware ControlBy { get; set; }
        public string NotificationDeviceID { get; protected set; }
        public string DFUDeviceID { get; set; }
        public Guid ID { get; protected set; }
        public List<HXCommandBase> Commands { get; set; }
        public Preset Preset { get; protected set; }
        public Preset BuiltinPreset { get; set; }
        public bool GameMode { get; set; }
        public int LightLevel { get; set; }
        public int PresetIndex { get; set; }
        public string Firmware { get; protected set; }
        public bool IOError { get; set; }
        public int MaxSyncFrameCount { get; set; }
        public int TotalSyncTime { get; set; }
        public bool batteryThresholdNotified { get; set; }
        public bool battery5PercentageNotified { get; set; }
        public ushort AudioDeviceVendorId { get; }
        public List<ushort> AudioDeviceProductIds { get; }
        public IReadOnlyList<MMDevice> AudioDevices { get; }
        public Guid[] SnapshotPresetIDs { get; }

        // Events
        public event TypedEventHandler<HyperXDevice, KeyEventAgrs> KeyReceived;
        public event TypedEventHandler<string, List<KeyMap>> LightsChanged;
        public event TypedEventHandler<object, bool> GameModeChanged;
        public event TypedEventHandler<object, int> BrightnessLevelChanged;
        public event TypedEventHandler<object, BrightnessAmbientPayload> BrightnessAmbientChanged;
        public event TypedEventHandler<HyperXDevice, bool> FunctionLockStatusChanged;
        public event TypedEventHandler<object, int> MouseDPILevelChanged;
        public event TypedEventHandler<object, ChargingStatus> ChargingStatusChanged;
        public event TypedEventHandler<object, int> BatteryUpdated;
        public event TypedEventHandler<object, bool> SleepingStatusChanged;
        public event TypedEventHandler<object, bool> ConnectedStatusChanged;
        public event TypedEventHandler<object, int> SyncProgressUpdated;
        public event TypedEventHandler<HyperXDevice, bool> DevicePaired;
        public event TypedEventHandler<MMDevice, HyperXDevice> AudioDeviceConnected;
        public event TypedEventHandler<AudioDeviceType, bool> AudioDeviceMuted;
        public event TypedEventHandler<AudioDeviceType, bool> AudioDeviceAvailabilityChanged;
        public event TypedEventHandler<AudioDeviceType, float> AudioDeviceVolumeUpdated;
        public event TypedEventHandler<AudioDeviceType, float> AudioDeviceMeterUpdated;
        public event TypedEventHandler<HyperXDevice, float> AudioDeviceTestProgressUpdated;
        public event TypedEventHandler<HyperXDevice, int> OnboardProfileChanged;
        public event EventHandler<HyperXDevice> ControlByUpdateUI;
        public event FirmwareProgressHandler Updating;

        // Methods
        public virtual void Serialize(BinaryWriter bw);
        public virtual void Deserialize(BinaryReader br, int version);
        public virtual void UpdateName();
        public virtual bool ApplyExtraProperty(int key, long value);
        public virtual void ApplyKeyAssignments(IEnumerable<KeyAssignment> keyAssignments);
        public virtual void ApplyKeyAssignments(Preset preset);
        public virtual void ApplyLowBatteryThreshold(int threshold);
        public virtual void ApplySensorDPIs();
        public virtual void ApplyMacros();
        public virtual void ApplyPollingRate();
        public virtual void ApplyBacklightFadeTime();
        public virtual void ApplyGameMode();
        public virtual void ApplySidetoneSettings();
        public virtual void ApplyBasicSettings();
        public virtual void ApplyKeyAssignments();
        public virtual void ApplyPreset(Preset preset);
        public virtual void SetupPreset(Preset preset);
        public virtual void ApplyPresetAndEffects(Preset preset);
        public virtual void ApplyEffects();
        public virtual void SetControlByNG2();
        public virtual void ResetToDefault();
        public virtual void SwitchToOnboardProfile(int profileId);
        public virtual List<string> CheckDrivers();
        public virtual void Start();
        public virtual void PreStop();
        public virtual void Stop(bool waitUntilStopped);
        public virtual void SetLightings(IList<KeyMap> keys);
        public virtual void OpenDevice(string deviceId);
        public virtual void OpenSecondaryDevice(string deviceId);
        public virtual void OpenDongle(string deviceId);
        public virtual void NotifyFirmwareUpdate();
        public virtual void EnterPairMode();
        public virtual void CancelPairMode();
        public virtual void FactoryReset();
        public virtual void SetupSimulator();
        public virtual void AddAudioDevice(MMDevice device);
        public virtual void RemoveAudioDevice(MMDevice device);
        public virtual void RemoveAudioDevice(string audioEndpointId);
        public abstract void SetAllLEDOff();
        public abstract EffectImplBase CreateEffect(EffectItemBase item);
    }

    public sealed class HyperXCenter
    {
        // Properties
        public static HyperXCenter Center { get; }
        public HyperXCompositeDevice CompositeDevice { get; }
        public Updater Updater { get; set; }
        public bool UpgradeMode { get; set; }
        public IReadOnlyList<ScreenMirrorInfo> AllMirrorInfo { get; }
        public IReadOnlyList<HyperXDevice> Devices { get; }
        public HyperXDevice CurrentDevice { get; set; }

        // Events
        public event TypedEventHandler<object, HyperXDevice> DeviceAdded;
        public event TypedEventHandler<object, HyperXDevice> DeviceRemoved;
        public event TypedEventHandler<object, HyperXDevice> DeviceFirmwareUpdated;
        public event TypedEventHandler<object, HyperXDevice> DeviceNameChanged;
        public event TypedEventHandler<object, HyperXDevice> DeviceUpdated;
        public event TypedEventHandler<object, HyperXMonitorBase> MonitorAdded;
        public event TypedEventHandler<object, HyperXMonitorBase> MonitorRemoved;
        public event TypedEventHandler<object, HyperXMonitorBase> MonitorDataRefreshed;
        public event TypedEventHandler<object, HyperXMonitorBase> MonitorOsdOn;
        public event TypedEventHandler<object, List<ScreenMirrorInfo>> AllMirrorChanged;

        // Methods
        public bool DeviceHasNewerVersion(int ver, ushort vid, ushort pid);
        public void RemoveDeviceName(string name);
        public string GetNewDeviceName(string baseName);
        public bool HasNewerBuiltinFirmware(HyperXDevice device);
        public bool IsWirelessProductFirmwareAvailable(DeviceInforPayload deviceInformation);
        public Version GetMinimunMainFirmwareVersion(uint deviceId);
        public Version GetMinimunRFHostFirmwareVersion(uint deviceId);
        public Version GetMinimunRFClientFirmwareVersion(uint deviceId);
        public ushort? GetMinimunRFClientFirmwareVersions(uint pidVidNumber);
        public void ApplyPresetToAllDevices(Preset preset);
        public void RemoveDevice(HyperXDevice device);
        public void ClearDeviceNames();
        public bool ContainsDevice(HyperXDevice device);
        public bool ContainsDevice(HyperXDeviceType type);
        public bool ContainsDevice(HyperXDeviceModel model);
        public void AddVirtualDevice(HyperXDeviceModel model);
        public void ScanUnloadedDevices();
        public void StartDeviceInitialization(IntPtr handle, string publisherCachePath);
        public HyperXDevice FindDevice(string id);
        public HyperXDevice FindDevice(ushort vid, ushort pid);
        public T FindDevice<T>() where T : HyperXDevice;
        public void FindDeviceAndSafeExecuteAction(Func<HyperXDevice, bool> predicate, Action<HyperXDevice> action);
        public bool IsDeviceExist(HyperXDeviceModel targetModel);
        public void StopAllDevices();
        public void RemoveAllDevices();
        public HyperXDevice CreateCompositeDevice(string deviceId1, string deviceId2);
        public void DismissCompositeDevice();
        public bool CheckAndAddDeviceMetaData(int vID, int pID, string devicePath);
        public void TakeControlOfDeviceLighting(HyperXDevice device);
        public void LoadAllDevices();
        public void ReloadDevice(string deviceId);
        public void InstallDrivers();
        public bool RemoveDTSDrivers();
    }
}
*/
#endregion

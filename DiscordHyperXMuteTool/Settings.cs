using Microsoft.Win32;
using System;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace DiscordHyperXMuteTool
{
    internal class Settings : Reactive<Settings>
    {
        public bool Enabled = true;
        public bool RunOnStartup = true;
        public bool SyncWithDiscord = true;
        public Keycode OnMuteKey = Keycode.F20;
        public Keycode OnUnmuteKey = Keycode.F20;
        public string DiscordProcessName = "Discord";

        private static readonly Settings Defaults = new Settings();

        public Settings()
        {
            Subscribe(settings => settings.Save());
        }

        public void Initialize()
        {
            Load();
            SyncStartup();
        }

        private const string SettingsRegistryPath = @"Software\DiscordHyperXMuteTool";

        private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string StartupRegistryName = "DiscordHyperXMuteTool";

        private static int GetRegistryInt(RegistryKey key, string name, int defaultValue)
        {
            object storedValue = key.GetValue(name);
            try
            {
                return Convert.ToInt32(storedValue);
            }
            catch (FormatException) { return defaultValue; }
            catch (OverflowException) { return defaultValue; }
            catch (InvalidCastException) { return defaultValue; }
        }
        private static void SetRegistryInt(RegistryKey key, string name, int value)
        {
            key.SetValue(name, value, RegistryValueKind.DWord);
        }

        private static string GetRegistryString(RegistryKey key, string name, string defaultValue)
        {
            object storedValue = key.GetValue(name);
            return storedValue as string ?? defaultValue;
        }
        private static void SetRegistryString(RegistryKey key, string name, string value)
        {
            key.SetValue(name, value, RegistryValueKind.String);
        }

        private void Load()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SettingsRegistryPath))
                {
                    if (key != null)
                    {
                        Enabled = GetRegistryInt(key, nameof(Enabled), Defaults.Enabled ? 1 : 0) != 0;
                        RunOnStartup = GetRegistryInt(key, nameof(RunOnStartup), Defaults.RunOnStartup ? 1 : 0) != 0;
                        SyncWithDiscord = GetRegistryInt(key, nameof(SyncWithDiscord), Defaults.SyncWithDiscord ? 1 : 0) != 0;
                        OnMuteKey = (Keycode)GetRegistryInt(key, nameof(OnMuteKey), (int)Defaults.OnMuteKey);
                        OnUnmuteKey = (Keycode)GetRegistryInt(key, nameof(OnUnmuteKey), (int)Defaults.OnUnmuteKey);
                        DiscordProcessName = GetRegistryString(key, nameof(DiscordProcessName), Defaults.DiscordProcessName);
                    }
                }
            }
            catch (IOException) { }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
        }
        private void Save()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsRegistryPath))
                {
                    if (key != null)
                    {
                        SetRegistryInt(key, nameof(Enabled), Enabled ? 1 : 0);
                        SetRegistryInt(key, nameof(RunOnStartup), RunOnStartup ? 1 : 0);
                        SetRegistryInt(key, nameof(SyncWithDiscord), SyncWithDiscord ? 1 : 0);
                        SetRegistryInt(key, nameof(OnMuteKey), (int)OnMuteKey);
                        SetRegistryInt(key, nameof(OnUnmuteKey), (int)OnUnmuteKey);
                        SetRegistryString(key, nameof(DiscordProcessName), DiscordProcessName);
                    }
                }
            }
            catch (IOException) { }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }

            SyncStartup();
        }

        private void SyncStartup()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, true))
                {
                    if (key != null)
                    {
                        if (RunOnStartup)
                        {
                            key.SetValue(StartupRegistryName, Application.ExecutablePath);
                        }
                        else
                        {
                            key.DeleteValue(StartupRegistryName, false);
                        }
                    }
                }
            }
            catch (IOException) { }
            catch (SecurityException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}

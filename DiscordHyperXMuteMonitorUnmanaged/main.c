#include "project.h"

HMODULE LibraryModule;
WCHAR LibraryPath[MAX_PATH];
HMODULE ApplicationModule;
WCHAR ApplicationPath[MAX_PATH];

static void InitializeLibrary(HMODULE module)
{
    LibraryModule = module;
    GetModuleFileNameW(LibraryModule, LibraryPath, MAX_PATH);
    ApplicationModule = GetModuleHandleW(NULL);
    GetModuleFileNameW(ApplicationModule, ApplicationPath, MAX_PATH);
}

static void UnloadLibrary()
{
    FreeLibraryAndExitThread(LibraryModule, 0);
}

SETTINGS Settings;

static DWORD ReadRegistryValue(HKEY key, LPCWSTR name, DWORD defaultValue)
{
    DWORD value;
    DWORD valueSize = sizeof(value);
    if (RegQueryValueExW(key, name, NULL, NULL, (LPBYTE)&value, &valueSize) != ERROR_SUCCESS)
    {
        value = defaultValue;
    }
    return value;
}

void ReadSettings(void)
{
    HKEY key;
    if (RegOpenKeyExW(HKEY_CURRENT_USER, L"Software\\DiscordHyperXMuteTool", 0, KEY_READ, &key) == ERROR_SUCCESS)
    {
        Settings.Enabled = ReadRegistryValue(key, L"Enabled", TRUE) != FALSE;
        Settings.RunOnStartup = ReadRegistryValue(key, L"RunOnStartup", FALSE) != FALSE;
        Settings.SyncWithDiscord = ReadRegistryValue(key, L"SyncWithDiscord", TRUE) != FALSE;
        Settings.OnMuteKey = ReadRegistryValue(key, L"OnMuteKey", VK_F20);
        Settings.OnUnmuteKey = ReadRegistryValue(key, L"OnUnmuteKey", VK_F20);
        RegCloseKey(key);
    }
}

DLLEXPORT BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID reserved)
{
    switch (reason)
    {
    case DLL_PROCESS_ATTACH:
        DisableThreadLibraryCalls(module);
        InitializeLibrary(module);
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

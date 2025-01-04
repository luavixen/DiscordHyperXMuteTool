#include "project.h"

HMODULE LibraryModule;
WCHAR LibraryPath[MAX_PATH];
HMODULE ExecutableModule;
WCHAR ExecutablePath[MAX_PATH];

static BOOL InitializeLibraryInNgenuity(void)
{
    HANDLE thread = CreateThread(NULL, 0, NgenuityMonitorBootstrapThreadProc, NULL, 0, NULL);
    if (!thread)
    {
        DebugFormatError(GetLastError(), L"CreateThread failed to start NgenuityMonitorBootstrapThreadProc thread");
        return FALSE;
    }
    else
    {
        return TRUE;
    }
}
static BOOL InitializeLibraryInExplorer(void)
{
    return TRUE;
}

static BOOL InitializeLibrary(HMODULE module)
{
    DisableThreadLibraryCalls(module);

    LibraryModule = module;
    if (GetModuleFileNameW(LibraryModule, LibraryPath, MAX_PATH) == 0)
    {
        OutputDebugStringW(DEBUG_PREFIX L"GetModuleFileNameW failed to get LibraryPath\n");
        return FALSE;
    }

    ExecutableModule = GetModuleHandleW(NULL);
    if (GetModuleFileNameW(ExecutableModule, ExecutablePath, MAX_PATH) == 0)
    {
        OutputDebugStringW(DEBUG_PREFIX L"GetModuleFileNameW failed to get ExecutablePath, possibly due to GetModuleHandleW\n");
        return FALSE;
    }

    if (!InitializeHeap())
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to initialize heap\n");
        return FALSE;
    }

    if (StringIndexOf(LibraryPath, MONITOR_UNMANAGED_FILENAME, FALSE) < 0)
    {
        DebugFormat(L"Unexpected LibraryPath: %s\n", LibraryPath);
    }

    if (StringIndexOf(ExecutablePath, NGENUITY_PROCESS_NAME, FALSE) >= 0)
    {
        Debug(L"Attached to NGENUITY (via injection) *waves my paw*\n");
        return InitializeLibraryInNgenuity();
    }
    if (StringIndexOf(ExecutablePath, EXPLORER_PROCESS_NAME, FALSE) >= 0)
    {
        Debug(L"Attached to Explorer (via hook) *waves my paw*\n");
        return InitializeLibraryInExplorer();
    }

    return TRUE;
}

static void FinalizeLibrary(void)
{
    if (!FinalizeHeap())
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to finalize heap\n");
    }

    OutputDebugStringW(DEBUG_PREFIX L"Detaching, goodbye!\n");
}

DLLEXPORT BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID reserved)
{
    switch (reason)
    {
    case DLL_PROCESS_ATTACH:
        return InitializeLibrary(module);
    case DLL_PROCESS_DETACH:
        if (!reserved) FinalizeLibrary();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    return TRUE;
}

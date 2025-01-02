#include "project.h"

HMODULE LibraryModule;
WCHAR LibraryPath[MAX_PATH];
HMODULE ApplicationModule;
WCHAR ApplicationPath[MAX_PATH];

static BOOL InitializeLibrary(HMODULE module)
{
    DisableThreadLibraryCalls(module);

    OutputDebugStringW(DEBUG_PREFIX L"Hello, world! *waves my paw*");

    LibraryModule = module;
    if (GetModuleFileNameW(LibraryModule, LibraryPath, MAX_PATH) == 0)
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to get LibraryPath with GetModuleFileNameW");
        return FALSE;
    }

    ApplicationModule = GetModuleHandleW(NULL);
    if (GetModuleFileNameW(ApplicationModule, ApplicationPath, MAX_PATH) == 0)
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to get ApplicationPath with GetModuleFileNameW, possibly due to GetModuleHandleW");
        return FALSE;
    }

    if (!InitializeHeap())
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to initialize heap");
        return FALSE;
    }

    if (StringIndexOf(ApplicationPath, L"NGenuity2Helper", FALSE) >= 0)
    {
        HANDLE thread = CreateThread(NULL, 0, NgenuityMonitorBootstrapThreadProc, NULL, 0, NULL);
        if (!thread)
        {
            DebugFormatError(GetLastError(), L"Failed to start NgenuityMonitorBootstrapThreadProc thread");
            return FALSE;
        }
    }

    return TRUE;
}

static void FinalizeLibrary(void)
{
    if (!FinalizeHeap())
    {
        OutputDebugStringW(DEBUG_PREFIX L"Failed to finalize heap");
    }

    OutputDebugStringW(DEBUG_PREFIX L"Goodbye, see you later!");
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

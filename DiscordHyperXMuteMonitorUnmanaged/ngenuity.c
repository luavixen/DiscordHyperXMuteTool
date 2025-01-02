#include "project.h"
#include <shlwapi.h>
#include <mscoree.h>
#include <metahost.h>
#include <corerror.h>

#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "mscoree.lib")

static LPTHREAD_START_ROUTINE FindLoadLibraryProcedure(void)
{
    HMODULE module = GetModuleHandleW(L"kernel32.dll");
    if (!module) return NULL;
    FARPROC procedure = GetProcAddress(module, "LoadLibraryW");
    if (!procedure) return NULL;
    return (LPTHREAD_START_ROUTINE)procedure;
}

DLLEXPORT LPWSTR WINAPI InjectMonitorIntoNgenuityProcess(DWORD processID)
{
    LPWSTR result = NULL;

    HANDLE process = NULL;

    LPVOID remoteLibraryPathBuffer = NULL;
    HANDLE remoteThread = NULL;

    LPTHREAD_START_ROUTINE loadLibraryProcedure = FindLoadLibraryProcedure();
    if (!loadLibraryProcedure)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Injection failed to find LoadLibraryW procedure in kernel32.dll");
        goto exit;
    }

    process = OpenProcess(
        PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE,
        FALSE, processID
    );
    if (!process)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Injection failed to open target process");
        goto exit;
    }

    remoteLibraryPathBuffer = VirtualAllocEx(
        process,
        NULL,
        sizeof(LibraryPath),
        MEM_COMMIT | MEM_RESERVE,
        PAGE_READWRITE
    );
    if (!remoteLibraryPathBuffer)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Injection failed to allocate memory in target process");
        goto exit;
    }

    BOOL writeProcessMemorySuccess = WriteProcessMemory(
        process,
        remoteLibraryPathBuffer,
        LibraryPath,
        sizeof(LibraryPath),
        NULL
    );
    if (!writeProcessMemorySuccess)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Injection failed to write memory in target process");
        goto exit;
    }

    remoteThread = CreateRemoteThread(
        process,
        NULL,
        0,
        loadLibraryProcedure,
        remoteLibraryPathBuffer,
        0,
        NULL
    );
    if (!remoteThread)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Injection failed to create remote thread in target process");
        goto exit;
    }

    WaitForSingleObject(remoteThread, INFINITE);

exit:
    if (remoteThread) CloseHandle(remoteThread);
    if (remoteLibraryPathBuffer) VirtualFreeEx(process, remoteLibraryPathBuffer, 0, MEM_RELEASE);
    if (process) CloseHandle(process);

    return result;
}

static ICLRRuntimeHost* GetRuntimeHost()
{
    HRESULT result = S_OK;

    ICLRMetaHost* clrMetaHost = NULL;
    ICLRRuntimeInfo* clrRuntimeInfo = NULL;
    ICLRRuntimeHost* clrRuntimeHost = NULL;

    result = CLRCreateInstance(&CLSID_CLRMetaHost, &IID_ICLRMetaHost, &clrMetaHost);
    if (FAILED(result))
    {
        DebugFormatError(result, L"Failed to create ICLRMetaHost");
        goto exit;
    }

    result = clrMetaHost->lpVtbl->GetRuntime(clrMetaHost, L"v4.0.30319", &IID_ICLRRuntimeInfo, &clrRuntimeInfo);
    if (FAILED(result))
    {
        DebugFormatError(result, L"Failed to get ICLRRuntimeInfo from ICLRMetaHost->GetRuntime");
        goto exit;
    }

    result = clrRuntimeInfo->lpVtbl->GetInterface(clrRuntimeInfo, &CLSID_CLRRuntimeHost, &IID_ICLRRuntimeHost, &clrRuntimeHost);
    if (FAILED(result))
    {
        DebugFormatError(result, L"Failed to get ICLRRuntimeHost from ICLRRuntimeInfo->GetInterface");
        goto exit;
    }

exit:
    if (clrMetaHost) clrMetaHost->lpVtbl->Release(clrMetaHost);
    if (clrRuntimeInfo) clrRuntimeInfo->lpVtbl->Release(clrRuntimeInfo);

    return clrRuntimeHost;
}

static void ReleaseRuntimeHost(ICLRRuntimeHost* clrRuntimeHost)
{
    clrRuntimeHost->lpVtbl->Release(clrRuntimeHost);
}

static void ExecuteAssemblyInRuntime(
    ICLRRuntimeHost* clrRuntimeHost,
    LPCWSTR assemblyPath,
    LPCWSTR assemblyTypeName,
    LPCWSTR assemblyMethodName,
    LPCWSTR assemblyArgument
) {
    DWORD value = 0;

    HRESULT result = clrRuntimeHost->lpVtbl->ExecuteInDefaultAppDomain(
        clrRuntimeHost,
        assemblyPath,
        assemblyTypeName,
        assemblyMethodName,
        assemblyArgument,
        &value
    );

    if (FAILED(result))
    {
        switch (result)
        {
        case HOST_E_CLRNOTAVAILABLE:
            Debug(L"Failed to execute managed DLL, CLR is not available\n");
            break;
        case HOST_E_TIMEOUT:
            Debug(L"Failed to execute managed DLL, call timed out\n");
            break;
        case HOST_E_NOT_OWNER:
            Debug(L"Failed to execute managed DLL, caller does not own lock\n");
            break;
        case HOST_E_ABANDONED:
            Debug(L"Failed to execute managed DLL, event was canceled\n");
            break;
        case COR_E_MISSINGMETHOD:
            Debug(L"Failed to execute managed DLL, method not found\n");
            break;
        case E_FAIL:
            Debug(L"Failed to execute managed DLL, unknown catastrophic failure occurred\n");
            break;
        default:
            DebugFormatError(result, L"Failed to execute managed DLL, unknown error occurred");
            break;
        }
    }
}

DWORD WINAPI NgenuityMonitorBootstrapThreadProc(LPVOID parameter)
{
    ICLRRuntimeHost* clrRuntimeHost = GetRuntimeHost();

    if (clrRuntimeHost)
    {
        WCHAR assemblyPath[MAX_PATH] = { 0 };
        StringCopyInto(assemblyPath, LibraryPath);
        PathRemoveFileSpecW(assemblyPath);
        PathAppendW(assemblyPath, L"DiscordHyperXMuteMonitorUnmanaged.dll");

        LPWSTR assemblyTypeName = L"DiscordHyperXMuteMonitor.Main";
        LPWSTR assemblyMethodName = L"Start";
        LPWSTR assemblyArgument = L"";

        ExecuteAssemblyInRuntime(clrRuntimeHost, assemblyPath, assemblyTypeName, assemblyMethodName, assemblyArgument);

        ReleaseRuntimeHost(clrRuntimeHost);
    }

    FreeLibraryAndExitThread(LibraryModule, 0);
}

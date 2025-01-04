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
    WCHAR processPath[MAX_PATH] = { 0 };

    LPVOID remoteLibraryPathBuffer = NULL;
    HANDLE remoteThread = NULL;

    LPTHREAD_START_ROUTINE loadLibraryProcedure = FindLoadLibraryProcedure();
    if (!loadLibraryProcedure)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"Failed to find LoadLibraryW procedure in kernel32.dll?");
        goto exit;
    }

    process = OpenProcess(
        PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE,
        FALSE, processID
    );
    if (!process)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"OpenProcess failed");
        goto exit;
    }

    if (!GetProcessPath(process, processPath)) {
        result = FormatErrorWithExplanation(GetLastError(), L"QueryFullProcessImageNameW failed");
        goto exit;
    }

    if (StringIndexOf(processPath, NGENUITY_PROCESS_NAME, FALSE) < 0)
    {
        result = StringFormat(L"Unexpected process path: %s", processPath);
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
        result = FormatErrorWithExplanation(GetLastError(), L"VirtualAllocEx failed");
        goto exit;
    }

    if (!WriteProcessMemory(
        process,
        remoteLibraryPathBuffer,
        LibraryPath,
        sizeof(LibraryPath),
        NULL
    )) {
        result = FormatErrorWithExplanation(GetLastError(), L"WriteProcessMemory failed");
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
        result = FormatErrorWithExplanation(GetLastError(), L"CreateRemoteThread failed");
        goto exit;
    }

    WaitForSingleObject(remoteThread, INFINITE);

exit:
    if (remoteThread)
    {
        CloseHandle(remoteThread);
    }
    if (remoteLibraryPathBuffer)
    {
        VirtualFreeEx(process, remoteLibraryPathBuffer, 0, MEM_RELEASE);
    }
    if (process)
    {
        CloseHandle(process);
    }

    return result;
}

static ICLRRuntimeHost* GetRuntimeHost(void)
{
    HRESULT result = S_OK;

    ICLRMetaHost* clrMetaHost = NULL;
    ICLRRuntimeInfo* clrRuntimeInfo = NULL;
    ICLRRuntimeHost* clrRuntimeHost = NULL;

    result = CLRCreateInstance(&CLSID_CLRMetaHost, &IID_ICLRMetaHost, &clrMetaHost);
    if (FAILED(result))
    {
        DebugFormatError(result, L"CLRCreateInstance failed to create ICLRMetaHost");
        goto exit;
    }

    result = clrMetaHost->lpVtbl->GetRuntime(clrMetaHost, L"v4.0.30319", &IID_ICLRRuntimeInfo, &clrRuntimeInfo);
    if (FAILED(result))
    {
        DebugFormatError(result, L"ICLRMetaHost->GetRuntime failed to get ICLRRuntimeInfo");
        goto exit;
    }

    result = clrRuntimeInfo->lpVtbl->GetInterface(clrRuntimeInfo, &CLSID_CLRRuntimeHost, &IID_ICLRRuntimeHost, &clrRuntimeHost);
    if (FAILED(result))
    {
        DebugFormatError(result, L"ICLRRuntimeInfo->GetInterface failed to get ICLRRuntimeHost");
        goto exit;
    }

exit:
    if (clrMetaHost)
    {
        clrMetaHost->lpVtbl->Release(clrMetaHost);
    }
    if (clrRuntimeInfo)
    {
        clrRuntimeInfo->lpVtbl->Release(clrRuntimeInfo);
    }

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
            Debug(L"ExecuteInDefaultAppDomain failed, CLR is not available\n");
            break;
        case HOST_E_TIMEOUT:
            Debug(L"ExecuteInDefaultAppDomain failed, call timed out\n");
            break;
        case HOST_E_NOT_OWNER:
            Debug(L"ExecuteInDefaultAppDomain failed, caller does not own lock\n");
            break;
        case HOST_E_ABANDONED:
            Debug(L"ExecuteInDefaultAppDomain failed, event was canceled\n");
            break;
        case COR_E_MISSINGMETHOD:
            Debug(L"ExecuteInDefaultAppDomain failed, method not found\n");
            break;
        case COR_E_TYPELOAD:
            Debug(L"ExecuteInDefaultAppDomain failed, type failed to load\n");
            break;
        case E_FAIL:
            Debug(L"ExecuteInDefaultAppDomain failed, unknown catastrophic failure occurred\n");
            break;
        default:
            DebugFormatError(result, L"ExecuteInDefaultAppDomain failed, unknown error occurred");
            break;
        }
    }
}

DLLEXPORT DWORD WINAPI NgenuityMonitorBootstrapThreadProc(LPVOID parameter)
{
    ICLRRuntimeHost* clrRuntimeHost = GetRuntimeHost();

    if (clrRuntimeHost)
    {
        WCHAR assemblyPath[MAX_PATH] = { 0 };
        StringCopyInto(assemblyPath, LibraryPath);
        PathRemoveFileSpecW(assemblyPath);
        PathAppendW(assemblyPath, MONITOR_MANAGED_FILENAME);

        LPCWSTR assemblyTypeName = L"DiscordHyperXMuteMonitorManaged.Monitor";
        LPCWSTR assemblyMethodName = L"Start";
        LPCWSTR assemblyArgument = L"";

        ExecuteAssemblyInRuntime(clrRuntimeHost, assemblyPath, assemblyTypeName, assemblyMethodName, assemblyArgument);

        ReleaseRuntimeHost(clrRuntimeHost);
    }

    FreeLibraryAndExitThread(LibraryModule, 0);
}

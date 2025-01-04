#include "project.h"

static HHOOK TrayHook = NULL;
static DWORD TrayThreadID = 0;

DLLEXPORT BOOL WINAPI ExplorerTrayIsHookValid(void)
{
    if (!TrayHook) return FALSE;
    if (!TrayThreadID) return FALSE;

    HANDLE thread = OpenThread(THREAD_QUERY_LIMITED_INFORMATION, FALSE, TrayThreadID);
    if (!thread)
    {
        DebugFormatError(GetLastError(), L"OpenThread failed while checking hook validity");
        return FALSE;
    }

    DWORD exitCode = 0;
    BOOL exitCodeSuccess = GetExitCodeThread(thread, &exitCode);
    if (!exitCodeSuccess)
    {
        DebugFormatError(GetLastError(), L"GetExitCodeThread failed while checking hook validity");
    }

    CloseHandle(thread);

    return exitCodeSuccess && exitCode == STILL_ACTIVE;
}

DLLEXPORT BOOL WINAPI ExplorerTrayRemoveHook(void)
{
    if (!TrayHook) return FALSE;

    BOOL success = UnhookWindowsHookEx(TrayHook);
    if (!success)
    {
        DebugFormatError(GetLastError(), L"UnhookWindowsHookEx failed");
    }

    TrayHook = NULL;
    TrayThreadID = 0;

    return success;
}

DLLEXPORT LPWSTR WINAPI ExplorerTraySetHook(void)
{
    LPWSTR result = NULL;

    HWND window = NULL;
    DWORD windowThreadID = 0;

    HOOKPROC hookProcedure = NULL;
    HHOOK hook = NULL;

    ExplorerTrayRemoveHook();

    window = FindWindowW(L"Shell_TrayWnd", NULL);
    if (!window)
    {
        result = StringCopy(L"FindWindowW failed for Shell_TrayWnd\n");
        goto exit;
    }

    windowThreadID = GetWindowThreadProcessId(window, NULL);
    if (windowThreadID == 0)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"GetWindowThreadProcessId failed for Shell_TrayWnd");
        goto exit;
    }

    hookProcedure = (HOOKPROC)GetProcAddress(LibraryModule, "ExplorerTrayMonitorCallWndProc");
    if (!hookProcedure)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"GetProcAddress failed for ExplorerTrayMonitorCallWndProc");
        goto exit;
    }

    hook = SetWindowsHookEx(WH_CALLWNDPROC, hookProcedure, LibraryModule, windowThreadID);
    if (!hook)
    {
        result = FormatErrorWithExplanation(GetLastError(), L"SetWindowsHookEx failed");
        goto exit;
    }

    TrayHook = hook;
    TrayThreadID = windowThreadID;

exit:
    if (window)
    {
        DestroyWindow(window);
    }

    return result;
}

typedef struct _NOTIFYICONDATA32 {
    DWORD cbSize;
    DWORD dwWnd;
    UINT uID;
    UINT uFlags;
    UINT uCallbackMessage;
    DWORD dwIcon;
    // Additional fields omitted due to differences between Windows versions
} NOTIFYICONDATA32, * PNOTIFYICONDATA32;

typedef struct _TRAYNOTIFYDATA {
    DWORD dwSignature;
    DWORD dwMessage;
    NOTIFYICONDATA32 notifyIconData;
} TRAYNOTIFYDATA, * PTRAYNOTIFYDATA;

static void HandleMessage(CWPSTRUCT* messageDetails);
static void HandleDiscordNotifyIconChange(HICON icon);

static void HandleMessage(CWPSTRUCT* messageDetails)
{
    UINT messageID = messageDetails->message;
    HWND messageDestinationWindow = messageDetails->hwnd;

    if (messageID == WM_COPYDATA)
    {
        WPARAM copyDataSourceWindowHandle = messageDetails->wParam;
        HWND copyDataSourceWindow = (HWND)copyDataSourceWindowHandle;

        LPARAM copyDataDetailsPointer = messageDetails->lParam;
        COPYDATASTRUCT* copyDataDetails = (COPYDATASTRUCT*)copyDataDetailsPointer;

        ULONG_PTR copyDataID = copyDataDetails->dwData;
        DWORD copyDataBufferSize = copyDataDetails->cbData;
        LPVOID copyDataBufferPointer = copyDataDetails->lpData;

        // ID should be 1 for the tray icon data
        if (copyDataID != 1)
        {
            DebugFormat(L"WM_COPYDATA message with unrecognized ID %lu, skipping\n", (unsigned long)copyDataID);
            return;
        }
        // Data size should be at least the size of the tray notify data structure
        if (copyDataBufferSize < sizeof(TRAYNOTIFYDATA))
        {
            DebugFormat(L"WM_COPYDATA message with invalid size %lu bytes\n", (unsigned long)copyDataBufferSize);
            return;
        }

        TRAYNOTIFYDATA* trayNotifyData = copyDataBufferPointer;
        NOTIFYICONDATA32* notifyIconData = &trayNotifyData->notifyIconData;

        HWND notifyWindow = (HWND)notifyIconData->dwWnd;
        WCHAR notifyWindowClass[256] = { 0 };
        WCHAR notifyWindowTitle[256] = { 0 };
        GetClassNameW(notifyWindow, notifyWindowClass, sizeof(notifyWindowClass) / sizeof(WCHAR));
        GetWindowTextW(notifyWindow, notifyWindowTitle, sizeof(notifyWindowTitle) / sizeof(WCHAR));

        DWORD notifyProcessID = 0;
        DWORD notifyProcessThreadID = GetWindowThreadProcessId(notifyWindow, &notifyProcessID);
        if (notifyProcessThreadID == 0)
        {
            DebugFormatError(GetLastError(), L"GetWindowThreadProcessId failed for notify window");
            return;
        }

        WCHAR notifyProcessPath[MAX_PATH] = { 0 };
        if (!GetProcessPathByID(notifyProcessID, notifyProcessPath)) {
            DebugFormatError(GetLastError(), L"GetProcessPathByID failed for notify process");
            return;
        }

        HICON icon = (HICON)notifyIconData->dwIcon;

        /*
        DebugFormat(
            L"WM_COPYDATA message from PID: %lu, path: '%s', window class: '%s', window title: '%s', icon: 0x%08lx\n",
            (unsigned long)notifyProcessID,
            notifyProcessPath,
            notifyWindowClass,
            notifyWindowTitle,
            (unsigned long)icon
        );
        */

        BOOL matchesProcessPath = StringIndexOf(notifyProcessPath, DISCORD_PROCESS_NAME, FALSE) >= 0;
        BOOL matchesWindowClass = StringEquals(notifyWindowClass, L"Electron_NotifyIconHostWindow");
        BOOL hasIcon = icon != NULL;

        if (!matchesProcessPath || !matchesWindowClass || !hasIcon)
        {
            /*
            DebugFormat(
                L"Skipping message, matchesProcessPath: %s, matchesWindowClass: %s, hasIcon: %s\n",
                matchesProcessPath ? L"true" : L"false",
                matchesWindowClass ? L"true" : L"false",
                hasIcon ? L"true" : L"false"
            );
            */
            return;
        }

        HandleDiscordNotifyIconChange(icon);
    }
}

static void HandleDiscordNotifyIconChange(HICON icon)
{
    // TODO Pack icon change into message and send it to the tool message window
}

DLLEXPORT LRESULT CALLBACK ExplorerTrayMonitorCallWndProc(int code, WPARAM isOnCurrentThread, LPARAM messageDetailsPointer)
{
    if (code >= 0)
    {
        CWPSTRUCT* messageDetails = (CWPSTRUCT*)messageDetailsPointer;
        HandleMessage(messageDetails);
    }
    return CallNextHookEx(NULL, code, isOnCurrentThread, messageDetailsPointer);
}

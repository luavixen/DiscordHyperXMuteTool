#pragma once

#define _CRT_SECURE_NO_WARNINGS

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#define NGENUITY_PROCESS_NAME L"NGenuity2Helper"
#define EXPLORER_PROCESS_NAME L"explorer"
#define DISCORD_PROCESS_NAME L"Discord"

#define MONITOR_MANAGED_FILENAME L"DiscordHyperXMuteMonitorManaged.dll"
#define MONITOR_UNMANAGED_FILENAME L"DiscordHyperXMuteMonitorUnmanaged.dll"

#define TOOL_MESSAGE_WINDOW_CLASS_NAME L"DiscordHyperXMuteTool_MessageWindow"

#define DEBUG_PREFIX L"[DiscordHyperXMuteMonitor] "

// Prints a debug message using OutputDebugStringW with a prefix
DLLEXPORT void WINAPI Debug(LPCWSTR message);

// Prints a formatted debug message using Debug
void DebugFormat(LPCWSTR format, ...);
// Prints a formatted debug message using Debug with a Windows error code at the end (don't use newline!)
void DebugFormatError(DWORD error, LPCWSTR format, ...);

// Initializes the heap, called by DllMain
BOOL InitializeHeap(void);
// Finalizes the heap, called by DllMain
BOOL FinalizeHeap(void);

// Allocates new memory on the heap
DLLEXPORT LPVOID WINAPI Allocate(SIZE_T size);
// Frees previously allocated memory on the heap
DLLEXPORT void WINAPI Free(LPVOID pointer);

// Returns the length of a string using wcslen
SIZE_T StringLength(LPCWSTR string);

// Compares two strings using wcscmp
BOOL StringEquals(LPCWSTR left, LPCWSTR right);
// Finds the index of a substring in a string
SSIZE_T StringIndexOf(LPCWSTR haystack, LPCWSTR needle, BOOL isCaseSensitive);

// Copies a string into a buffer (returns destination unmodified)
LPWSTR StringCopyInto(LPWSTR destination, LPCWSTR source);
// Copies a string into a new buffer (must free buffer)
LPWSTR StringCopy(LPCWSTR string);

// Concatenates multiple strings into a new buffer (must free buffer)
// Last argument must be NULL.
LPWSTR StringConcat(LPCWSTR first, ...);
// Formats a string into a new buffer (must free buffer)
LPWSTR StringFormat(LPCWSTR format, ...);

// Formats a message with a Windows error code at the end (don't use newline!) (must free buffer)
LPWSTR FormatErrorWithExplanation(DWORD error, LPCWSTR message);

// Stringifies a Windows error code, either Win32 or HRESULT (must free buffer)
DLLEXPORT LPWSTR WINAPI StringifyError(DWORD error);

// Gets the full path of the executable for a process
// Buffer must be at least MAX_PATH in size
BOOL GetProcessPath(HANDLE process, LPWSTR buffer);
// Gets the full path of the executable for a process by its ID
// Buffer must be at least MAX_PATH in size
BOOL GetProcessPathByID(DWORD processID, LPWSTR buffer);

// Module handle and file path to DiscordHyperXMuteMonitorUnmanaged.dll
extern HMODULE LibraryModule;
extern WCHAR LibraryPath[MAX_PATH];
// Module handle and file path for the currently running executable
extern HMODULE ExecutableModule;
extern WCHAR ExecutablePath[MAX_PATH];

// Injects this DLL into the NGenuity2Helper process and starts the microphone mute state monitor
// Returns an error message if something goes wrong during injection, or NULL if successful
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT LPWSTR WINAPI InjectMonitorIntoNgenuityProcess(DWORD processID);

// Checks if the explorer tray hook is currently valid
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT BOOL WINAPI ExplorerTrayIsHookValid(void);

// Removes the explorer tray hook
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT BOOL WINAPI ExplorerTrayRemoveHook(void);

// Sets the explorer tray hook which monitors the system tray icon for Discord
// Returns an error message if something goes wrong during hooking, or NULL if successful
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT LPWSTR WINAPI ExplorerTraySetHook(void);

// Registers the TOOL_MESSAGE_WINDOW_CLASS_NAME window class
// Returns an error message if something goes wrong, or NULL if successful
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT LPWSTR WINAPI RegisterToolMessageWindowClass(void);

// Type definition for a tool window procedure
typedef BOOL(CALLBACK* ToolWindowProcedure)(HWND, UINT, UINT64, INT64);

// Creates the tool message window with the TOOL_MESSAGE_WINDOW_CLASS_NAME window class, storing the procedure in the window
// Returns an error message if something goes wrong, or NULL if successful
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT LPWSTR WINAPI CreateToolMessageWindow(ToolWindowProcedure procedure, HWND* window);

// Destroys the tool message window
// Called from DiscordHyperXMuteTool using P/Invoke
DLLEXPORT BOOL WINAPI DestroyToolMessageWindow(HWND window);

DLLEXPORT DWORD WINAPI NgenuityMonitorBootstrapThreadProc(LPVOID parameter);

DLLEXPORT LRESULT CALLBACK ExplorerTrayMonitorCallWndProc(int code, WPARAM isOnCurrentThread, LPARAM messageDetailsPointer);

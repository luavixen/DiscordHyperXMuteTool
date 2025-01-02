#pragma once

#define _CRT_SECURE_NO_WARNINGS

#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif

DLLEXPORT void WINAPI Debug(LPCWSTR message);

void DebugFormat(LPCWSTR format, ...);
void DebugFormatError(DWORD error, LPCWSTR format, ...);

HRESULT InitializeHeap(void);
void FinalizeHeap(void);

DLLEXPORT LPVOID WINAPI Allocate(SIZE_T size);
DLLEXPORT void WINAPI Free(LPVOID pointer);

SIZE_T StringLength(LPCWSTR string);

BOOL StringEquals(LPCWSTR left, LPCWSTR right);
SSIZE_T StringIndexOf(LPCWSTR haystack, LPCWSTR needle, BOOL isCaseSensitive);

LPWSTR StringCopyInto(LPWSTR destination, LPCWSTR source);
LPWSTR StringCopy(LPCWSTR string);

LPWSTR StringConcat(LPCWSTR first, ...);
LPWSTR StringFormat(LPCWSTR format, ...);

LPWSTR FormatErrorWithExplanation(DWORD error, LPCWSTR message);

DLLEXPORT LPWSTR WINAPI StringifyError(DWORD error);

extern HMODULE LibraryModule;
extern WCHAR LibraryPath[MAX_PATH];
extern HMODULE ApplicationModule;
extern WCHAR ApplicationPath[MAX_PATH];

DWORD WINAPI NgenuityMonitorBootstrapThreadProc(LPVOID parameter);

typedef struct _SETTINGS {
    BOOL Enabled;
    BOOL RunOnStartup;
    BOOL SyncWithDiscord;
    DWORD OnMuteKey;
    DWORD OnUnmuteKey;
} SETTINGS;

extern SETTINGS Settings;

void ReadSettings(void);

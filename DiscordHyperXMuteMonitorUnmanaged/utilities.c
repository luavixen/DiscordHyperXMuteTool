#include "project.h"
#include <wchar.h>

static HANDLE heap = NULL;

BOOL InitializeHeap(void)
{
    if (!heap)
    {
        heap = HeapCreate(0, 0, 0);
        return heap ? TRUE : FALSE;
    }
    return TRUE;
}
BOOL FinalizeHeap(void)
{
    if (heap)
    {
        BOOL result = HeapDestroy(heap); heap = NULL;
        return result;
    }
    return TRUE;
}

DLLEXPORT LPVOID WINAPI Allocate(SIZE_T size)
{
    if (!heap) return NULL;
    return HeapAlloc(heap, HEAP_ZERO_MEMORY, size);
}
DLLEXPORT void WINAPI Free(LPVOID pointer)
{
    if (!heap) return;
    if (!pointer) return;
    HeapFree(heap, 0, pointer);
}

SIZE_T StringLength(LPCWSTR string)
{
    return string ? (SIZE_T)wcslen(string) : 0;
}

BOOL StringEquals(LPCWSTR left, LPCWSTR right)
{
    if (left == right) return TRUE;
    if (!left || !right) return FALSE;
    return wcscmp(left, right) == 0;
}

SSIZE_T StringIndexOf(LPCWSTR haystack, LPCWSTR needle, BOOL isCaseSensitive)
{
    if (!haystack || !needle) return -1;

    SIZE_T haystackLength = StringLength(haystack);
    SIZE_T needleLength = StringLength(needle);

    if (needleLength > haystackLength) return -1;
    if (needleLength == 0) return 0;

    for (SIZE_T i = 0; i <= haystackLength - needleLength; i++)
    {
        BOOL isMatching = TRUE;

        for (SIZE_T j = 0; j < needleLength; j++)
        {
            WCHAR haystackChar = haystack[i + j];
            WCHAR needleChar = needle[j];

            if (!isCaseSensitive)
            {
                haystackChar = towupper(haystackChar);
                needleChar = towupper(needleChar);
            }

            if (haystackChar != needleChar)
            {
                isMatching = FALSE;
                break;
            }
        }

        if (isMatching)
        {
            return (SSIZE_T)i;
        }
    }

    return -1;
}

LPWSTR StringCopyInto(LPWSTR destination, LPCWSTR source)
{
    if (!destination || !source) return NULL;
    wcscpy(destination, source);
    return destination;
}

LPWSTR StringCopy(LPCWSTR string)
{
    if (!string) return NULL;
    SIZE_T length = StringLength(string);
    LPWSTR buffer = Allocate((length + 1) * sizeof(WCHAR));
    return buffer ? StringCopyInto(buffer, string) : NULL;
}

LPWSTR StringConcat(LPCWSTR first, ...)
{
    if (!first) return NULL;

    va_list arguments1, arguments2;
    va_start(arguments1, first);
    va_copy(arguments2, arguments1);

    LPCWSTR current;

    // First, collect the total length of all the input strings

    SIZE_T length = StringLength(first);

    while ((current = va_arg(arguments1, LPCWSTR)) != NULL)
    {
        length += StringLength(current);
    }

    va_end(arguments1);

    // Then, allocate a buffer for the output string (plus null terminator)

    LPWSTR buffer = Allocate((length + 1) * sizeof(WCHAR));

    // Finally, concatenate all the input strings into the output string

    if (buffer)
    {
        buffer[0] = L'\0';

        wcscat(buffer, first);

        while ((current = va_arg(arguments2, LPCWSTR)) != NULL)
        {
            wcscat(buffer, current);
        }
    }

    va_end(arguments2);

    return buffer;
}

static INT FormatCalculateLength(LPCWSTR format, va_list arguments)
{
    // Calculate the length of the formatted string (plus null terminator)
    INT length = _vscwprintf(format, arguments);
    return length > 0 ? length + 1 : 0;
}
static LPWSTR FormatWithKnownLength(INT length, LPCWSTR format, va_list arguments)
{
    if (length <= 0)
    {
        LPWSTR buffer = Allocate(sizeof(WCHAR));
        return buffer ? buffer[0] = L'\0', buffer : NULL;
    }
    else
    {
        LPWSTR buffer = Allocate(length * sizeof(WCHAR));
        return buffer ? vswprintf(buffer, (SIZE_T)length, format, arguments), buffer : NULL;
    }
}

LPWSTR StringFormat(LPCWSTR format, ...)
{
    if (!format) return NULL;

    va_list arguments1, arguments2;
    va_start(arguments1, format);
    va_copy(arguments2, arguments1);

    INT length = FormatCalculateLength(format, arguments1);
    va_end(arguments1);

    LPWSTR buffer = FormatWithKnownLength(length, format, arguments2);
    va_end(arguments2);

    return buffer;
}

DLLEXPORT void WINAPI Debug(LPCWSTR message)
{
    if (!message) return;
    LPWSTR messagePrefixed = StringConcat(DEBUG_PREFIX, message, NULL);
    if (messagePrefixed)
    {
        OutputDebugStringW(messagePrefixed);
        Free(messagePrefixed);
    }
    else
    {
        OutputDebugStringW(message);
    }
}

void DebugFormat(LPCWSTR format, ...)
{
    if (!format) return;

    va_list arguments1, arguments2;
    va_start(arguments1, format);
    va_copy(arguments2, arguments1);

    INT length = FormatCalculateLength(format, arguments1);
    va_end(arguments1);

    LPWSTR buffer = FormatWithKnownLength(length, format, arguments2);
    va_end(arguments2);

    if (buffer)
    {
        Debug(buffer);
        Free(buffer);
    }
}

void DebugFormatError(DWORD error, LPCWSTR format, ...)
{
    if (!format) return;

    va_list arguments1, arguments2;
    va_start(arguments1, format);
    va_copy(arguments2, arguments1);

    INT length = FormatCalculateLength(format, arguments1);
    va_end(arguments1);

    LPWSTR messageBuffer = FormatWithKnownLength(length, format, arguments2);
    va_end(arguments2);

    if (messageBuffer)
    {
        LPWSTR errorBuffer = StringifyError(error);

        DebugFormat(L"%s: %s", messageBuffer, errorBuffer);

        Free(errorBuffer);
        Free(messageBuffer);
    }
}

LPWSTR FormatErrorWithExplanation(DWORD error, LPCWSTR message)
{
    if (!message) return NULL;
    LPWSTR errorBuffer = StringifyError(error);
    LPWSTR formattedBuffer = StringFormat(L"%s: %s", message, errorBuffer);
    Free(errorBuffer);
    return formattedBuffer;
}

DLLEXPORT LPWSTR WINAPI StringifyError(DWORD error)
{
    if (FAILED(error) && HRESULT_FACILITY(error) == FACILITY_WIN32)
    {
        error = HRESULT_CODE(error);
    }
    WCHAR buffer[256] = { 0 };
    DWORD count = FormatMessageW(
        FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
        NULL,
        error,
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        buffer,
        sizeof(buffer) / sizeof(WCHAR),
        NULL
    );
    return count > 0
        ? StringFormat(L"0x%08X %s", error, buffer)
        : StringFormat(L"0x%08X\n", error);
}

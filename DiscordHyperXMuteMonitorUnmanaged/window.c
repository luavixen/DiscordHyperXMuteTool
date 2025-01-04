#include "project.h"

static LRESULT CALLBACK ToolMessageWindowWndProc(HWND window, UINT message, WPARAM wparam, LPARAM lparam) {
    ToolWindowProcedure procedure = (ToolWindowProcedure)GetWindowLongPtrW(window, GWLP_USERDATA);
    if (procedure && procedure(window, message, wparam, lparam)) return TRUE;
    else return DefWindowProcW(window, message, wparam, lparam);
}

DLLEXPORT LPWSTR WINAPI RegisterToolMessageWindowClass(void)
{
    WNDCLASSW windowClass = {
        .style = 0,
        .lpfnWndProc = ToolMessageWindowWndProc,
        .cbClsExtra = 0,
        .cbWndExtra = sizeof(ToolWindowProcedure),
        .hInstance = ExecutableModule,
        .lpszMenuName = NULL,
        .lpszClassName = TOOL_MESSAGE_WINDOW_CLASS_NAME,
    };

    ATOM windowClassAtom = RegisterClassW(&windowClass);
    if (!windowClassAtom)
    {
        return FormatErrorWithExplanation(GetLastError(), L"RegisterClassW failed");
    }

    return NULL;
}

DLLEXPORT LPWSTR WINAPI CreateToolMessageWindow(ToolWindowProcedure procedure, HWND* window)
{
    HWND handle = CreateWindowExW(
        0,
        TOOL_MESSAGE_WINDOW_CLASS_NAME,
        TOOL_MESSAGE_WINDOW_CLASS_NAME,
        0,
        0, 0,
        0, 0,
        HWND_MESSAGE,
        NULL,
        ExecutableModule,
        NULL
    );
    if (!handle)
    {
        return FormatErrorWithExplanation(GetLastError(), L"CreateWindowExW failed");
    }

    SetWindowLongPtrW(handle, GWLP_USERDATA, (LONG_PTR)procedure);

    *window = handle;

    return NULL;
}

DLLEXPORT BOOL WINAPI DestroyToolMessageWindow(HWND window)
{
    return DestroyWindow(window);
}

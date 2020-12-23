#include <stdint.h>

#define STR(x) #x
#define XSTR(x) STR(x)

#if defined(_WIN64)
#define NATIVE_API(x) extern "C" __declspec(dllexport) x __fastcall
#elif defined(_WIN32)
#define NATIVE_API(x) extern "C" __declspec(dllexport) x __cdecl
#else
#define NATIVE_API(x) extern "C" x __attribute__ ((visibility("default")))
#endif

#if defined(_WIN32)

#ifndef WINAPI
#define WINAPI __stdcall
#endif

typedef int BOOL;
typedef uint32_t DWORD;

#define JAVA_NAMESPACE rtmath_utilities_FunctionsImport

//
// DLL Entry Point
//
BOOL WINAPI DllMain(void *instance, DWORD reason, void *reserved)
{
    return 1;
}

#endif /* #if defined(_WIN32) || defined(_WIN64) */

static inline double test_function(double a, double b)
{
#if !defined(CODEVERSION)
#error The CODEVERSION variable is undefined.
#endif
#pragma message ("The value of CODEVERSION: " XSTR(CODEVERSION))
#if CODEVERSION == 1
    return a + b;
#elif CODEVERSION == 2
    return a * b;
#else
#error Unsupported CODEVERSION value.
#endif
}


/**
 * We are creating 3 entry points for our exported functions.
 * For C#, for Java and for the slightly faster JavaCritical API.
 */

#define FN0(return_type, java_ns, name) FN1(return_type, java_ns, name)
#define FN(return_type, name) FN0(return_type, JAVA_NAMESPACE, name)


 /* Create .NET implementation */
#define ARGS(...) (__VA_ARGS__)
#define FN1(return_type, java_ns, name) NATIVE_API(return_type) name
#define DOTNET
#include "FunctionsNative.h"
#undef DOTNET
#undef FN1


 /* Create JavaCritical JNI implementation */
#define FN1(return_type, java_ns, name) NATIVE_API(return_type) JavaCritical_ ## java_ns ## _ ## name
#include "FunctionsNative.h"
#undef FN1


 /* Create Java Native Interface implementation */
#define FN1(return_type, java_ns, name) NATIVE_API(return_type) Java_ ## java_ns ## _ ## name
#undef ARGS
#define ARGS(...) (void *env, void *obj,  __VA_ARGS__)
#include "FunctionsNative.h"

#pragma once

#ifdef BCRYPT2_EXPORTS
#define BCRYPT2_API __declspec(dllexport)
#else
#define BCRYPT2_API __declspec(dllimport)
#endif

// Windows-related definitions
typedef long NTSTATUS;
typedef unsigned int ULONG;
typedef void VOID, * PVOID, * LPVOID;

typedef wchar_t* LPWSTR, * PWSTR;
typedef const wchar_t* LPCWSTR;
typedef unsigned char UCHAR, * PUCHAR;

typedef void* PVOID;

// bcrypt-related definitions
#define BCRYPT_AES_ALGORITHM  "A\0E\0S\0\0" // UTF16 of AES
#define BCRYPT_CHAINING_MODE  "C\0h\0a\0i\0n\0i\0n\0g\0M\0o\0d\0e\0\0" // UTF16 of ChainingMode
#define BCRYPT_CHAIN_MODE_CBC "C\0h\0a\0i\0n\0i\0n\0g\0M\0o\0d\0e\0C\0B\0C\0\0" // UTF16 of ChainingModeCBC

#define STATUS_SUCCESS             ((NTSTATUS)0x00000000L)
#define STATUS_INVALID_HANDLE      ((NTSTATUS)0xC0000008L)
#define STATUS_INVALID_PARAMETER   ((NTSTATUS)0xC000000DL)
#define STATUS_NO_MEMORY           ((NTSTATUS)0xC0000017L)
#define STATUS_BUFFER_TOO_SMALL    ((NTSTATUS)0xC0000023L)
#define STATUS_NOT_SUPPORTED       ((NTSTATUS)0xC00000BBL)
#define STATUS_INVALID_BUFFER_SIZE ((NTSTATUS)0xC0000206L)
#define STATUS_NOT_FOUND           ((NTSTATUS)0xC0000225L)
#define STATUS_DECRYPTION_FAILED   ((NTSTATUS)0xC000028BL)

typedef PVOID BCRYPT_HANDLE;
typedef PVOID BCRYPT_ALG_HANDLE;
typedef PVOID BCRYPT_KEY_HANDLE;

extern BCRYPT2_API NTSTATUS BCryptOpenAlgorithmProvider(BCRYPT_ALG_HANDLE* phAlgorithm, LPCWSTR pszAlgId, LPCWSTR pszImplementation, ULONG dwFlags);
extern BCRYPT2_API NTSTATUS BCryptCloseAlgorithmProvider(BCRYPT_ALG_HANDLE hAlgorithm, ULONG dwFlags);
extern BCRYPT2_API NTSTATUS BCryptGenerateSymmetricKey(BCRYPT_ALG_HANDLE hAlgorithm, BCRYPT_KEY_HANDLE * phKey, PUCHAR pbKeyObject, ULONG cbKeyObject, PUCHAR pbSecret, ULONG cbSecret, ULONG dwFlags);
extern BCRYPT2_API NTSTATUS BCryptDestroyKey(BCRYPT_KEY_HANDLE hKey);
extern BCRYPT2_API NTSTATUS BCryptSetProperty(BCRYPT_HANDLE hObject, LPCWSTR pszProperty, PUCHAR pbInput, ULONG cbInput, ULONG dwFlags);
extern BCRYPT2_API NTSTATUS BCryptDecrypt(BCRYPT_KEY_HANDLE hKey, PUCHAR pbInput, ULONG cbInput, VOID* pPaddingInfo, PUCHAR pbIV, ULONG cbIV, PUCHAR pbOutput, ULONG cbOutput, ULONG* pcbResult, ULONG dwFlags);

#include <stdlib.h>
#include <string.h>
#include <openssl/evp.h>
#include "bcrypt.h"

// Local definitions
#define PADDING_ENABLED 1

typedef struct _CIPHER_KEY
{
    EVP_CIPHER_CTX *ctx;
    UCHAR *key;
    const EVP_CIPHER *cipher;
    int key_length;
} CIPHER_KEY;

typedef const EVP_CIPHER *(*CIPHER_FACTORY)();

typedef struct _CIPHER_ITEM
{
    int key_bits;
    CIPHER_FACTORY cipher_factory;
} CIPHER_INFO;

#define CIPHER_INFOS_LENGTH 3
CIPHER_INFO CipherInfos[CIPHER_INFOS_LENGTH] =
{
    { 128, &EVP_aes_128_cbc },
    { 192, &EVP_aes_192_cbc },
    { 256, &EVP_aes_256_cbc }
};

int strcmpU16(const int16_t *lpString1, const int16_t *lpString2)
{
    if (lpString1 == NULL)
    {
        return -1;
    }

    if (lpString2 == NULL)
    {
        return 1;
    }

    while (*lpString1 != 0 && *lpString2 != 0 && *lpString1 == *lpString2)
    {
        ++lpString1;
        ++lpString2;
    }

    if (*lpString1 == *lpString2)
    {
        return 0;
    }
    else if (*lpString1 < *lpString2)
    {
        return -1;
    }
    else
    {
        return 1;
    }
}

NTSTATUS BCryptOpenAlgorithmProvider(BCRYPT_ALG_HANDLE *phAlgorithm, LPCWSTR pszAlgId, LPCWSTR pszImplementation, ULONG dwFlags)
{
    if ((NULL == phAlgorithm) || (NULL == pszAlgId) || (NULL != pszImplementation) || (0 != dwFlags))
    {
        return STATUS_INVALID_PARAMETER;
    }

    if (0 != strcmpU16((int16_t *)pszAlgId, (int16_t*)BCRYPT_AES_ALGORITHM))
    {
        return STATUS_NOT_FOUND;
    }

    EVP_CIPHER_CTX *ctx = EVP_CIPHER_CTX_new();

    if (NULL == ctx)
    {
        return STATUS_NO_MEMORY;
    }

    *phAlgorithm = (BCRYPT_ALG_HANDLE)ctx;

    return STATUS_SUCCESS;
}

NTSTATUS BCryptCloseAlgorithmProvider(BCRYPT_ALG_HANDLE hAlgorithm, ULONG dwFlags)
{
    if (NULL == hAlgorithm)
    {
        return STATUS_INVALID_HANDLE;
    }

    if (0 != dwFlags)
    {
        return STATUS_INVALID_PARAMETER;
    }

    EVP_CIPHER_CTX_free((EVP_CIPHER_CTX*)hAlgorithm);

    return STATUS_SUCCESS;
}

NTSTATUS BCryptGenerateSymmetricKey(BCRYPT_ALG_HANDLE hAlgorithm, BCRYPT_KEY_HANDLE *phKey, PUCHAR pbKeyObject, ULONG cbKeyObject, PUCHAR pbSecret, ULONG cbSecret, ULONG dwFlags)
{
    if (NULL == hAlgorithm)
    {
        return STATUS_INVALID_HANDLE;
    }

    if ((NULL == phKey) || (NULL != pbKeyObject) || (0 != cbKeyObject) || (NULL == pbSecret) || (0 == cbSecret) || (0 != dwFlags))
    {
        return STATUS_INVALID_PARAMETER;
    }

    const int keyBits = cbSecret * 8;
    CIPHER_INFO* cipher_info = &CipherInfos[0];
    for (int cipher_idx = 0; cipher_idx < CIPHER_INFOS_LENGTH; ++cipher_idx, cipher_info++)
    {
        if (keyBits == cipher_info->key_bits)
        {
            break;
        }
    }

    if (NULL == cipher_info)
    {
        return STATUS_INVALID_PARAMETER;
    }

    CIPHER_KEY *cipher_key = (CIPHER_KEY *)calloc(1, sizeof(CIPHER_KEY));
    if (NULL == cipher_key)
    {
        return STATUS_NO_MEMORY;
    }

    cipher_key->key = calloc(cbSecret, 1);
    if (NULL == cipher_key->key)
    {
        free(cipher_key);
        return STATUS_NO_MEMORY;
    }

    memcpy(cipher_key->key, pbSecret, cbSecret);
    cipher_key->key_length = cbSecret;

    cipher_key->ctx = (EVP_CIPHER_CTX*)hAlgorithm;
    cipher_key->cipher = cipher_info->cipher_factory();

    *phKey = (BCRYPT_KEY_HANDLE)cipher_key;

    return STATUS_SUCCESS;
}

NTSTATUS BCryptDestroyKey(BCRYPT_KEY_HANDLE hKey)
{
    if (NULL == hKey)
    {
        return STATUS_INVALID_HANDLE;
    }

    free(((CIPHER_KEY *)hKey)->key);
    free(hKey);

    return STATUS_SUCCESS;
}

NTSTATUS BCryptSetProperty(BCRYPT_HANDLE hObject, LPCWSTR pszProperty, PUCHAR pbInput, ULONG cbInput, ULONG dwFlags)
{
    if (NULL == hObject)
    {
        return STATUS_INVALID_HANDLE;
    }

    if ((NULL == pszProperty) || (NULL == pbInput) || (0 == cbInput) || (0 != dwFlags))
    {
        return STATUS_INVALID_PARAMETER;
    }

    if (0 != strcmpU16((int16_t*)pszProperty, (int16_t*)BCRYPT_CHAINING_MODE))
    {
        return STATUS_NOT_SUPPORTED;
    }

    if (0 != strcmpU16((int16_t*)pbInput, (int16_t*)BCRYPT_CHAIN_MODE_CBC)) // TODO , sizeof(BCRYPT_CHAIN_MODE_CBC) / sizeof(wchar_t)))
    {
        return STATUS_NOT_SUPPORTED;
    }

    // DO nothing. CBC is the only supported mode.

    return STATUS_SUCCESS;
}

NTSTATUS BCryptDecrypt(BCRYPT_KEY_HANDLE hKey, PUCHAR pbInput, ULONG cbInput, VOID *pPaddingInfo, PUCHAR pbIV, ULONG cbIV, PUCHAR pbOutput, ULONG cbOutput, ULONG *pcbResult, ULONG dwFlags)
{
    if (NULL == hKey)
    {
        return STATUS_INVALID_HANDLE;
    }

    if ((NULL == pbInput) || (0 == cbInput)
        || (NULL != pPaddingInfo)
        || (NULL == pbIV) || (0 == cbIV)
        || (NULL == pcbResult)
        || (1 != dwFlags))
    {
        return STATUS_INVALID_PARAMETER;
    }

    CIPHER_KEY *cipher_key = (CIPHER_KEY *)hKey;
    ULONG maxOutputSize = cbInput + EVP_CIPHER_block_size(cipher_key->cipher);

    if ((NULL == pbOutput) || (0 == cbOutput))
    {
        // calculate size of output and return it
        *pcbResult = maxOutputSize;
        return STATUS_SUCCESS;
    }

    if (cbOutput < maxOutputSize)
    {
        return STATUS_BUFFER_TOO_SMALL;
    }

    // Check lengths of key and IV
    if (!EVP_DecryptInit_ex(cipher_key->ctx, cipher_key->cipher, NULL, NULL, NULL))
    {
        return STATUS_DECRYPTION_FAILED;
    }

    OPENSSL_assert(EVP_CIPHER_CTX_key_length(cipher_key->ctx) == cipher_key->key_length);
    OPENSSL_assert(EVP_CIPHER_CTX_iv_length(cipher_key->ctx) == cbIV);

    if (!EVP_DecryptInit_ex(cipher_key->ctx, NULL, NULL, cipher_key->key, pbIV))
    {
        return STATUS_DECRYPTION_FAILED;
    }

    if (!EVP_CIPHER_CTX_set_padding(cipher_key->ctx, PADDING_ENABLED))
    {
        return STATUS_DECRYPTION_FAILED;
    }

    // Decode
    int outl = 0;
    int totalLength = 0;
    if (!EVP_DecryptUpdate(cipher_key->ctx, pbOutput, &outl, pbInput, cbInput))
    {
        return STATUS_DECRYPTION_FAILED;
    }

    totalLength += outl;

    if (!EVP_DecryptFinal_ex(cipher_key->ctx, pbOutput + totalLength, &outl))
    {
        return STATUS_DECRYPTION_FAILED;
    }

    totalLength += outl;

    *pcbResult = totalLength;
    return STATUS_SUCCESS;
}

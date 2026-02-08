#pragma once
#include <wincodec.h>
#include <wrl/client.h>
#pragma comment(lib, "windowscodecs.lib")

//C++の名前マンダリングを止める
#ifdef __cplusplus
extern "C" {
#endif

    //外部公開するという意味
    __declspec(dllexport)
        HRESULT  LoadImageWIC(const wchar_t* path, IWICBitmap** outBitmapHandle, UINT size);


#ifdef __cplusplus
}
#endif


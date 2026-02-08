#include "pch.h"
#include "wicTest.h"
#include <windows.h>
#include <comdef.h>

#include <wincodec.h>
#include <wrl/client.h>


using Microsoft::WRL::ComPtr; 




ComPtr<IWICImagingFactory> g_factory;



ComPtr<IWICBitmapSource> ScaleBitmap(
    IWICImagingFactory* factory,
    IWICBitmapSource* src,
    UINT maxSize
);


HRESULT  LoadImageWIC(const wchar_t* path, IWICBitmap** outBitmapPtr, UINT size)
{
    if (!outBitmapPtr) return E_POINTER;
    *outBitmapPtr = nullptr;

    HRESULT hr;

    //もし前のデータが残っったら破棄
    //
    g_factory.Reset();

    //ここでwic本体を生成
    hr=CoCreateInstance(
        CLSID_WICImagingFactory,
        nullptr,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(g_factory.GetAddressOf()) //ここでfactoryのポインタを渡す
    );
    if (FAILED(hr))
        return hr;


    ComPtr<IWICBitmapDecoder> decoder;

    //画像ファイルを開く

     hr = g_factory->CreateDecoderFromFilename(
        path,
        nullptr,
        GENERIC_READ,
        WICDecodeMetadataCacheOnLoad,
        decoder.GetAddressOf()
    );
     if (FAILED(hr)) return hr; 




    ComPtr<IWICBitmapFrameDecode> frame;

    //複数フレームある場合があるから先頭フレームを取得
    hr= decoder->GetFrame(0, frame.GetAddressOf());
    if (FAILED(hr)) return hr;


    ComPtr<IWICFormatConverter> converter;
    auto scaled = ScaleBitmap(g_factory.Get(), frame.Get(), size);
    if (!scaled)
        return E_FAIL;
    //変換
    //32bit,rgba,direct2d形式
    hr=g_factory->CreateFormatConverter(converter.GetAddressOf());

    if (FAILED(hr))
        return hr;


    hr = converter->Initialize(
        scaled.Get(),
        GUID_WICPixelFormat32bppPBGRA,
        WICBitmapDitherTypeNone,
        nullptr,
        0.0,
        WICBitmapPaletteTypeCustom
    );

    if (FAILED(hr))
        return hr;


    hr = g_factory->CreateBitmapFromSource(
        converter.Get(),
        WICBitmapCacheOnLoad,
        reinterpret_cast<IWICBitmap**>(outBitmapPtr)  
    );


    return hr;
}
ComPtr<IWICBitmapSource> ScaleBitmap(
    IWICImagingFactory* factory,
    IWICBitmapSource* src,
    UINT maxSize
)
{
    UINT w, h;
    src->GetSize(&w, &h);

    float scale = min(
        (float)maxSize / w,
        (float)maxSize / h
    );

    if (scale >= 1.0f)
        return src; // 縮小不要ならそのまま返す

    UINT newW = (UINT)(w * scale);
    UINT newH = (UINT)(h * scale);

    ComPtr<IWICBitmapScaler> scaler;
    factory->CreateBitmapScaler(&scaler);

    scaler->Initialize(
        src,
        newW,
        newH,
        WICBitmapInterpolationModeFant
    );

    return scaler; 
}


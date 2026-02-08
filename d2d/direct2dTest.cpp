#include "pch.h"
#include "direct2dTest.h"
#include <wincodec.h>
#include <wrl/client.h>
#include <unordered_map>
using Microsoft::WRL::ComPtr;
Microsoft::WRL::ComPtr<ID2D1Factory> g_factory;



struct Renderer
{
    HWND hwnd;
    ComPtr<ID2D1HwndRenderTarget> g_pRenderTarget;
    ComPtr<ID2D1Bitmap> g_d2dBitmap;
    ComPtr<IWICBitmap> g_bitmap; //bitmapの描画先

    Renderer(HWND h) : hwnd(h) {}

};
std::unordered_map<HWND, Renderer*> renderers;


using Microsoft::WRL::ComPtr;





extern "C" __declspec(dllexport)
HRESULT InitD2D(HWND hWnd, wchar_t* path)
{
    if (renderers.count(hWnd))
        return S_OK;

    auto r = new Renderer(hWnd);
    r->hwnd = hWnd;
    renderers[hWnd] = r;
    


    HRESULT hr = S_OK;

    if (!g_factory)
    {
        hr = D2D1CreateFactory(
            D2D1_FACTORY_TYPE_SINGLE_THREADED,
            g_factory.GetAddressOf()
        );
        if (FAILED(hr)) return hr;
    }


    //クライアント領域サイズを取得
    //枠やタイトルバーを除いた描画できる面積

    RECT rc;
    GetClientRect(hWnd, &rc);


    hr = g_factory->CreateHwndRenderTarget(//このウィンドウに描画
        D2D1::RenderTargetProperties(),//描画設定


        
        D2D1::HwndRenderTargetProperties(//描画領域を設定
            hWnd,
            D2D1::SizeU(rc.right - rc.left, rc.bottom - rc.top)
        ),
        &r->g_pRenderTarget
    );
    if (FAILED(hr)) return hr;


    hr = LoadImageWIC(path, r->g_bitmap.GetAddressOf(), 12000);
    if (FAILED(hr) || !r->g_bitmap)
        return hr;

    double dpiX = 0, dpiY = 0;
    r->g_bitmap->GetResolution(&dpiX, &dpiY);



    r->g_pRenderTarget->CreateBitmapFromWicBitmap(
        r->g_bitmap.Get(),
        nullptr,
        r->g_d2dBitmap.GetAddressOf()
    );


    return hr;
}

extern "C" __declspec(dllexport)
void OnResize(HWND hWnd,UINT width, UINT height)
{
    auto r = renderers[hWnd];


    if (r->g_pRenderTarget)
    {
        r->g_pRenderTarget->Resize(
            D2D1::SizeU(width, height)
        );
        r->g_pRenderTarget->SetTransform(
            D2D1::Matrix3x2F::Identity()
        );
    }

}

extern "C" __declspec(dllexport)
D2D1_SIZE_U getPixelSize(HWND hWnd)
{
    auto it = renderers.find(hWnd);
    if (it == renderers.end())
        return D2D1::SizeU(0, 0);

    auto r = it->second;
    if (!r)
        return D2D1::SizeU(0, 0);

    if (!r->g_d2dBitmap)
        return D2D1::SizeU(0, 0);

    return r->g_d2dBitmap->GetPixelSize();
}


extern "C" __declspec(dllexport)
void ShutdownRenderer(HWND hWnd)
{
    delete renderers[hWnd];
    renderers.erase(hWnd);
}


extern "C" __declspec(dllexport)
void  OnPaintwithDirect(HWND hWnd,DrawParams params)
{
    auto r = renderers[hWnd];

    PAINTSTRUCT ps;
    BeginPaint(hWnd, &ps);

    r->g_pRenderTarget->BeginDraw();
    r->g_pRenderTarget->Clear(D2D1::ColorF(D2D1::ColorF::White));



    D2D1::Matrix3x2F transform;
     transform =
        D2D1::Matrix3x2F::Scale(params.scale, params.scale) *
        D2D1::Matrix3x2F::Translation(params.posX, params.posY);





    D2D1::Matrix3x2F inv = transform;
    inv.Invert();




    r->g_pRenderTarget->SetTransform(transform);

    r->g_pRenderTarget->DrawBitmap(r->g_d2dBitmap.Get());

    // 変換を戻す
    r->g_pRenderTarget->SetTransform(
        D2D1::Matrix3x2F::Identity()
    );

    r->g_pRenderTarget->EndDraw();
    EndPaint(hWnd, &ps);



}

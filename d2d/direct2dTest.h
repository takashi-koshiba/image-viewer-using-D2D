
#pragma once

#include <windows.h> //windows api使用
#include <d2d1.h>  //direct2d本体
#include "wicTest.h"

#pragma comment(lib, "d2d1.lib") //リンカにd2d1使用

// 描画パラメータ構造体
struct DrawParams
{
    float scale;
    float posX;
    float posY;

    /*
    float cx;
    float cy;
    bool isScaling;*/
};


//C++の名前マンダリングを止める
#ifdef __cplusplus
extern "C" {
#endif

    //外部公開するという意味
    __declspec(dllexport)
        HRESULT InitD2D(HWND hWnd, wchar_t* path);
    
    __declspec(dllexport)
        void  OnPaintwithDirect(HWND hWnd,DrawParams params);

    __declspec(dllexport)
        D2D1_SIZE_U  getPixelSize(HWND hWnd );

    __declspec(dllexport)
        void   ShutdownRenderer(HWND hWnd);

    __declspec(dllexport)
        void OnResize(HWND hWnd,UINT width, UINT height);

#ifdef __cplusplus
}
#endif

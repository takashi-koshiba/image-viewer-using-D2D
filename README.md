# image-viewer-using-D2D


<img src=test.png>
<p>Direct2Dを使った複数画像対応のビューアです。<br>
  以前に作った<a href="https://github.com/takashi-koshiba/image-viewer">image-viewer</a>と使い勝手を変えずにDirect2Dに変更してパフォーマンスを向上させました。<br>
  
</p>
<br>
<h2>起動方法</h2>
<ul>
  <li>
    <a href="https://github.com/takashi-koshiba/image-viewer-using-D2D/releases">ここ</a>ここからリポジトリをクローンします。<br>
  </li>
<li>
   画像の規定アプリをこのアプリに変更して画像をファイルを開くか、exeファイルを起動してください。
</li>
 
</p>
  
</ul>
<h2>機能</h2>
<ul>
<li>マウスで画像をドラッグすると平行移動ができます。</li>
<li>マウスホイールを操作するとカーソルを中心にして拡大/縮小ができます。<br></li>
<li>右クリックでアプリが終了します。<br></li>
<li>フォームに画像をD&Dすると画像が表示されます。<br>
  複数画像同時にD&Dすることもできます。<br></li>
</ul>

<h2>ビルドする場合</h2>
<p>/d2d と/form は別々にビルドしてください。<br>
/d2dをビルドすると「imagelodar2.dll」が生成されます。<br>
 それを/form内のimageViewer.exeと同じディレクトリに配置してください。</p>

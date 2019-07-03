# Text-Extraction-Web
An web application that can extract text of an image uploaded by users

![](https://raw.githubusercontent.com/Lei1025/ImgRepo/master/myblog/c92d40a4-dff3-07d7-7278-f1d6b024cb2d.png)

## Image Upload

- [x] 圖片上傳時，應適當的做一些圖片格式/大小的檢查；
  - Bootstrap 4 input button style
  - Check file size by js then pop up warning if exceeded
- [x] 編寫圖片上傳功能，只需支持單个圖片上傳，只需支持常見圖片格式，如: JPEG，PNG等
- [x] 上傳圖片過程中，需要有適當的加載中動效/進度條，以便追踪上傳進度
  - bootstrap progress bar with js control

## OCR

  - [ ] 成功上傳圖片后，需使用第3方平台接口/自己封裝的接口，來進行OCR的識別，把圖片中的文字抽取出來；
  - [ ] 理想狀態，是同時可以識別手寫文字和數學公式；
  - [ ] 數學公式，識別后，轉化成LaTex碼；
  - [x] 在進行OCR識別時，需要適當的給出進度條給用戶，以便追踪識別狀態；
  - [x] 在完成OCR識別后，需要適當的把結果呈現出來；建議使用以下方式來呈現：
> 可以考慮使用的OCR SDK有：MathPix,ABBYY,Infty Reader；也可以使用別的平台SDK;


# Snapshot

Blog: [Selenium - FullPage ScreenShot](https://partypeopleland.github.io/artblog/2022/09/05/Selenium-FullPage-ScreenShot/)   
使用 Selenium 4 + ChromeDriver 進行網頁截圖


## 重點

1. 訪問頁面後，透過 js 的 `document.readyState` 判斷是否已經載入完畢
2. 載入完畢後，從前端回傳網頁高度並重新設定 `headless` 模式下的寬、高
3. 經由 Chrome DevTool Protocol Command 截圖

## 注意事項

本專案為 PoC 實驗性質，請勿直接使用於 Production 環境
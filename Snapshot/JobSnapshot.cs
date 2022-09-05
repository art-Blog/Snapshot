using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Snapshot
{
    internal class JobSnapshot : JobBase
    {
        protected override void Main()
        {
            var thread = CreateStaThread(SnapshopScreen);
            thread.Start();
            thread.Join();
        }

        private static Thread CreateStaThread(Action action)
        {
            var thread = new Thread(() => action());
            thread.SetApartmentState(ApartmentState.STA);
            return thread;
        }

        protected override void Initial(string[] args)
        {
            TargetSite = ConfigurationManager.AppSettings["TargetSite"];
            WriteHistoryLog($"[Initial] TargetSite:{TargetSite}");

            OutputFolder = ConfigurationManager.AppSettings["OutputFolder"];
            WriteHistoryLog($"[Initial] OutputFolder:{OutputFolder}");

            Arguments = ValidateArgs(args);
            WriteHistoryLog($"[Initial] Arguments:{string.Join(",", Arguments)}");
        }

        private static string[] ValidateArgs(string[] args)
        {
            return args.Any() ? args : new[] { "10363227" };
        }

        protected override string GetTargetUrl(string argument)
        {
            // 讀取網頁
            // https://www.rakuten.com.tw/shop/jsfamily/product/AA7-114110-2/
            // https://www.momoshop.com.tw/goods/GoodsDetail.jsp?i_code=10363227
            return $"{TargetSite}/goods/GoodsDetail.jsp?i_code={argument}";
        }

        private void SnapshopScreen()
        {
            StartTimeRecord();

            int width = 1280;
            int height = 768;

            // headless 模式
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            options.AddArgument($"--window-size={width}x{height}");
            var driver = new ChromeDriver(options);

            try
            {
                foreach (var arg in Arguments)
                {
                    StartCaptureTiming();

                    var targetUrl = GetTargetUrl(arg);
                    driver.Navigate().GoToUrl(targetUrl);

                    // 等候網頁載入完畢，取得實際上網頁的高度
                    // ChromeDriver : ChromiumDriver : WebDriver : IJavaScriptExecutor
                    IJavaScriptExecutor js = driver;
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    wait.IgnoreExceptionTypes(typeof(InvalidOperationException));
                    // wait default 500ms
                    wait.Until(wd => (bool)js.ExecuteScript("return document.readyState === 'complete'"));
                    
                    var docHeight = driver.ExecuteScript("return Math.max(window.innerHeight,document.body.scrollHeight,document.documentElement.scrollHeight)").ToString();
                    int.TryParse(docHeight, out height);

                    // 重新指定網頁寬高
                    driver.Manage().Window.Size = new Size(width, height);

                    // 呼叫 Chrome DevTool Protocol Command 做截圖，回傳 base64 string
                    //REF:https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-captureScreenshot
                    var screenshot = driver.ExecuteCdpCommand("Page.captureScreenshot", new Dictionary<string, object>()
                    {
                        // Image compression format (defaults to png).Allowed Values: jpeg, png, webp
                        { "format", "jpeg" },
                        // Compression quality from range [0..100] (jpeg only).
                        { "quality", 70 },
                        // Capture the screenshot beyond the viewport. Defaults to false
                        { "captureBeyondViewport", true },
                        // Capture the screenshot from the surface, rather than the view. Defaults to true.
                        { "fromSurface", true },
                        { "width", width },
                        { "height", height },
                    });
                    var base64Str = ((Dictionary<string, object>)screenshot)["data"].ToString();
                    var img = Base64StringToImage(base64Str);

                    // 添加浮水印
                    ApplyWaterMark(img);

                    // 儲存圖片
                    img.Save($"{OutputFolder}\\{arg}.jpg", ImageFormat.Jpeg);

                    // 紀錄
                    EndCaptureTiming($"[Target]:{targetUrl} - [Height]:{height}");

                    SuccessCount++;
                }
            }
            catch (Exception e)
            {
                WriteHistoryLog(e.Message);
                FailureCount++;
            }

            driver.Close();

            EndTimeRecord();
        }

        private static Bitmap Base64StringToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] buffer = Convert.FromBase64String(base64String);

            MemoryStream stream = null;
            Bitmap bitmap;
            //建立副本
            var data = (byte[])buffer.Clone();
            try
            {
                stream = new MemoryStream(data);
                //設定資料流位置
                stream.Position = 0;
                //建立副本
                bitmap = new Bitmap(Image.FromStream(stream));
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            return bitmap;
        }

        private static void ApplyWaterMark(Image bmp)
        {
            var font = new Font("arial", 16, FontStyle.Bold);
            int x = 0;
            int y = 0;

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                var printStr = new StringBuilder();
                printStr.AppendLine($"TIME: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                printStr.AppendLine("Order: This is Fake Order Number");
                printStr.AppendLine("Product: I am Fake Product Code");

                SizeF measureStr = graphics.MeasureString(printStr.ToString(), font);
                graphics.FillRectangle(Brushes.Black, new Rectangle(x, y, (int)measureStr.Width, (int)measureStr.Height));
                graphics.DrawString(printStr.ToString(), font, Brushes.White, new PointF(x, y));
            }
        }
    }
}
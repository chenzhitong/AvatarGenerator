using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AvatarGenerator
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer t = new Timer(180000);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Generate(object sender, ElapsedEventArgs e)
        {
            var today = DateTime.Now;
            var img = new BitmapImage(new Uri("template.png", UriKind.Relative));
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(img, new Rect(0, 0, img.Width, img.Height));
                
                drawingContext.DrawText(
                    new FormattedText($"今天是 {today.Year} 年 {today.Month} 月 {today.Day} 日      老婆也很爱我呢",
                    new CultureInfo("zh-cn"),
                    FlowDirection.LeftToRight, new Typeface("微软雅黑"), 18, new SolidColorBrush(Colors.Black)),
                    new Point(170, 30));
                drawingContext.DrawText(
                    new FormattedText(GetWeather(),
                    new CultureInfo("zh-cn"),
                    FlowDirection.LeftToRight, new Typeface("微软雅黑"), 18, new SolidColorBrush(Colors.Black)),
                    new Point(190, 70));
                drawingContext.DrawText(
                    new FormattedText($"{DateTime.Now.ToShortTimeString()}",
                    new CultureInfo("zh-cn"),
                    FlowDirection.LeftToRight, new Typeface("新宋体"), 10, new SolidColorBrush(Colors.Black)),
                    new Point(650, 670));
            }
            RenderTargetBitmap bmp = new RenderTargetBitmap(img.PixelHeight, img.PixelHeight, 144, 144, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);
            bmp.Freeze();

            canvasImage.Dispatcher.Invoke(new Action(() => {
                canvasImage.Source = bmp;
            }));


            BitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(bmp));
            using (Stream stm = File.Create("today.png"))
            {
                png.Save(stm);
            }
        }

        private string GetWeather()
        {
            WebClient wc = new WebClient();
            var raw = wc.DownloadData("http://t.weather.sojson.com/api/weather/city/101020100");
            var json = Encoding.UTF8.GetString(raw);
            JObject obj = JObject.Parse(json);
            var forecast = obj["data"]["forecast"];
            var result = "";
            foreach (var f in forecast)
            {
                if (f["date"].ToString() == DateTime.Now.Date.Day.ToString())
                {
                    result = $"{f["week"]}    {f["type"]}    {f["notice"]}";
                    break;
                }
            }
            return result;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            t.Elapsed += Generate;
            t.Start();
            Generate(sender, null);
        }
    }
}

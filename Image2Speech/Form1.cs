using Google.Cloud.Translation.V2;
using IronOcr;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;


namespace Image2Speech
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        System.Drawing.Point cursorPositionStart = new System.Drawing.Point();
        System.Drawing.Point cursorPositionStop = new System.Drawing.Point();

        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private readonly NotifyIcon notifyIcon = new NotifyIcon();



        int startx = 0;
        int starty = 0;

        int stopx = 0;
        int stopy = 0;

        int width = 0;
        int height = 0;

        bool hasLeftMouseButtonBeenPressed = false;
        bool isPointsRegistered = false;

        string modifyerKey = string.Empty;

        private readonly System.Timers.Timer timer = new System.Timers.Timer();
        private readonly System.Timers.Timer timer2 = new System.Timers.Timer();

        public Form1()
        {
            timer.Interval = 10;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            timer2.Interval = 100;
            timer2.Elapsed += Timer2_Elapsed;
            timer2.Start();

            InitializeComponent();
            InitializeNotifyIcon("notreading.ico");


            this.Opacity = 0;
            this.ShowInTaskbar = false;


        }

        private void InitializeNotifyIcon(string icon)
        {
            notifyIcon.Icon = new Icon(icon);
            notifyIcon.Visible = true;
        }

        int ctrlpressed = 0;

        private void Timer2_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                ctrlpressed++;
            }

            if (ctrlpressed == 4)
            {
                synthesizer.SpeakAsyncCancelAll();
                InitializeNotifyIcon("notreading.ico");

                ctrlpressed = 0;
            }
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            modifyerKey = ModifierKeys.ToString();

            if (modifyerKey == "Control" && Control.MouseButtons == MouseButtons.Left && hasLeftMouseButtonBeenPressed == false)
            {
                GetCursorPos(ref cursorPositionStart);

                startx = cursorPositionStart.X;
                starty = cursorPositionStart.Y;
                hasLeftMouseButtonBeenPressed = true;
            }

            if (modifyerKey == "None" && Control.MouseButtons == MouseButtons.None && hasLeftMouseButtonBeenPressed == true)
            {
                GetCursorPos(ref cursorPositionStop);

                stopx = cursorPositionStop.X;
                stopy = cursorPositionStop.Y;
                hasLeftMouseButtonBeenPressed = false;

            }

            width = stopx - startx;
            height = stopy - starty;


            if (startx != 0 && starty != 0 && stopx != 0 && stopy != 0)
            {
                if (width != 0 && height != 0)
                {
                    isPointsRegistered = true;
                }
                else
                {
                    isPointsRegistered = false;
                }
            }

            WriteToDebugConsole();

            width = Math.Abs(width);
            height = Math.Abs(height);


            if (isPointsRegistered)
            {
                timer.Stop();

                CaptureScreenAndSaveToFileImage();

                isPointsRegistered = false;
                startx = 0;
                starty = 0;
                stopx = 0;
                stopy = 0;
                width = 0;
                height = 0;
                hasLeftMouseButtonBeenPressed = false;

                string text = ConvertImageToText();
                Debug.WriteLine(text);
                SpeakText(text);
                timer.Start();
            }

        }

        private void CaptureScreenAndSaveToFileImage()
        {
            try
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(startx, starty, width, height);
                Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
                string fileName = "output.jpg";
                bmp.Save(fileName, ImageFormat.Jpeg);
                Debug.WriteLine($"Screenshot saved as {fileName}");
            }
            catch (Exception)
            {
                Debug.WriteLine("Could not catch screen image");
            }

        }

        private string ConvertImageToText()
        {
            try
            {
                License.LicenseKey = "IRONSUITE.INFO.DANIELCARLSON.NET.11806-7EE99788EA-ALGOP6N6A5PH7LKM-WSRAZUKSAYYA-TF7L5MMBKSDS-7LV6MKCRYMEH-AAQTOWSCJQ2U-CBTJGAPK5GYA-H2TIW5-TKYM7YO2N42LUA-DEPLOYMENT.TRIAL-A423OP.TRIAL.EXPIRES.09.MAR.2024";
                IronTesseract IronOcr = new IronTesseract();
                var Result = IronOcr.Read("output.jpg");
                return Result.Text;
            }
            catch (Exception)
            {
                Debug.WriteLine("Could not get text from image");
            }
            return "";
        }

        private void SpeakText(string text)
        {
            //var text = TranslateAsync(Result);
            InitializeNotifyIcon("reading.ico");
            if (synthesizer.State != SynthesizerState.Speaking)
            {
                synthesizer.SpeakAsync(text);
            }
        }

        private void WriteToDebugConsole()
        {
            var cursor = new System.Drawing.Point();
            GetCursorPos(ref cursor);

            Debug.WriteLine($"Mouse X:{cursor.X} Y:{cursor.Y}");
            Debug.WriteLine($"Key: {modifyerKey}");
            Debug.WriteLine($"Mouse button: {MouseButtons}");
            Debug.WriteLine($"startx: {startx}");
            Debug.WriteLine($"starty: {starty}");
            Debug.WriteLine($"stopx: {stopx}");
            Debug.WriteLine($"stopy: {stopy}");
            Debug.WriteLine($"ctrlpressed: {ctrlpressed}");
        }

        public class Root
        {
            public List<object> MyArray { get; set; } = new List<object>();
        }



        public string TranslateAsync(string text)
        {
            TranslationClient client = TranslationClient.Create();

            var translationResult = client.TranslateText(text, "sv");
            string swedishTranslation = translationResult.TranslatedText;

            Debug.WriteLine($"Translated text (Swedish): {swedishTranslation}");



            return swedishTranslation;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

using CoordinateSystemApp;

namespace LandmarkMarker
{
    public partial class MainPage : ContentPage
    {
        private readonly CoordinateSystemDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();

            _drawable = new CoordinateSystemDrawable();
            _drawable.stepFactor = 10;
            _drawable.ScaleFactor = 2f;

            BindingContext = new
            {
                GridDrawable = _drawable
            };
        }

        // معالجة حدث الضغط على الشاشة
        private void OnTapped(object sender, TappedEventArgs e)
        {
            if (e.GetPosition(graphicsView) is not Point tapPosition)
                return;

            // الحصول على إحداثيات الضغط
            var touchX = tapPosition.X;
            var touchY = tapPosition.Y;

            // تحويل الإحداثيات إلى نظام الرسم (نظام المعلم)
            float centerX = (float)graphicsView.Width / 2;
            float centerY = (float)graphicsView.Height / 2;
            float step = _drawable.stepFactor * _drawable.ScaleFactor; // المسافة بين الخطوط

            // حساب الإحداثيات بالنسبة للمعلم
            float x = (float)(touchX - centerX) / step;
            float y = (float)(centerY - touchY) / step;

            // إضافة النقطة إلى الرسم
            _drawable.AddPoint((float)Math.Round(x, 2), (float)Math.Round(y, 2));

            // تحديث الرسم
            graphicsView.Invalidate();
            lblArea.Text = _drawable.CalculatePolygonArea().ToString();
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            _drawable._points.Clear();
            graphicsView.Invalidate();
            PointX.Text = string.Empty;
            PointY.Text = string.Empty;
            lblArea.Text = "0";
        }

        private void OnDrawClicked(object sender, EventArgs e)
        {
            if (float.TryParse(PointX.Text, out float x) && float.TryParse(PointY.Text, out float y))
            {
                if (_drawable._points.Contains(new PointF(x, y)))
                    return;
                _drawable.AddPoint(x, y);
                graphicsView.Invalidate();
                lblArea.Text =_drawable.CalculatePolygonArea().ToString("0.0000");
                PointY.Text = string.Empty;
                PointX.Text = string.Empty;
                PointX.Focus();
            }            
        }

        private void btnUndo_Clicked(object sender, EventArgs e)
        {
            if (_drawable._points.Count() == 0)
                return;
            _drawable._points.RemoveAt(_drawable._points.Count()-1);
            graphicsView.Invalidate();
            lblArea.Text = _drawable.CalculatePolygonArea().ToString();

        }
    }
}

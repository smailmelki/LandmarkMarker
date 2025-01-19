using CoordinateSystemApp;

namespace LandmarkMarker
{
    public partial class MainPage : ContentPage
    {
        // كائن للرسم يمثل نظام الإحداثيات
        private readonly CoordinateSystemDrawable _drawable;

        public MainPage()
        {
            InitializeComponent();

            // إنشاء كائن الرسم وتحديد عوامل التكبير والتصغير
            _drawable = new CoordinateSystemDrawable();
            _drawable.stepFactor = 10; // المسافة بين خطوط الشبكة
            _drawable.ScaleFactor = 2f; // مقياس الرسم الابتدائي

            // ربط كائن الرسم بواجهة المستخدم
            BindingContext = new
            {
                GridDrawable = _drawable
            };
        }

        // معالجة حدث النقر على الشاشة
        private void OnTapped(object sender, TappedEventArgs e)
        {
            if (e.GetPosition(graphicsView) is not Point tapPosition)
                return;

            // الحصول على إحداثيات النقطة التي تم النقر عليها
            var touchX = tapPosition.X;
            var touchY = tapPosition.Y;

            // حساب الإحداثيات بالنسبة لنظام الرسم (المعلم)
            float centerX = (float)graphicsView.Width / 2;
            float centerY = (float)graphicsView.Height / 2;
            float step = _drawable.stepFactor * _drawable.ScaleFactor;

            float x = (float)(touchX - centerX) / step; // تحويل الإحداثي الأفقي
            float y = (float)(centerY - touchY) / step; // تحويل الإحداثي العمودي

            // إضافة النقطة إلى الرسم
            _drawable.AddPoint((float)Math.Round(x, 2), (float)Math.Round(y, 2));

            // تحديث واجهة الرسم
            graphicsView.Invalidate();
            lblArea.Text = _drawable.CalculatePolygonArea().ToString();
        }

        // زر مسح جميع النقاط
        private void OnClearClicked(object sender, EventArgs e)
        {
            _drawable._points.Clear(); // مسح قائمة النقاط
            graphicsView.Invalidate(); // إعادة تحديث واجهة الرسم
            PointX.Text = string.Empty; // إفراغ حقل إدخال X
            PointY.Text = string.Empty; // إفراغ حقل إدخال Y
            lblArea.Text = "0"; // إعادة ضبط قيمة المساحة
        }

        // زر إضافة نقطة
        private void OnDrawClicked(object sender, EventArgs e)
        {
            if (float.TryParse(PointX.Text, out float x) && float.TryParse(PointY.Text, out float y))
            {
                // منع إضافة النقطة إذا كانت مكررة
                if (_drawable._points.Contains(new PointF(x, y)))
                    return;

                // التأكد من أن النقطة تناسب حدود الرسم
                EnsurePointFits(x, y);

                // إضافة النقطة إلى قائمة النقاط
                _drawable.AddPoint(x, y);

                // تحديث واجهة الرسم
                graphicsView.Invalidate();
                lblArea.Text = _drawable.CalculatePolygonArea().ToString("0.0000");

                // إعادة ضبط الحقول
                PointY.Text = string.Empty;
                PointX.Text = string.Empty;
                PointX.Focus();
            }
        }

        // زر التراجع عن آخر نقطة مضافة
        private void btnUndo_Clicked(object sender, EventArgs e)
        {
            if (_drawable._points.Count() == 0)
                return; // لا توجد نقاط للتراجع عنها

            _drawable._points.RemoveAt(_drawable._points.Count() - 1); // إزالة آخر نقطة
            graphicsView.Invalidate(); // تحديث واجهة الرسم
            lblArea.Text = _drawable.CalculatePolygonArea().ToString(); // تحديث قيمة المساحة
        }

        // الانتقال إلى إدخال Y بعد الانتهاء من إدخال X
        private void PointX_Completed(object sender, EventArgs e)
        {
            PointY.Focus();
        }

        // التأكد من أن النقطة تناسب منطقة الرسم وتعديل المقياس إذا لزم الأمر
        private void EnsurePointFits(float x, float y)
        {
            float centerX = (float)graphicsView.Width / 2;
            float centerY = (float)graphicsView.Height / 2;
            float step = _drawable.stepFactor * _drawable.ScaleFactor;

            // تحويل النقطة إلى إحداثيات الرسم
            float drawX = centerX + x * step;
            float drawY = centerY - y * step;

            // تحقق إذا كانت النقطة خارج حدود العرض
            if (drawX < 0 || drawX > graphicsView.Width || drawY < 0 || drawY > graphicsView.Height)
            {
                // حساب مقياس الرسم الجديد لاستيعاب النقطة
                float newScaleFactorX = (float)graphicsView.Width / (Math.Abs(x) * 2 * _drawable.stepFactor);
                float newScaleFactorY = (float)graphicsView.Height / (Math.Abs(y) * 2 * _drawable.stepFactor);

                // اختيار المقياس الأصغر للحفاظ على جميع النقاط داخل الرسم
                _drawable.ScaleFactor = Math.Min(newScaleFactorX, newScaleFactorY);

                // تحديث واجهة الرسم
                graphicsView.Invalidate();
            }
        }
    }
}

﻿using CoordinateSystemApp;
#if ANDROID
using Android.Widget;
#endif

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
            _drawable.stepFactor = 10f; // المسافة بين خطوط الشبكة
            _drawable.ScaleFactor = 2f; // مقياس الرسم الابتدائي

            // ربط كائن الرسم بواجهة المستخدم
            BindingContext = new
            {
                GridDrawable = _drawable
            };
        }

        /// <summary>
        /// معالجة حدث النقر على الشاشة
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>               
        private void OnTapped(object sender, TappedEventArgs e)
        {
            if (e.GetPosition(graphicsView) is not Point tapPosition)
                return;

            // الحصول على إحداثيات النقطة التي تم النقر عليها
            var touchX = tapPosition.X;
            var touchY = tapPosition.Y;
            /////////////////////////////////////
            float width = (float)graphicsView.Width;
            float height = (float)graphicsView.Height;
            float step = _drawable.stepFactor * _drawable.ScaleFactor; // المسافة بين الخطوط
            int a = (int)Math.Round(width / step, 0);
            int b = (int)Math.Round(height / step, 0);
            // حساب الإحداثيات بالنسبة لنظام الرسم (المعلم)
            float centerX = (float)Math.Round(a / 2 * step, 0);
            float centerY = (float)Math.Round(b / 2 * step, 0);
            /////////////////////////////////////
            float x = (float)Math.Round((touchX - centerX) / step, 2); // تحويل الإحداثي الأفقي
            float y = (float)Math.Round((centerY - touchY) / step, 2); // تحويل الإحداثي العمودي
            PointWithCoordinates point = new PointWithCoordinates(x, y, CoordinateType.Cartesian);
            // منع إضافة النقطة إذا كانت مكررة
            if (_drawable._points.Contains(point))
                return;

            // إضافة النقطة إلى الرسم
            _drawable.AddPoint(point);

            // تحديث واجهة الرسم
            graphicsView.Invalidate();
            lblArea.Text = _drawable.CalculatePolygonArea().ToString();
        }

        // زر مسح جميع النقاط
        private void OnClearClicked(object sender, EventArgs e)
        {
            _drawable._points.Clear(); // مسح قائمة النقاط
            _drawable.ScaleFactor = 2f; // مقياس الرسم الابتدائي
            graphicsView.Invalidate(); // إعادة تحديث واجهة الرسم
            PointX.Text = string.Empty; // إفراغ حقل إدخال X
            PointY.Text = string.Empty; // إفراغ حقل إدخال Y
            lblArea.Text = "0"; // إعادة ضبط قيمة المساحة
        }

        // زر إضافة نقطة
        private void OnDrawClicked(object sender, EventArgs e)
        {
            if (swPolar.IsToggled)
            {
                PolarDraw();
            }
            else
            {
                DrawPoint();                
            }
        }

        void PolarDraw()
        {
            if (float.TryParse(PointX.Text, out float radius) && float.TryParse(PointY.Text, out float theta))
            {
                PointWithCoordinates point = new PointWithCoordinates(radius, theta, CoordinateType.Polar);
                // منع إضافة النقطة إذا كانت مكررة
                if (_drawable._points.Contains(point))
                return;

                // التأكد من أن النقطة تناسب حدود الرسم
                EnsurePointFits(point.CartesianPoint.X, point.CartesianPoint.Y);

                // إضافة النقطة إلى قائمة النقاط
                _drawable.AddPoint(point);

                // تحديث واجهة الرسم
                graphicsView.Invalidate();
                lblArea.Text = _drawable.CalculatePolygonArea().ToString("0.0000");

                // إعادة ضبط الحقول
                // إعادة ضبط الحقول
                PointX.Text = string.Empty;
                PointY.Text = string.Empty;
                PointX.Focus();
            }
        }

        void DrawPoint()
        {
            if (float.TryParse(PointX.Text, out float x) && float.TryParse(PointY.Text, out float y))
            {
                // منع إضافة النقطة إذا كانت مكررة
                PointWithCoordinates point = new PointWithCoordinates(x, y, CoordinateType.Cartesian);
                if (_drawable._points.Contains(point))
                return;

                // التأكد من أن النقطة تناسب حدود الرسم
                EnsurePointFits(point.CartesianPoint.X, point.CartesianPoint.Y);

                // إضافة النقطة إلى قائمة النقاط
                _drawable.AddPoint(point);

                // تحديث واجهة الرسم
                graphicsView.Invalidate();
                lblArea.Text = _drawable.CalculatePolygonArea().ToString("0.0000");

                // إعادة ضبط الحقول
                PointX.Text = string.Empty;
                PointY.Text = string.Empty;
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

        private void btnDrawGrid_Clicked(object sender, EventArgs e)
        {
            if (_drawable.drawGrid)
                _drawable.drawGrid = false;
            else
                _drawable.drawGrid = true;
            graphicsView.Invalidate(); // تحديث واجهة الرسم
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

        private void OnApoutClicked(object sender, EventArgs e)
        {
#if ANDROID
            Toast.MakeText(Android.App.Application.Context,
                           "هذا البرنامج تم تصميمه وبرمجته \n من قبل ملكي اسماعيل",
                           ToastLength.Short).Show();
#else
            DisplayAlert("معلومات المطور", "هذا البرنامج تم تصميمه وبرمجته \n من قبل ملكي اسماعيل", "OK",FlowDirection.RightToLeft);
#endif
        }

        private void swPolar_Toggled(object sender, ToggledEventArgs e)
        {
            if (swPolar.IsToggled)
            {
                txtX.Text = "R = ";
                txtY.Text = "θ = ";
            }
            else 
            {
                txtX.Text = "X = ";
                txtY.Text = "Y = ";
            }
        }
    }
}

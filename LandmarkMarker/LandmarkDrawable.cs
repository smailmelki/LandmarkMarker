namespace CoordinateSystemApp;

public class CoordinateSystemDrawable : IDrawable
{
    // قائمة النقاط التي سيتم تعليمها
    public readonly List<PointF> _points = new List<PointF>();

    // عامل التكبير أو التصغير
    public float ScaleFactor { get; set; } = 1;

    public void AddPoint(float x, float y)
    {
        _points.Add(new PointF(x, y));
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        float step = 20 * ScaleFactor; // المسافة بين الخطوط مع تطبيق التكبير أو التصغير

        // إعداد الألوان وخطوط الرسم
        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;

        // رسم الشبكة (Grid)
        canvas.StrokeColor = Colors.LightGray;

        // خطوط الشبكة العمودية
        float a = 0;
        for (float x = 0; x <= width; x += step)
        {
            canvas.DrawLine(x, 0, x, height);
            a += 1;
        }

        // خطوط الشبكة الأفقية
        float b = 0;
        for (float y = 0; y <= height; y += step)
        {
            canvas.DrawLine(0, y, width, y);
            b += 1;
        }

        // رسم المحاور
        canvas.StrokeColor = Colors.Orange;
        canvas.StrokeSize = 2;

        float centerX = (float)Math.Round(a / 2, 0) * step;
        float centerY = (float)Math.Round(b / 2, 0) * step;

        // المحور X
        canvas.DrawLine(0, centerY, width, centerY);

        // المحور Y
        canvas.DrawLine(centerX, 0, centerX, height);

        // إضافة الأسهم على المحاور
        DrawArrow(canvas, centerX, 200, centerX, 0); // أعلى المحور Y
        DrawArrow(canvas, width - 200, centerY, width, centerY); // يمين المحور X

        // تسمية المحاور
        canvas.FontColor = Colors.Orange;
        canvas.FontSize = 12;

        // الحرف X
        canvas.DrawString("X", width - 20, centerY - 20, HorizontalAlignment.Center);
        // الحرف Y
        canvas.DrawString("Y", centerX + 10, 10, HorizontalAlignment.Center);

        // رسم النقاط على المعلم
        DrawPoints(canvas, centerX, centerY, step);

        // ملء المنطقة بالنقاط
        FillPolygonArea(canvas, centerX, centerY, step);

        // حساب المساحة وعرضها
        float area = CalculatePolygonArea();
        canvas.FontColor = Colors.DarkBlue;
        canvas.FontSize = 14;
        canvas.DrawString($"Area: {area:F2}", 10, 10, HorizontalAlignment.Left);
    }

    private void DrawPoints(ICanvas canvas, float centerX, float centerY, float step)
    {
        canvas.FillColor = Colors.Red;

        foreach (var point in _points)
        {
            // تحويل الإحداثيات إلى نظام الرسم مع تطبيق التكبير أو التصغير
            float drawX = centerX + point.X * step;
            float drawY = centerY - point.Y * step;

            // رسم النقطة
            canvas.FillCircle(drawX, drawY, 5);

            // كتابة الإحداثيات
            canvas.FontColor = Colors.Black;
            canvas.FontSize = 10;
            canvas.DrawString($"({point.X}, {point.Y})", drawX + 5, drawY - 10, HorizontalAlignment.Left);
        }
    }

    private void FillPolygonArea(ICanvas canvas, float centerX, float centerY, float step)
    {
        if (_points.Count < 3)
            return; // لا يمكن ملء مساحة أقل من ثلاث نقاط

        var path = new PathF();

        // تحويل النقاط إلى إحداثيات الرسم
        for (int i = 0; i < _points.Count; i++)
        {
            float drawX = centerX + _points[i].X * step;
            float drawY = centerY - _points[i].Y * step;

            if (i == 0)
                path.MoveTo(drawX, drawY); // النقطة الأولى
            else
                path.LineTo(drawX, drawY); // النقاط التالية
        }

        path.Close(); // إغلاق المضلع

        // تعبئة المضلع باللون الأخضر
        canvas.FillColor = Colors.Green;
        canvas.FillPath(path);
    }

    private void DrawArrow(ICanvas canvas, float x1, float y1, float x2, float y2)
    {
        canvas.DrawLine(x1, y1, x2, y2);

        float arrowSize = 10;
        float angle = (float)Math.Atan2(y2 - y1, x2 - x1);

        float x3 = x2 - arrowSize * (float)Math.Cos(angle - Math.PI / 6);
        float y3 = y2 - arrowSize * (float)Math.Sin(angle - Math.PI / 6);

        float x4 = x2 - arrowSize * (float)Math.Cos(angle + Math.PI / 6);
        float y4 = y2 - arrowSize * (float)Math.Sin(angle + Math.PI / 6);

        canvas.DrawLine(x2, y2, x3, y3);
        canvas.DrawLine(x2, y2, x4, y4);
    }

    private float CalculatePolygonArea()
    {
        if (_points.Count < 3)
            return 0; // أقل من ثلاث نقاط لا تشكل مضلعًا

        float area = 0;

        for (int i = 0; i < _points.Count; i++)
        {
            int j = (i + 1) % _points.Count; // النقطة التالية (مع الإغلاق)

            area += _points[i].X * _points[j].Y;
            area -= _points[j].X * _points[i].Y;
        }

        area = Math.Abs(area) / 2.0f;
        return area;
    }
}

using LandmarkMarker;

namespace CoordinateSystemApp;

public class CoordinateSystemDrawable : IDrawable
{
    // قائمة النقاط التي سيتم تعليمها
    public readonly List<PointWithCoordinates> _points = new List<PointWithCoordinates>();

    // عامل التكبير أو التصغير
    public float ScaleFactor { get; set; } = 1;
    public float stepFactor { get; set; } = 10;
    public bool drawGrid = true;

    public void AddPoint(PointWithCoordinates point)
    {
        _points.Add(point);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        float step = stepFactor * ScaleFactor; // المسافة بين الخطوط
        int a = (int)Math.Round(width / step, 0);
        int b = (int)Math.Round(height / step, 0);
        float centerX = (float)Math.Round(a / 2 * step, 0);
        float centerY = (float)Math.Round(b / 2 * step, 0);

        // رسم الشبكة مرة واحدة
        if (drawGrid)
            DrawGrid(canvas, width, height, step);

        // رسم المحاور
        DrawAxes(canvas, width, height, step,centerX,centerY);

        // رسم النقاط
        foreach (var point in _points)
        {
            DrawSinglePoint(canvas, point, centerX, centerY, step);
        }

        // ملء المنطقة بالنقاط (إن وجدت)
        FillPolygonArea(canvas, centerX, centerY, step);
    }

    private void DrawGrid(ICanvas canvas, float width, float height, float step)
    {
        canvas.StrokeColor = Colors.LightGray;
        canvas.StrokeSize = 1;

        // خطوط الشبكة العمودية
        for (float x = 0; x <= width; x += step)
        {
            canvas.DrawLine(x, 0, x, height);
        }

        // خطوط الشبكة الأفقية
        for (float y = 0; y <= height; y += step)
        {
            canvas.DrawLine(0, y, width, y);
        }
    }

    private void DrawAxes(ICanvas canvas, float width, float height, float step, float centerX, float centerY)
    {
        canvas.StrokeColor = Colors.Orange;
        canvas.StrokeSize = 2;

        // المحور X
        canvas.DrawLine(0, centerY, width, centerY);

        // المحور Y
        canvas.DrawLine(centerX, 0, centerX, height);

        // الأسهم
        DrawArrow(canvas, centerX, 200, centerX, 0); // أعلى المحور Y
        DrawArrow(canvas, width - 200, centerY, width, centerY); // يمين المحور X
    }

    public void DrawSinglePoint(ICanvas canvas, PointWithCoordinates point, float centerX, float centerY, float step)
    {
        // تحويل النقطة إلى إحداثيات الشاشة
        float drawX = centerX + point.CartesianPoint.X * step;
        float drawY = centerY - point.CartesianPoint.Y * step;

        // رسم النقطة
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(drawX, drawY, 5);

        // كتابة الإحداثيات
        string pointInfo;
        if (point.Type == CoordinateType.Polar)
            pointInfo = $"(r={point.Radius:F2}, θ={point.Angle:F2}°)";
        else
            pointInfo = $"(x={point.CartesianPoint.X:F2}, y={point.CartesianPoint.Y:F2})";

        canvas.FontColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
        canvas.FontSize = 10;
        canvas.DrawString(pointInfo, drawX + 5, drawY - 10, HorizontalAlignment.Left);
    }

    private void FillPolygonArea(ICanvas canvas, float centerX, float centerY, float step)
    {
        if (_points.Count < 3)
            return; // لا يمكن ملء مساحة أقل من ثلاث نقاط

        var path = new PathF();

        // تحويل النقاط إلى إحداثيات الرسم
        for (int i = 0; i < _points.Count; i++)
        {
            float drawX = centerX + _points[i].CartesianPoint.X * step;
            float drawY = centerY - _points[i].CartesianPoint.Y * step;

            if (i == 0)
                path.MoveTo(drawX, drawY); // النقطة الأولى
            else
                path.LineTo(drawX, drawY); // النقاط التالية
        }

        path.Close(); // إغلاق المضلع

        // تعبئة المضلع باللون الأخضر
        canvas.FillColor = Color.FromHsla(120 / 360.0, 1.0, 0.5, 0.5);

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

    public float CalculatePolygonArea()
    {
        if (_points.Count < 3)
            return 0; // أقل من ثلاث نقاط لا تشكل مضلعًا

        float area = 0;

        for (int i = 0; i < _points.Count; i++)
        {
            int j = (i + 1) % _points.Count; // النقطة التالية (مع الإغلاق)

            area += _points[i].CartesianPoint.X * _points[j].CartesianPoint.Y;
            area -= _points[j].CartesianPoint.X * _points[i].CartesianPoint.Y;
        }

        area = Math.Abs(area) / 2.0f;
        return area;
    }
}

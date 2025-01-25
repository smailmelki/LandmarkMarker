namespace LandmarkMarker
{
    public class PointWithCoordinates
    {
        public PointF CartesianPoint { get; set; }
        public CoordinateType Type { get; set; }
        public float Radius { get; set; }
        public float Angle { get; set; } // الزاوية بالغراد

        public PointWithCoordinates(float x, float y, CoordinateType type = CoordinateType.Cartesian)
        {
            Type = type;

            if (type == CoordinateType.Polar)
            {
                Radius = x;
                Angle = y;

                CartesianPoint = new PointF(
                    x: (float)(x * Math.Cos(-(y - 100) * Math.PI / 200)), // تحويل إلى ديكارتية
                    y: (float)(x * Math.Sin(-(y - 100) * Math.PI / 200))
                );
            }
            else
            {
                Radius = (float)Math.Sqrt(x * x + y * y);
                Angle = (float)(Math.Atan2(y, x) * 180 / Math.PI); // تحويل إلى قطبية
                CartesianPoint = new PointF(x, y);
            }
        }
    }
    public enum CoordinateType
    {
        Cartesian,
        Polar
    }
}

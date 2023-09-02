using OpenCvSharp;

namespace YoLoTool.Drawers
{
    public static class CameraDrawer
    {
        private static readonly Scalar PointScalar = Scalar.Black;

        private static readonly Scalar PointSecondScalar = Scalar.White;

        private static readonly Scalar RectangleScalar = Scalar.Yellow;

        private static readonly Scalar LineScalar = Scalar.Gray;

        private static readonly Scalar RectangleFillScalar = Scalar.Red;

        public static void DrawPoint(this Mat mat, Point2d point)
        {
            Cv2.Circle(mat, new Point(point.X, point.Y), 5, PointSecondScalar, -1);
            Cv2.Circle(mat, new Point(point.X, point.Y), 4, PointScalar, -1);
        }

        public static void DrawRect(this Mat mat, Rect rect)
        {
            Cv2.Rectangle(mat, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), RectangleScalar, 3);
        }

        public static void DrawRectInside(this Mat mat, Rect rect)
        {
            Cv2.Rectangle(mat, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), RectangleFillScalar, 6);
        }

        public static void DrawLines(this Mat mat, Point2d point)
        {
            Cv2.Line(mat, new Point(0, point.Y), new Point(mat.Width, point.Y), LineScalar, 1);
            Cv2.Line(mat, new Point(point.X, 0), new Point(point.X, mat.Height), LineScalar, 1);
        }
    }
}

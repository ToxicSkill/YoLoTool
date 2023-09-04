using OpenCvSharp;
using YoLoTool.Enums;

namespace YoLoTool.Drawers
{
    public static class CameraDrawer
    {
        private const int RectThickness = 2;

        private const int SelectedRectThickness = 3;

        private const int PointRadius = 2;

        private static readonly Scalar PointScalar = Scalar.Black;

        private static readonly Scalar PointSecondScalar = Scalar.White;

        private static readonly Scalar RectangleScalar = Scalar.Yellow;

        private static readonly Scalar LineScalar = Scalar.Gray;

        private static readonly Scalar RectangleFillScalar = Scalar.Red; 

        private static readonly Scalar RectangleFillPressedScalar = Scalar.Blue;

        public static void DrawPoint(this Mat mat, Point2d point)
        {
            Cv2.Circle(mat, new Point(point.X, point.Y), PointRadius + 1, PointSecondScalar, -1);
            Cv2.Circle(mat, new Point(point.X, point.Y), PointRadius, PointScalar, -1);
        }

        public static void DrawRect(this Mat mat, Rect rect)
        {
            Cv2.Rectangle(mat, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), RectangleScalar, RectThickness);
        }

        public static void DrawResizingLine(this Mat mat, Rect rect, EPointInRectResult result)
        {
            switch (result)
            {
                case EPointInRectResult.Top:
                    Cv2.Line(mat, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y), RectangleFillPressedScalar, SelectedRectThickness);
                    break;
                case EPointInRectResult.Bottom:
                    Cv2.Line(mat, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height), RectangleFillPressedScalar, SelectedRectThickness);
                    break;
                case EPointInRectResult.Left:
                    Cv2.Line(mat, new Point(rect.X, rect.Y), new Point(rect.X , rect.Y + rect.Height), RectangleFillPressedScalar, SelectedRectThickness);
                    break;
                case EPointInRectResult.Right:
                    Cv2.Line(mat, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height), RectangleFillPressedScalar, SelectedRectThickness);
                    break;
            }
        }

        public static void DrawSelectedRect(this Mat mat, Rect rect)
        {
            Cv2.Rectangle(mat, new Point(rect.X, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height),RectangleFillScalar, SelectedRectThickness);
        }

        public static void DrawLines(this Mat mat, Point2d point)
        {
            Cv2.Line(mat, new Point(0, point.Y), new Point(mat.Width, point.Y), LineScalar, 1);
            Cv2.Line(mat, new Point(point.X, 0), new Point(point.X, mat.Height), LineScalar, 1);
        }
    }
}

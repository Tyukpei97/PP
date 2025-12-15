using System.Windows.Forms;

namespace ShapeEditor.WinForms.UI
{
    public sealed class CanvasPanel : Panel
    {
        public CanvasPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}

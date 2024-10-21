using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Client
{
    public class DraggableAdorner : Adorner
    {
        Rect renderRect;
        Brush renderBrush;
        public Point CenterOffset;

        public DraggableAdorner(UIElement adornedElement) : base(adornedElement)
        {
            renderRect = new Rect(adornedElement.RenderSize);
            this.IsHitTestVisible = false;
            renderBrush = (adornedElement as Border).Background.Clone();
            CenterOffset = new Point(-renderRect.Width / 2, -renderRect.Height / 2);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(renderBrush, null, renderRect);
        }

    }
}

using PointsChecker.Utils;
using System;
using System.Windows.Forms;

namespace PointsChecker.Controls
{
    public class RoundedTextBox : TextBox
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            this.Region = GraphicsUtils.GetRoundedRegion(0, 0, this.Width, this.Height);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            this.Region = GraphicsUtils.GetRoundedRegion(0, 0, this.Width, this.Height);
        }
    }
}

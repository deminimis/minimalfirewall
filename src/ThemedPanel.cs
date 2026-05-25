using System;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class ThemedPanel : Panel
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Theme.Colors == null) return;

            // Apply surface back color unconditionally
            if (BackColor != Color.Transparent)
            {
                BackColor = Theme.Colors.Surface;
            }
            BorderStyle = BorderStyle.FixedSingle;
            ForeColor = Theme.Colors.TextActive;
        }
    }
}

using System;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class ThemedLabel : Label
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Theme.Colors == null) return;
            this.BackColor = Color.Transparent;
            this.ForeColor = Theme.Colors.TextActive;
            this.BorderStyle = BorderStyle.None;
        }
    }
}

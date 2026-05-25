using System;
using System.Drawing;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class ThemedButton : Button
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Theme.Colors == null) return;

            // Check the active palette rather than the system setting
            bool isDarkMode = Theme.Colors.Surface.R < 128;

            FlatStyle = isDarkMode ?
                FlatStyle.Flat : FlatStyle.Standard;
            FlatAppearance.CheckedBackColor = Theme.Colors.Accent;

            if (BackColor != Color.Transparent)
            {
                BackColor = Theme.Colors.Control;
            }

            ForeColor = Enabled ? Theme.Colors.TextActive : Theme.Colors.TextInactive;

            Form? parentForm = FindForm();
            FlatAppearance.BorderColor = (parentForm?.AcceptButton == this) ? Theme.Colors.Accent : Theme.Colors.ControlDark;

            FlatAppearance.MouseOverBackColor = Theme.Colors.ControlLight;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            ApplyTheme();
        }
    }
}

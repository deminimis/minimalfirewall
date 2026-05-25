using System;
using System.Windows.Forms;

namespace DarkModeForms
{
    public class ThemedDataGridView : DataGridView
    {
        public ThemedDataGridView()
        {
            this.DoubleBuffered = true;
            this.EnableHeadersVisualStyles = false;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            this.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Theme.Colors == null) return;
            this.BackgroundColor = Theme.Colors.Control;
            this.GridColor = Theme.Colors.ControlDark;
            this.DefaultCellStyle.BackColor = Theme.Colors.Surface;
            this.DefaultCellStyle.ForeColor = Theme.Colors.TextActive;
            this.ColumnHeadersDefaultCellStyle.BackColor = Theme.Colors.Surface;
            this.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Colors.TextActive;
            this.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.Colors.Surface;
            this.RowHeadersDefaultCellStyle.BackColor = Theme.Colors.Surface;
            this.RowHeadersDefaultCellStyle.ForeColor = Theme.Colors.TextActive;
            this.RowHeadersDefaultCellStyle.SelectionBackColor = Theme.Colors.Surface;
        }
    }
}

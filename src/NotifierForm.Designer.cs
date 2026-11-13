using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    partial class NotifierForm
    {
        private System.ComponentModel.IContainer components = null;
        private DarkModeForms.ThemedLabel infoLabel;
        private DarkModeForms.ThemedLabel appNameLabel;
        private System.Windows.Forms.TextBox pathLabel;
        private System.Windows.Forms.CheckBox trustPublisherCheckBox;
        private DarkModeForms.ThemedButton createWildcardButton;
        private DarkModeForms.ThemedButton copyDetailsButton;
        private DarkModeForms.ThemedButton allowButton;
        private DarkModeForms.ThemedButton tempAllowButton;
        private DarkModeForms.ThemedButton blockButton;
        private DarkModeForms.ThemedButton ignoreButton;
        private System.Windows.Forms.ContextMenuStrip tempAllowContextMenu;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private TableLayoutPanel _notificationLayout;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tempAllowContextMenu = new ContextMenuStrip(components);
            toolTip1 = new ToolTip(components);
            infoLabel = new DarkModeForms.ThemedLabel { Name = "infoLabel", AutoSize = true, Dock = DockStyle.Fill, Text = "Blocked a connection for:", Margin = new Padding(0, 0, 0, 8) };
            appNameLabel = new DarkModeForms.ThemedLabel { Name = "appNameLabel", AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
            pathLabel = new TextBox
            {
                Name = "pathLabel", Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, WordWrap = true,
                ScrollBars = ScrollBars.Vertical, BorderStyle = BorderStyle.None, MinimumSize = new Size(0, 48), Margin = new Padding(0, 0, 0, 12)
            };
            trustPublisherCheckBox = new CheckBox { Name = "trustPublisherCheckBox", AutoSize = true, Text = "Always trust publisher", Visible = false };
            allowButton = new DarkModeForms.ThemedButton { Name = "allowButton", Text = "Allow" };
            blockButton = new DarkModeForms.ThemedButton { Name = "blockButton", Text = "Block" };
            ignoreButton = new DarkModeForms.ThemedButton { Name = "ignoreButton", Text = "Ignore" };
            tempAllowButton = new DarkModeForms.ThemedButton { Name = "tempAllowButton", Text = "Allow Temporarily" };
            createWildcardButton = new DarkModeForms.ThemedButton { Name = "createWildcardButton", Text = "Create Wildcard Rule..." };
            copyDetailsButton = new DarkModeForms.ThemedButton { Name = "copyDetailsButton", Text = "Copy details" };
            allowButton.Click += AllowButton_Click;
            blockButton.Click += BlockButton_Click;
            ignoreButton.Click += IgnoreButton_Click;
            tempAllowButton.Click += TempAllowButton_Click;
            createWildcardButton.Click += CreateWildcardButton_Click;
            copyDetailsButton.Click += CopyDetailsButton_Click;

            FlowLayoutPanel Commands(params Button[] buttons)
            {
                var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = Padding.Empty };
                foreach (var button in buttons)
                {
                    button.AutoSize = true;
                    button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                    button.MinimumSize = new Size(90, 32);
                    button.Padding = new Padding(8, 4, 8, 4);
                    button.Margin = new Padding(0, 4, 8, 0);
                    flow.Controls.Add(button);
                }
                return flow;
            }
            _notificationLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(16) };
            _notificationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _notificationLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _notificationLayout.Controls.Add(infoLabel, 0, 0);
            _notificationLayout.Controls.Add(appNameLabel, 0, 1);
            _notificationLayout.Controls.Add(pathLabel, 0, 2);
            _notificationLayout.Controls.Add(trustPublisherCheckBox, 0, 3);
            _notificationLayout.Controls.Add(Commands(copyDetailsButton, createWildcardButton, tempAllowButton), 0, 4);
            _notificationLayout.Controls.Add(Commands(allowButton, blockButton, ignoreButton), 0, 5);
            _notificationLayout.Layout += (_, _) =>
            {
                if (IsHandleCreated && !_fittingLayout) BeginInvoke((Action)FitToWorkingArea);
            };
            Controls.Add(_notificationLayout);
            Font = SystemFonts.MessageBoxFont;
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(480, 320);
            MinimumSize = new Size(360, 270);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NotifierForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Connection Blocked";
        }

        #endregion

        private ToolTip toolTip1;
    }
}

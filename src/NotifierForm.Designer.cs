using System.Drawing;
using System.Windows.Forms;

namespace MinimalFirewall
{
    partial class NotifierForm
    {
        private System.ComponentModel.IContainer components = null;
        private DarkModeForms.ThemedLabel infoLabel;
        private DarkModeForms.ThemedLabel appNameLabel;
        private DarkModeForms.ThemedLabel subtitleLabel;
        private Panel detailsPanel;
        private TableLayoutPanel detailsGrid;
        private System.Windows.Forms.CheckBox trustPublisherCheckBox;
        private LinkLabel createWildcardButton;
        private LinkLabel copyDetailsButton;
        private LinkLabel searchOnlineButton;
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
            Font = SystemFonts.MessageBoxFont;
            infoLabel = new DarkModeForms.ThemedLabel { Name = "infoLabel", AutoSize = true, Dock = DockStyle.Fill, Text = "Blocked a connection for:", Margin = new Padding(0, 0, 0, 4) };
            appNameLabel = new DarkModeForms.ThemedLabel { Name = "appNameLabel", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false, Margin = Padding.Empty };
            subtitleLabel = new DarkModeForms.ThemedLabel { Name = "subtitleLabel", AutoSize = true, Dock = DockStyle.Fill, UseMnemonic = false, Margin = new Padding(0, 2, 0, 0), Visible = false };
            detailsGrid = new TableLayoutPanel { Name = "detailsGrid", ColumnCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, Margin = Padding.Empty };
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            detailsPanel = new Panel { Name = "detailsPanel", Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0, 12, 0, 8) };
            detailsPanel.Controls.Add(detailsGrid);
            trustPublisherCheckBox = new CheckBox { Name = "trustPublisherCheckBox", AutoSize = true, Text = "Always trust publisher", Visible = false, Margin = new Padding(0, 4, 0, 0) };
            allowButton = new DarkModeForms.ThemedButton { Name = "allowButton", Text = "Allow" };
            tempAllowButton = new DarkModeForms.ThemedButton { Name = "tempAllowButton", Text = "Allow temporarily " + (char)0x25BE };
            blockButton = new DarkModeForms.ThemedButton { Name = "blockButton", Text = "Block" };
            ignoreButton = new DarkModeForms.ThemedButton { Name = "ignoreButton", Text = "Ignore" };
            searchOnlineButton = new LinkLabel { Name = "searchOnlineButton", Text = "Search online", AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            copyDetailsButton = new LinkLabel { Name = "copyDetailsButton", Text = "Copy details", AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            createWildcardButton = new LinkLabel { Name = "createWildcardButton", Text = "Create wildcard rule...", AutoSize = true, Margin = Padding.Empty };
            allowButton.Click += AllowButton_Click;
            blockButton.Click += BlockButton_Click;
            ignoreButton.Click += IgnoreButton_Click;
            tempAllowButton.Click += TempAllowButton_Click;
            createWildcardButton.LinkClicked += (s, e) => CreateWildcardButton_Click(s!, e);
            copyDetailsButton.LinkClicked += (s, e) => CopyDetailsButton_Click(s!, e);
            searchOnlineButton.LinkClicked += SearchOnlineButton_Click;

            FlowLayoutPanel Flow(params Control[] controls)
            {
                var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Margin = Padding.Empty };
                flow.Controls.AddRange(controls);
                return flow;
            }
            foreach (var button in new Button[] { allowButton, tempAllowButton, blockButton, ignoreButton })
            {
                button.AutoSize = true;
                button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                button.MinimumSize = new Size(84, 32);
                button.Padding = new Padding(8, 4, 8, 4);
                button.Margin = new Padding(0, 8, 8, 0);
            }

            _notificationLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16) };
            _notificationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Control[] rows = [infoLabel, appNameLabel, subtitleLabel, detailsPanel,
                Flow(searchOnlineButton, copyDetailsButton, createWildcardButton), trustPublisherCheckBox,
                Flow(allowButton, tempAllowButton, blockButton, ignoreButton)];
            _notificationLayout.RowCount = rows.Length;
            for (int i = 0; i < rows.Length; i++)
            {
                _notificationLayout.RowStyles.Add(rows[i] == detailsPanel ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));
                // Explicit row: auto-placement skips hidden controls and would shift rows onto the wrong style.
                _notificationLayout.Controls.Add(rows[i], 0, i);
            }
            _notificationLayout.Layout += (_, _) =>
            {
                if (IsHandleCreated && !_fittingLayout) BeginInvoke((Action)FitToWorkingArea);
            };
            Controls.Add(_notificationLayout);
            OnFontChanged(EventArgs.Empty);
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(500, 400);
            MinimumSize = new Size(360, 300);
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

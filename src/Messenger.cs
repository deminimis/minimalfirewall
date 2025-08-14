// Messenger.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static DarkModeForms.KeyValue;
using Timer = System.Windows.Forms.Timer;

namespace DarkModeForms
{
    /* Author: BlueMystic (bluemystic.play@gmail.com)  2024 */
    public static class Messenger
    {
        #region Events
        private static Action<object, ValidateEventArgs>? ValidateControlsHandler;

        public static event Action<object, ValidateEventArgs>? ValidateControls
        {
            add => ValidateControlsHandler += value;
            remove => ValidateControlsHandler -= value;
        }

        private static void ResetEvents()
        {
            ValidateControlsHandler = null;
        }

        #endregion Events

        #region MessageBox

        private static MessageBoxDefaultButton _defaultButton = MessageBoxDefaultButton.Button1;
        public static DialogResult MessageBox(string Message)
            => MessageBox(Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        public static DialogResult MessageBox(Exception ex, bool ShowTrace = true) =>
            MessageBox(ex.Message + (ShowTrace ? "\r\n" + ex.StackTrace : ""), "Error!", icon: MessageBoxIcon.Error);
        public static DialogResult MessageBox(
            string Message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information, bool pIsDarkMode = true)
        {
            MsgIcon Icon = MsgIcon.None;
            switch (icon)
            {
                case MessageBoxIcon.Information: Icon = MsgIcon.Info; break;
                case MessageBoxIcon.Exclamation: Icon = MsgIcon.Warning; break;
                case MessageBoxIcon.Question: Icon = MsgIcon.Question; break;
                case MessageBoxIcon.Error: Icon = MsgIcon.Error; break;
                case MessageBoxIcon.None:
                default:
                    break;
            }

            return MessageBox(Message, title, Icon, buttons, pIsDarkMode);
        }


        public static DialogResult MessageBox(string Message, string title, MessageBoxButtons buttons,
            MessageBoxIcon icon, MessageBoxDefaultButton DefaultButton, bool pIsDarkMode = true)
        {
            _defaultButton = DefaultButton;
            return MessageBox(Message, title, buttons, icon, pIsDarkMode);
        }

        public static DialogResult MessageBox(string Message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK,
                                              MsgIcon icon = MsgIcon.None, bool pIsDarkMode = true)
        {
            return MessageBox(Message, title, icon, buttons, pIsDarkMode, owner: null);
        }

        public static DialogResult MessageBox(Form pOwner, string Message, string title,
            MessageBoxButtons buttons, MsgIcon icon = MsgIcon.None, bool pIsDarkMode = true)
        {
            return MessageBox(Message, title, icon, buttons, pIsDarkMode, owner: pOwner);
        }

        public static DialogResult MessageBox(
            string Message, string title, MsgIcon Icon,
            MessageBoxButtons buttons = MessageBoxButtons.OK, bool pIsDarkMode = true,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, Form? owner = null)
        {
            Form form = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = title,
                Width = 340,
                Height = 170,
                KeyPreview = true,
            };
            if (owner != null)
            {
                form.Owner = owner;
            }

            DarkModeCS DMode = new DarkModeCS(form)
            { ColorMode = pIsDarkMode ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode };
            DMode.ApplyTheme(pIsDarkMode);

            Base64Icons _Icons = new Base64Icons();

            Font systemFont = SystemFonts.DefaultFont;
            int fontHeight = systemFont.Height;
            #region Bottom Panel & Buttons

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = DMode.OScolors.Surface,
                ForeColor = DMode.OScolors.TextActive
            };
            form.Controls.Add(bottomPanel);
            string CurrentLanguage = GetCurrentLanguage();
            var ButtonTranslations = GetButtonTranslations(CurrentLanguage);


            List<Button> CmdButtons = new List<Button>();
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.OK,
                        Text = ButtonTranslations["OK"],
                        Height = fontHeight + 10,
                        FlatStyle = FlatStyle.System
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.KeyPreview = true;
                    form.KeyDown += (s, e) =>
                    { if (e.KeyCode == Keys.Escape) { form.Close(); } };
                    form.FormClosed += (s, e) =>
                    { form.DialogResult = DialogResult.OK; };
                    break;

                case MessageBoxButtons.OKCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.OK,
                        Text = ButtonTranslations["OK"],
                        Height = fontHeight + 10,
                        FlatStyle = FlatStyle.System
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"],
                        FlatStyle = FlatStyle.System
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Abort,
                        Text = ButtonTranslations["Abort"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Retry,
                        Text = ButtonTranslations["Retry"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Ignore,
                        Text = ButtonTranslations["Ignore"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.ControlBox = false;
                    break;

                case MessageBoxButtons.YesNoCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Yes,
                        Text = ButtonTranslations["Yes"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.No,
                        Text = ButtonTranslations["No"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[2];
                    break;

                case MessageBoxButtons.YesNo:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Yes,
                        Text = ButtonTranslations["Yes"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.No,
                        Text = ButtonTranslations["No"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.ControlBox = false;
                    break;

                case MessageBoxButtons.RetryCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Retry,
                        Text = ButtonTranslations["Retry"],
                        FlatStyle = FlatStyle.System
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;
            }

            int Padding = 4;
            int LastPos = form.ClientSize.Width;

            systemFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

            using (Graphics g = form.CreateGraphics())
            {
                for (int c = CmdButtons.Count - 1; c >= 0; c--)
                {
                    Button _button = CmdButtons[c];
                    _button.FlatAppearance.BorderColor = (form.AcceptButton == _button) ? DMode.OScolors.Accent : DMode.OScolors.Control;

                    bottomPanel.Controls.Add(_button);
                    _button.TabIndex = c;
                    _button.Font = systemFont;
                    SizeF textSize = g.MeasureString(_button.Text, systemFont);
                    _button.Size = new Size((int)textSize.Width + 20, systemFont.Height + 10);
                    _button.Location = new Point(LastPos - (_button.Width + Padding), (bottomPanel.Height - _button.Height) / 2);
                    LastPos = _button.Left;
                }
            }

            int b = (int)_defaultButton;
            if (b > 0)
            {
                b >>= 8;
                if (b < CmdButtons.Count)
                {
                    CmdButtons[b].Select();
                    CmdButtons[b].FlatStyle = FlatStyle.Flat;
                    CmdButtons[b].FlatAppearance.BorderColor = DMode.OScolors.AccentLight;
                }
            }

            #endregion Bottom Panel & Buttons

            #region Icon

            Rectangle picBox = new Rectangle(2, 10, 0, 0);
            if (Icon != MsgIcon.None)
            {
                PictureBox picIcon = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(64, 64) };
                picIcon.Image = _Icons.GetIcon(Icon);
                form.Controls.Add(picIcon);

                picBox.Size = new Size(64, 64);
                picIcon.SetBounds(picBox.X, picBox.Y, picBox.Width, picBox.Height);
                picIcon.BringToFront();
            }

            #endregion Icon

            #region Prompt Text

            Label lblPrompt = new Label
            {
                Text = Message,
                AutoSize = true,
                ForeColor = DMode.OScolors.TextActive,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(picBox.X + picBox.Width + 4, picBox.Y),
                MaximumSize = new Size(form.ClientSize.Width - (picBox.X + picBox.Width) + 8, 0),
                MinimumSize = new Size(form.ClientSize.Width - (picBox.X + picBox.Width) + 8, 64)
            };
            lblPrompt.BringToFront();
            form.Controls.Add(lblPrompt);
            #endregion Prompt Text

            form.ClientSize = new Size(340,
                bottomPanel.Height +
                lblPrompt.Height +
                20
            );
            #region Keyboard Shortcuts

            string localMessage = Message;
            string localTitle = title;

            form.KeyDown += (object? sender, KeyEventArgs e) =>
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    string clipboardText = $"Title: {localTitle}\r\nMessage: {localMessage}";
                    Clipboard.SetText(clipboardText);
                    e.Handled = true;
                }
            };
            #endregion


            return form.ShowDialog();
        }

        #endregion MessageBox

        #region InputBox

        public static DialogResult InputBox(
            string title, string promptText, ref List<KeyValue> Fields,
            MsgIcon Icon = 0, MessageBoxButtons buttons = MessageBoxButtons.OK, bool pIsDarkMode = true)
        {
            Form form = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = title,
                Width = 340,
                Height = 170
            };
            DarkModeCS DMode = new DarkModeCS(form) { ColorMode = pIsDarkMode ? DarkModeCS.DisplayMode.DarkMode : DarkModeCS.DisplayMode.ClearMode };
            DMode.ApplyTheme(pIsDarkMode);
            ErrorProvider Err = new ErrorProvider();
            Base64Icons _Icons = new Base64Icons();

            #region Bottom Panel

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = DMode.OScolors.Surface,
                ForeColor = DMode.OScolors.TextActive
            };
            form.Controls.Add(bottomPanel);

            #endregion Bottom Panel

            #region Icon

            if (Icon != MsgIcon.None)
            {
                PictureBox picIcon = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(48, 48) };
                picIcon.Image = _Icons.GetIcon(Icon);
                bottomPanel.Controls.Add(picIcon);

                picIcon.SetBounds(0, 2, 48, 48);
                picIcon.BringToFront();
            }

            #endregion Icon

            #region Buttons

            string CurrentLanguage = GetCurrentLanguage();
            var ButtonTranslations = GetButtonTranslations(CurrentLanguage);

            List<Button> CmdButtons = new List<Button>();
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.OK,
                        Text = ButtonTranslations["OK"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    break;

                case MessageBoxButtons.OKCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.OK,
                        Text = ButtonTranslations["OK"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;

                case MessageBoxButtons.AbortRetryIgnore:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Retry,
                        Text = ButtonTranslations["Retry"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Abort,
                        Text = ButtonTranslations["Abort"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;

                case MessageBoxButtons.YesNoCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Yes,
                        Text = ButtonTranslations["Yes"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.No,
                        Text = ButtonTranslations["No"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[2];
                    break;

                case MessageBoxButtons.YesNo:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Yes,
                        Text = ButtonTranslations["Yes"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.No,
                        Text = ButtonTranslations["No"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;

                case MessageBoxButtons.RetryCancel:
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Retry,
                        Text = ButtonTranslations["Retry"]
                    });
                    CmdButtons.Add(new Button
                    {
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        DialogResult = DialogResult.Cancel,
                        Text = ButtonTranslations["Cancel"]
                    });
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;
            }

            int Padding = 4;
            int LastPos = form.ClientSize.Width;

            foreach (var _button in CmdButtons)
            {
                _button.FlatAppearance.BorderColor = (form.AcceptButton == _button) ? DMode.OScolors.Accent : DMode.OScolors.Control;
                bottomPanel.Controls.Add(_button);

                _button.Location = new Point(LastPos - (_button.Width + Padding), (bottomPanel.Height - _button.Height) / 2);
                LastPos = _button.Left;
            }

            #endregion Buttons

            #region Prompt Text

            Label lblPrompt = new Label();
            if (!string.IsNullOrWhiteSpace(promptText))
            {
                lblPrompt.Dock = DockStyle.Top;
                lblPrompt.Text = promptText;
                lblPrompt.AutoSize = false;
                lblPrompt.Height = 24;
                lblPrompt.TextAlign = ContentAlignment.MiddleCenter;
            }
            else
            {
                lblPrompt.Location = new Point(0, 0);
                lblPrompt.Width = 0;
                lblPrompt.Height = 0;
            }
            form.Controls.Add(lblPrompt);
            #endregion Prompt Text

            #region Controls for KeyValues

            TableLayoutPanel Contenedor = new TableLayoutPanel
            {
                Size = new Size(form.ClientSize.Width - 20, 50),
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = DMode.OScolors.Background,
                AutoSize = true,
                ColumnCount = 2,
                Location = new Point(10, lblPrompt.Location.Y + lblPrompt.Height + 4)
            };
            form.Controls.Add(Contenedor);
            Contenedor.ColumnStyles.Clear();
            Contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Contenedor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute));
            Contenedor.ColumnStyles[1].Width = form.ClientSize.Width - 120;
            Contenedor.RowStyles.Clear();

            int ChangeDelayMS = 1000;
            int currentRow = 0;
            foreach (KeyValue field in Fields)
            {
                Label field_label = new Label
                {
                    Text = field.Key,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Control? field_Control = null;

                BorderStyle BStyle = (DMode.IsDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D);
                if (field.ValueType == ValueTypes.String)
                {
                    field_Control = new TextBox
                    {
                        Text = field.Value,
                        Dock = DockStyle.Fill,
                        TextAlign = HorizontalAlignment.Center
                    };
                    ((TextBox)field_Control).TextChanged += (sender, args) =>
                    {
                        AddTextChangedDelay((TextBox)field_Control, ChangeDelayMS, text =>
                        {
                            field.Value = ((TextBox)sender!).Text;
                            ((TextBox)sender!).Text = Convert.ToString(field.Value);
                            Err.SetError(field_Control, field.ErrorText);
                        });
                    };
                }
                if (field.ValueType == ValueTypes.Multiline)
                {
                    field_Control = new TextBox
                    {
                        Text = field.Value,
                        Dock = DockStyle.Fill,
                        TextAlign = HorizontalAlignment.Left,
                        Multiline = true,
                        ScrollBars = ScrollBars.Vertical
                    };
                    ((TextBox)field_Control).TextChanged += (sender, args) =>
                    {
                        AddTextChangedDelay((TextBox)field_Control, ChangeDelayMS, text =>
                        {
                            field.Value = ((TextBox)sender!).Text;
                            ((TextBox)sender!).Text = Convert.ToString(field.Value);
                            Err.SetError(field_Control, field.ErrorText);
                        });
                    };
                }
                if (field.ValueType == ValueTypes.Password)
                {
                    field_Control = new TextBox
                    {
                        Text = field.Value,
                        Dock = DockStyle.Fill,
                        UseSystemPasswordChar = true,
                        TextAlign = HorizontalAlignment.Center
                    };
                    ((TextBox)field_Control).TextChanged += (sender, args) =>
                    {
                        AddTextChangedDelay((TextBox)field_Control, ChangeDelayMS, text =>
                        {
                            field.Value = ((TextBox)sender!).Text;
                            ((TextBox)sender!).Text = Convert.ToString(field.Value);
                            Err.SetError(field_Control, field.ErrorText);
                        });
                    };
                }
                if (field.ValueType == ValueTypes.Integer)
                {
                    field_Control = new NumericUpDown
                    {
                        Minimum = int.MinValue,
                        Maximum = int.MaxValue,
                        TextAlign = HorizontalAlignment.Center,
                        Value = Convert.ToInt32(field.Value, CultureInfo.InvariantCulture),
                        ThousandsSeparator = true,
                        Dock = DockStyle.Fill,
                        DecimalPlaces = 0
                    };
                    ((NumericUpDown)field_Control).ValueChanged += (sender, args) =>
                    {
                        AddTextChangedDelay((NumericUpDown)field_Control, ChangeDelayMS, text =>
                        {
                            field.Value = ((NumericUpDown)sender!).Value.ToString(CultureInfo.InvariantCulture);
                            ((NumericUpDown)sender!).Value = Convert.ToInt32(field.Value, CultureInfo.InvariantCulture);
                            Err.SetError(field_Control, field.ErrorText);
                        });
                    };
                }
                if (field.ValueType == ValueTypes.Decimal)
                {
                    field_Control = new NumericUpDown
                    {
                        Minimum = int.MinValue,
                        Maximum = int.MaxValue,
                        TextAlign = HorizontalAlignment.Center,
                        Value = Convert.ToDecimal(field.Value, CultureInfo.InvariantCulture),
                        ThousandsSeparator = false,
                        Dock = DockStyle.Fill,
                        DecimalPlaces = 2
                    };
                    ((NumericUpDown)field_Control).ValueChanged += (sender, args) =>
                    {
                        AddTextChangedDelay((NumericUpDown)field_Control, ChangeDelayMS, text =>
                        {
                            field.Value = ((NumericUpDown)sender!).Value.ToString(CultureInfo.InvariantCulture);
                            ((NumericUpDown)sender!).Value = Convert.ToDecimal(field.Value, CultureInfo.InvariantCulture);
                            Err.SetError(field_Control, field.ErrorText);
                        });
                    };
                }
                if (field.ValueType == ValueTypes.Date)
                {
                    field_Control = new DateTimePicker
                    {
                        Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture),
                        Dock = DockStyle.Fill,
                        Format = DateTimePickerFormat.Short,

                        CalendarForeColor = DMode.OScolors.TextActive,
                        CalendarMonthBackground = DMode.OScolors.Control,
                        CalendarTitleBackColor = DMode.OScolors.Surface,
                        CalendarTitleForeColor = DMode.OScolors.TextActive
                    };
                    ((DateTimePicker)field_Control).ValueChanged += (sender, args) =>
                    {
                        field.Value = ((DateTimePicker)sender!).Value.ToString("o");
                        ((DateTimePicker)sender!).Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture);
                        Err.SetError(field_Control, field.ErrorText);
                        Err.SetIconAlignment(field_Control, ErrorIconAlignment.MiddleLeft);
                    };
                }
                if (field.ValueType == ValueTypes.Time)
                {
                    field_Control = new DateTimePicker
                    {
                        Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture),
                        Dock = DockStyle.Fill,
                        Format = DateTimePickerFormat.Time
                    };
                    ((DateTimePicker)field_Control).ValueChanged += (sender, args) =>
                    {
                        field.Value = ((DateTimePicker)sender!).Value.ToString("o");
                        ((DateTimePicker)sender!).Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture);
                        Err.SetError(field_Control, field.ErrorText);
                        Err.SetIconAlignment(field_Control, ErrorIconAlignment.MiddleLeft);
                    };
                }
                if (field.ValueType == ValueTypes.Boolean)
                {
                    field_Control = new CheckBox
                    {
                        Checked = Convert.ToBoolean(field.Value),
                        Dock = DockStyle.Fill,
                        Text = field.Key
                    };
                    ((CheckBox)field_Control).CheckedChanged += (sender, args) =>
                    {
                        field.Value = ((CheckBox)sender!).Checked.ToString();
                        ((CheckBox)sender!).Checked = Convert.ToBoolean(field.Value);
                        Err.SetError(field_Control, field.ErrorText);
                    };
                }
                if (field.ValueType == ValueTypes.Dynamic)
                {
                    field_Control = new FlatComboBox
                    {
                        DataSource = field.DataSet,
                        ValueMember = "Value",
                        DisplayMember = "Key",
                        Dock = DockStyle.Fill,
                        BackColor = DMode.OScolors.Control,
                        ButtonColor = DMode.OScolors.Surface,
                        ForeColor = DMode.OScolors.TextActive,
                        SelectedValue = field.Value,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FlatStyle = (DMode.IsDarkMode ? FlatStyle.Flat : FlatStyle.Standard)
                    };
                    ((ComboBox)field_Control).SelectedValueChanged += (sender, args) =>
                    {
                        field.Value = ((ComboBox)sender!).SelectedValue.ToString()!;
                        ((ComboBox)sender!).SelectedValue = Convert.ToString(field.Value)!;
                        Err.SetError(field_Control, field.ErrorText);
                    };
                }

                Contenedor.Controls.Add(field_label, 0, currentRow);

                if (field_Control != null)
                {
                    if (field.ValueType == ValueTypes.Multiline)
                    {
                        Contenedor.Controls.Add(field_Control, 1, currentRow);
                        const int spanRow = 6;
                        for (int i = 0; i < spanRow; i++)
                        {
                            currentRow++;
                            Contenedor.RowCount++;
                            Contenedor.RowStyles.Add(new RowStyle(SizeType.Absolute, field_Control.Height));
                        }
                        Contenedor.SetRowSpan(field_Control, spanRow);
                    }
                    else
                    {
                        Contenedor.Controls.Add(field_Control, 1, currentRow);
                    }

                    Err.SetIconAlignment(field_Control, ErrorIconAlignment.MiddleLeft);
                    if (field_Control is ComboBox box)
                    {
                        box.CreateControl();
                        box.SelectedValue = field.Value;
                    }

                    field_Control.TabIndex = currentRow;
                }
                currentRow++;
            }

            Contenedor.Width = form.ClientSize.Width - 20;
            #endregion Controls for KeyValues

            form.ClientSize = new Size(340,
                bottomPanel.Height +
                lblPrompt.Height +
                Contenedor.Height +
                20
            );
            form.FormClosing += (sender, e) =>
            {
                if (form.ActiveControl == form.AcceptButton)
                {
                    ValidateEventArgs cArgs = new ValidateEventArgs(null);
                    ValidateControlsHandler?.Invoke(form, cArgs);

                    e.Cancel = cArgs.Cancel;
                    if (!e.Cancel)
                    {
                        form.DialogResult = form.AcceptButton!.DialogResult;
                    }
                }
            };
            return form.ShowDialog();
        }

        #endregion InputBox

        #region Private Stuff

        private static Dictionary<Control, Timer>? timers;

        private static void AddTextChangedDelay<TControl>(TControl control, int milliseconds, Action<TControl> action) where TControl : Control
        {
            if (timers == null)
            {
                timers = new Dictionary<Control, Timer>();
            }

            if (timers.ContainsKey(control))
            {
                timers[control].Stop();
                timers.Remove(control);
            }

            var timer = new Timer();
            timer.Interval = milliseconds;
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                timers.Remove(control);
                action(control);
            };
            timer.Start();
            timers.Add(control, timer);
        }

        public static string GetCurrentLanguage(string pDefault = "en")
        {
            string _ret = pDefault;
            string CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (IsCurrentLanguageSupported(new List<string> { "en", "es", "fr", "de", "ru", "ko", "pt" }, CurrentLanguage))
            {
                _ret = CurrentLanguage;
            }
            if (CurrentLanguage.ToLowerInvariant().Equals("zh"))
            {
                var LangVariable = CultureInfo.CurrentCulture.Name;
                if (string.Equals(LangVariable, "zh-CN") || string.Equals(LangVariable, "zh-SG") || string.Equals(LangVariable, "zh-Hans"))
                {
                    _ret = "zh-Hans";
                }
                else if (string.Equals(LangVariable, "zh-TW") || string.Equals(LangVariable, "zh-HK") || string.Equals(LangVariable, "zh-MO") || string.Equals(LangVariable, "zh-Hant"))
                {
                    _ret = "zh-Hant";
                }
                else
                {
                    _ret = "zh-Hans";
                }
            }
            return _ret;
        }

        private static Dictionary<string, string> GetButtonTranslations(string pLanguage)
        {
            Dictionary<string, string> _ret = new Dictionary<string, string>();
            Dictionary<string, string> ButtonTranslations = new Dictionary<string, string> {
                { "en", "OK|Cancel|Yes|No|Continue|Retry|Abort|Ignore|Try Again" },
                { "es", "Aceptar|Cancelar|Sí|No|Continuar|Reintentar|Abortar|Ignorar|Intentar" },
                { "fr", "Accepter|Annuler|Oui|Non|Continuer|Réessayer|Abandonner|Ignorer|Essayer" },
                { "de", "Akzeptieren|Abbrechen|Ja|Nein|Weiter|Wiederholen|Abbrechen|Ignorieren|Versuchen" },
                { "ru", "Принять|Отменить|Да|Нет|Продолжить|Повторить|Прервать|Игнорировать|Пытаться" },
                { "ko", "확인|취소|예|아니오|계속|다시 시도|중단|무시|써 보다" },
                { "pt", "Aceitar|Cancelar|Sim|Não|Continuar|Tentar novamente|Abortar|Ignorar|Tentar" },
                { "zh-Hans", "确定|取消|是|否|继续|重试|中止|忽略|尝试" },
                { "zh-Hant", "確定|取消|是|否|繼續|重試|中止|忽略|嘗試" }
              };
            string? raw = ButtonTranslations[pLanguage];
            if (!string.IsNullOrEmpty(raw))
            {
                var Words = raw.Split('|').ToList();
                _ret = new Dictionary<string, string> {
                    { "OK", Words[0] },
                    { "Cancel", Words[1] },
                    { "Yes", Words[2] },
                    { "No", Words[3] },
                    { "Continue", Words[4] },
                    { "Retry", Words[5] },
                    { "Abort", Words[6] },
                    { "Ignore", Words[7] },
                    { "Try Again", Words[8] }
                };
            }

            return _ret;
        }

        private static bool IsCurrentLanguageSupported(List<string> languages, string currentLanguage)
        {
            if (languages == null || currentLanguage == null)
            {
                throw new ArgumentNullException(languages == null ? nameof(languages) : nameof(currentLanguage));
            }

            currentLanguage = currentLanguage.ToLowerInvariant();
            if (languages.Contains(currentLanguage))
            {
                return true;
            }

            if (currentLanguage.Length >= 2)
            {
                string baseLanguage = currentLanguage.Substring(0, 2);
                return languages.Contains(baseLanguage);
            }

            return false;
        }

        #endregion Private Stuff
    }

    public enum MsgIcon
    {
        None = 0,
        Info,
        Success,
        Warning,
        Error,
        Question,
        Lock,
        User,
        Forbidden,
        AddNew,
        Cancel,
        Edit,
        List
    }

    public class KeyValue
    {
        #region Private Members

        private string _value = string.Empty;
        #endregion Private Members

        #region Contructors

        public KeyValue()
        {
        }

        public KeyValue(string pKey, string pValue, ValueTypes pType = 0, List<KeyValue>? pDataSet = null)
        {
            Key = pKey;
            Value = pValue;
            ValueType = pType;
            DataSet = pDataSet;
        }

        #endregion Contructors

        #region Public Properties

        public enum ValueTypes
        {
            String = 0,
            Integer = 1,
            Decimal = 2,
            Date = 3,
            Time,
            Boolean,
            Dynamic,
            Password,
            Multiline
        }

        public string Key { get; set; } = string.Empty;

        public string Value
        {
            get => _value;
            set
            {
                var newValue = value;
                OnValidate(ref newValue);

                if (_value != newValue)
                {
                    _value = newValue;
                }
            }
        }

        public ValueTypes ValueType { get; set; } = ValueTypes.String;

        public List<KeyValue>? DataSet { get; set; }

        public string ErrorText { get; set; } = string.Empty;
        #endregion Public Properties

        #region Public Events

        public class ValidateEventArgs : EventArgs
        {
            public ValidateEventArgs(string? newValue)
            {
                NewValue = newValue;
                Cancel = false;
            }

            public string? NewValue { get; }
            public string OldValue { get; set; } = string.Empty;

            public bool Cancel { get; set; }
            public string ErrorText { get; set; } = string.Empty;
        }

        public event EventHandler<ValidateEventArgs>? Validate;
        protected virtual void OnValidate(ref string newValue)
        {
            var validateHandler = Validate;
            if (validateHandler != null)
            {
                var args = new ValidateEventArgs(newValue) { OldValue = _value };
                validateHandler(this, args);

                if (args.Cancel) { newValue = _value; }

                ErrorText = args.ErrorText;
            }
        }

        #endregion Public Events

        #region Public Methods

        public override string ToString()
        {
            return string.Format("{0} - {1}", Key, Value);
        }

        #endregion Public Methods
    }

    public class Base64Image
    {
        public Base64Image()
        {
        }

        public Base64Image(string pName, string pBase64Data)
        {
            Name = pName;
            Base64Data = pBase64Data;
        }

        public string Name { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;

        public Image? Image
        {
            get
            {
                if (string.IsNullOrEmpty(Base64Data)) return null;
                return System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(Base64Data)));
            }
        }
    }

    public class Base64Icons
    {
        private List<Base64Image> _Icons = new List<Base64Image> {
        new Base64Image("Info", "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")
        };
        public List<Base64Image> Icons
        {
            get
            {
                return this._Icons;
            }
            set
            {
                this._Icons = value;
            }
        }

        public Image? GetIcon(string pName)
        {
            Image? _ret = null;
            if (_Icons != null && _Icons.Count > 0)
            {
                var Found = _Icons.Find(x => x.Name == pName);
                if (Found != null)
                {
                    _ret = Found.Image;
                }
            }
            return _ret;
        }

        public Image? GetIcon(MsgIcon pIcon) => GetIcon(pIcon.ToString());
        public bool AddIcon(string pName, string pFilePath)
        {
            bool _ret = false;
            if (!string.IsNullOrEmpty(pFilePath) && File.Exists(pFilePath))
            {
                var _icon = File.ReadAllBytes(pFilePath);
                if (_icon != null)
                {
                    if (_Icons is null) _Icons = new List<Base64Image>();
                    _Icons.Add(new Base64Image(pName, Convert.ToBase64String(_icon)));
                    _ret = true;
                }
            }
            return _ret;
        }
    }
}
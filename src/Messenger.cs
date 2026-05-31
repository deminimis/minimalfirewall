// File: Messenger.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static DarkModeForms.KeyValue;
using Timer = System.Windows.Forms.Timer;

namespace DarkModeForms
{
    public static class Messenger
    {
        private static readonly Base64Icons _sharedIcons = new();
        private static readonly Dictionary<string, string> _rawTranslations = new()
        {
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

        #region Events

        private static Action<object, ValidateEventArgs>? ValidateControlsHandler;

        public static event Action<object, ValidateEventArgs>? ValidateControls
        {
            add => ValidateControlsHandler += value;
            remove => ValidateControlsHandler -= value;
        }

        public static void ResetEvents()
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

        public static DialogResult MessageBox(string Message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information, bool pIsDarkMode = true)
        {
            MsgIcon msgIcon = MsgIcon.None;
            switch (icon)
            {
                case MessageBoxIcon.Information: msgIcon = MsgIcon.Info; break;
                case MessageBoxIcon.Exclamation: msgIcon = MsgIcon.Warning; break;
                case MessageBoxIcon.Question: msgIcon = MsgIcon.Question; break;
                case MessageBoxIcon.Error: msgIcon = MsgIcon.Error; break;
            }

            return MessageBox(Message: Message, title: title, icon: msgIcon, buttons: buttons, pIsDarkMode: pIsDarkMode, defaultButton: _defaultButton, owner: null);
        }

        public static DialogResult MessageBox(string Message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton DefaultButton, bool pIsDarkMode = true)
        {
            _defaultButton = DefaultButton;
            return MessageBox(Message, title, buttons, icon, pIsDarkMode);
        }

        public static DialogResult MessageBox(string Message, string title, MessageBoxButtons buttons = MessageBoxButtons.OK, MsgIcon icon = MsgIcon.None, bool pIsDarkMode = true)
        {
            // Explicitly name arguments 
            return MessageBox(Message: Message, title: title, icon: icon, buttons: buttons, pIsDarkMode: pIsDarkMode, defaultButton: _defaultButton, owner: null);
        }

        public static DialogResult MessageBox(Form pOwner, string Message, string title, MessageBoxButtons buttons, MsgIcon icon = MsgIcon.None, bool pIsDarkMode = true)
        {
            // Explicitly name arguments
            return MessageBox(Message: Message, title: title, icon: icon, buttons: buttons, pIsDarkMode: pIsDarkMode, defaultButton: _defaultButton, owner: pOwner);
        }

        public static DialogResult MessageBox(string Message, string title, MsgIcon icon, MessageBoxButtons buttons = MessageBoxButtons.OK, bool pIsDarkMode = true, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1, Form? owner = null)
        {
            using var form = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen,
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

            Theme.Colors = Theme.GetSystemColors(pIsDarkMode ? 0 : 1);
            Theme.ApplyTitleBarTheme(form.Handle, pIsDarkMode ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            form.BackColor = Theme.Colors.Background;
            form.ForeColor = Theme.Colors.TextInactive;

            Font systemFont = SystemFonts.DefaultFont;
            int fontHeight = systemFont.Height;

            #region Bottom Panel & Buttons

            var bottomPanel = new ThemedPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Theme.Colors.Surface,
                ForeColor = Theme.Colors.TextActive
            };
            form.Controls.Add(bottomPanel);

            string CurrentLanguage = GetCurrentLanguage();
            var ButtonTranslations = GetButtonTranslations(CurrentLanguage);
            List<ThemedButton> CmdButtons = GenerateDialogButtons(form, bottomPanel, buttons, ButtonTranslations, fontHeight);

            int Padding = 4;
            int LastPos = form.ClientSize.Width;

            systemFont = SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont;

            for (int c = CmdButtons.Count - 1; c >= 0; c--)
            {
                ThemedButton _button = CmdButtons[c];
                _button.FlatAppearance.BorderColor = (form.AcceptButton == _button) ? Theme.Colors.Accent : Theme.Colors.Control;

                _button.TabIndex = c;
                _button.Font = systemFont;
                Size textSize = TextRenderer.MeasureText(_button.Text, systemFont);
                _button.Size = new Size(textSize.Width + 20, systemFont.Height + 10);
                _button.Location = new Point(LastPos - (_button.Width + Padding), (bottomPanel.Height - _button.Height) / 2);
                LastPos = _button.Left;
            }

            int b = (int)_defaultButton;
            if (b >= 0)
            {
                b >>= 8;
                if (b < CmdButtons.Count)
                {
                    CmdButtons[b].Select();
                    CmdButtons[b].FlatStyle = FlatStyle.Flat;
                    CmdButtons[b].FlatAppearance.BorderColor = Theme.Colors.AccentLight;
                }
            }

            #endregion Bottom Panel & Buttons

            #region Icon

            var picBox = new Rectangle(2, 10, 0, 0);
            if (icon != MsgIcon.None)
            {
                var picIcon = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(64, 64),
                    Image = _sharedIcons.GetIcon(icon)
                };

                // Fallback to system icons 
                picIcon.Image ??= GetSafeSystemIcon(icon, 64);

                form.Controls.Add(picIcon);

                picBox.Size = new Size(64, 64);
                picIcon.SetBounds(picBox.X, picBox.Y, picBox.Width, picBox.Height);
                picIcon.BringToFront();
            }

            #endregion Icon

            #region Prompt Text

            int lblWidth = Math.Max(10, form.ClientSize.Width - (picBox.X + picBox.Width) - 12); // Fix: Prevent negative/zero scale measurements
            var lblPrompt = new ThemedLabel
            {
                Text = Message,
                AutoSize = true,
                ForeColor = Theme.Colors.TextActive,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(picBox.X + picBox.Width + 4, picBox.Y),
                MaximumSize = new Size(lblWidth, 0),
                MinimumSize = new Size(lblWidth, 0) 
            };
            form.Controls.Add(lblPrompt);
            lblPrompt.BringToFront();

            int promptHeight = Math.Max(64, lblPrompt.GetPreferredSize(new Size(lblWidth, 0)).Height);
            lblPrompt.MinimumSize = new Size(lblWidth, promptHeight);

            #endregion Prompt Text

            form.ClientSize = new Size(340, bottomPanel.Height + promptHeight + 20);

            #region Keyboard Shortcuts

            string localMessage = Message;
            string localTitle = title;

            form.KeyDown += (sender, e) =>
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

        public static DialogResult InputBox(string title, string promptText, ref List<KeyValue> Fields, MsgIcon icon = MsgIcon.None, MessageBoxButtons buttons = MessageBoxButtons.OK, bool pIsDarkMode = true)
        {
            using var form = new Form
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                Text = title,
                Width = 340,
                Height = 170
            };
            Theme.Colors = Theme.GetSystemColors(pIsDarkMode ? 0 : 1);
            Theme.ApplyTitleBarTheme(form.Handle, pIsDarkMode ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            form.BackColor = Theme.Colors.Background;
            form.ForeColor = Theme.Colors.TextInactive;

            var Err = new ErrorProvider();

            #region Bottom Panel

            var bottomPanel = new ThemedPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Theme.Colors.Surface,
                ForeColor = Theme.Colors.TextActive
            };
            form.Controls.Add(bottomPanel);

            #endregion Bottom Panel

            #region Icon

            if (icon != MsgIcon.None)
            {
                var picIcon = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(48, 48),
                    Image = _sharedIcons.GetIcon(icon)
                };

                // Fallback to system icons 
                picIcon.Image ??= GetSafeSystemIcon(icon, 48);

                bottomPanel.Controls.Add(picIcon);
                picIcon.SetBounds(0, 2, 48, 48);
                picIcon.BringToFront();
            }

            #endregion Icon

            #region Buttons

            string CurrentLanguage = GetCurrentLanguage();
            var ButtonTranslations = GetButtonTranslations(CurrentLanguage);
            List<ThemedButton> CmdButtons = GenerateDialogButtons(form, bottomPanel, buttons, ButtonTranslations, SystemFonts.DefaultFont.Height);

            int Padding = 4;
            int LastPos = form.ClientSize.Width;
            foreach (var _button in CmdButtons)
            {
                _button.FlatAppearance.BorderColor = (form.AcceptButton == _button) ? Theme.Colors.Accent : Theme.Colors.Control;
                _button.Location = new Point(LastPos - (_button.Width + Padding), (bottomPanel.Height - _button.Height) / 2);
                LastPos = _button.Left;
            }

            #endregion Buttons

            #region Prompt Text

            var lblPrompt = new ThemedLabel();
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

            var Contenedor = new TableLayoutPanel
            {
                Size = new Size(form.ClientSize.Width - 20, 50),
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.Colors.Background,
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
                var field_label = new ThemedLabel
                {
                    Text = field.Key,
                    AutoSize = false,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Control? field_Control = null;

                switch (field.ValueType)
                {
                    case ValueTypes.String:
                    case ValueTypes.Multiline:
                    case ValueTypes.Password:
                        {
                            bool isMulti = field.ValueType == ValueTypes.Multiline;
                            var txtBox = new TextBox
                            {
                                Text = field.Value,
                                Dock = DockStyle.Fill,
                                TextAlign = isMulti ? HorizontalAlignment.Left : HorizontalAlignment.Center,
                                Multiline = isMulti,
                                ScrollBars = isMulti ? ScrollBars.Vertical : ScrollBars.None,
                                UseSystemPasswordChar = field.ValueType == ValueTypes.Password
                            };
                            txtBox.TextChanged += (sender, args) =>
                            {
                                AddTextChangedDelay(txtBox, ChangeDelayMS, text =>
                                {
                                    field.Value = ((TextBox)sender!).Text;
                                    ((TextBox)sender!).Text = Convert.ToString(field.Value);
                                    Err.SetError(txtBox, field.ErrorText);
                                });
                            };
                            field_Control = txtBox;
                        }
                        break;
                    case ValueTypes.Integer:
                    case ValueTypes.Decimal:
                        {
                            bool isDec = field.ValueType == ValueTypes.Decimal;
                            var num = new NumericUpDown
                            {
                                Minimum = int.MinValue,
                                Maximum = int.MaxValue,
                                TextAlign = HorizontalAlignment.Center,
                                Value = Convert.ToDecimal(field.Value, CultureInfo.InvariantCulture),
                                ThousandsSeparator = !isDec,
                                Dock = DockStyle.Fill,
                                DecimalPlaces = isDec ? 2 : 0
                            };
                            num.ValueChanged += (sender, args) =>
                            {
                                AddTextChangedDelay(num, ChangeDelayMS, text =>
                                {
                                    field.Value = num.Value.ToString(CultureInfo.InvariantCulture);
                                    num.Value = Convert.ToDecimal(field.Value, CultureInfo.InvariantCulture);
                                    Err.SetError(num, field.ErrorText);
                                });
                            };
                            field_Control = num;
                        }
                        break;
                    case ValueTypes.Date:
                    case ValueTypes.Time:
                        {
                            bool isDate = field.ValueType == ValueTypes.Date;
                            var dt = new DateTimePicker
                            {
                                Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture),
                                Dock = DockStyle.Fill,
                                Format = isDate ? DateTimePickerFormat.Short : DateTimePickerFormat.Time
                            };
                            if (isDate)
                            {
                                dt.CalendarForeColor = Theme.Colors.TextActive;
                                dt.CalendarMonthBackground = Theme.Colors.Control;
                                dt.CalendarTitleBackColor = Theme.Colors.Surface;
                                dt.CalendarTitleForeColor = Theme.Colors.TextActive;
                            }
                            dt.ValueChanged += (sender, args) =>
                            {
                                field.Value = dt.Value.ToString("o");
                                dt.Value = Convert.ToDateTime(field.Value, CultureInfo.InvariantCulture);
                                Err.SetError(dt, field.ErrorText);
                                Err.SetIconAlignment(dt, ErrorIconAlignment.MiddleLeft);
                            };
                            field_Control = dt;
                        }
                        break;
                    case ValueTypes.Boolean:
                        {
                            var chk = new CheckBox { Checked = Convert.ToBoolean(field.Value), Dock = DockStyle.Fill, Text = field.Key };
                            chk.CheckedChanged += (sender, args) =>
                            {
                                field.Value = chk.Checked.ToString();
                                chk.Checked = Convert.ToBoolean(field.Value);
                                Err.SetError(chk, field.ErrorText);
                            };
                            field_Control = chk;
                        }
                        break;
                    case ValueTypes.Dynamic:
                        {
                            var combo = new ComboBox { DataSource = field.DataSet, ValueMember = "Value", DisplayMember = "Key", Dock = DockStyle.Fill, BackColor = Theme.Colors.Control, ForeColor = Theme.Colors.TextActive, SelectedValue = field.Value, DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = (pIsDarkMode ? FlatStyle.Flat : FlatStyle.Standard) };
                            combo.SelectedValueChanged += (sender, args) =>
                            {
                                field.Value = combo.SelectedValue.ToString()!;
                                combo.SelectedValue = Convert.ToString(field.Value)!;
                                Err.SetError(combo, field.ErrorText);
                            };
                            field_Control = combo;
                        }
                        break;
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

            form.ClientSize = new Size(340, bottomPanel.Height + lblPrompt.Height + Contenedor.Height + 20);

            form.FormClosing += (sender, e) =>
            {
                if (form.ActiveControl == form.AcceptButton)
                {
                    var cArgs = new ValidateEventArgs(null);
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

        private static readonly Dictionary<Control, (Timer timer, EventHandler disposedHandler)> timers = [];

        private static void AddTextChangedDelay<TControl>(TControl control, int milliseconds, Action<TControl> action) where TControl : Control
        {
            if (timers.TryGetValue(control, out var existingEntry))
            {
                existingEntry.timer.Stop();
                existingEntry.timer.Dispose();
                control.Disposed -= existingEntry.disposedHandler;
                timers.Remove(control);
            }

            var timer = new Timer { Interval = milliseconds };
            EventHandler? disposedHandler = null;

            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                if (timers.ContainsKey(control))
                {
                    timers.Remove(control);
                }
                if (disposedHandler != null)
                {
                    control.Disposed -= disposedHandler;
                }
                action(control);
                timer.Dispose();
            };

            disposedHandler = (sender, e) =>
            {
                if (timers.TryGetValue(control, out var entryToDispose))
                {
                    entryToDispose.timer.Stop();
                    entryToDispose.timer.Dispose();
                    timers.Remove(control);
                }
            };

            control.Disposed += disposedHandler;
            timer.Start();
            timers.Add(control, (timer, disposedHandler));
        }

        private static List<ThemedButton> GenerateDialogButtons(Form form, Panel bottomPanel, MessageBoxButtons buttons, Dictionary<string, string> translations, int fontHeight)
        {
            var CmdButtons = new List<ThemedButton>();
            ThemedButton CreateBtn(DialogResult result, string textKey, bool isFlat = true)
            {
                var btn = new ThemedButton
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    DialogResult = result,
                    Text = translations.TryGetValue(textKey, out string? translated) ? translated : textKey,
                    Height = fontHeight + 10,
                    FlatStyle = isFlat ? FlatStyle.System : FlatStyle.Standard
                };
                bottomPanel.Controls.Add(btn); 
                return btn;
            }

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    CmdButtons.Add(CreateBtn(DialogResult.OK, "OK"));
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[0];
                    break;
                case MessageBoxButtons.OKCancel:
                    CmdButtons.Add(CreateBtn(DialogResult.OK, "OK"));
                    CmdButtons.Add(CreateBtn(DialogResult.Cancel, "Cancel"));
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    CmdButtons.Add(CreateBtn(DialogResult.Abort, "Abort", false));
                    CmdButtons.Add(CreateBtn(DialogResult.Retry, "Retry", false));
                    CmdButtons.Add(CreateBtn(DialogResult.Ignore, "Ignore", false));
                    form.AcceptButton = CmdButtons[0];
                    form.ControlBox = false;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    CmdButtons.Add(CreateBtn(DialogResult.Yes, "Yes", false));
                    CmdButtons.Add(CreateBtn(DialogResult.No, "No", false));
                    CmdButtons.Add(CreateBtn(DialogResult.Cancel, "Cancel", false));
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[2];
                    break;
                case MessageBoxButtons.YesNo:
                    CmdButtons.Add(CreateBtn(DialogResult.Yes, "Yes", false));
                    CmdButtons.Add(CreateBtn(DialogResult.No, "No", false));
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1]; 
                    form.ControlBox = false;
                    break;
                case MessageBoxButtons.RetryCancel:
                    CmdButtons.Add(CreateBtn(DialogResult.Retry, "Retry"));
                    CmdButtons.Add(CreateBtn(DialogResult.Cancel, "Cancel", false));
                    form.AcceptButton = CmdButtons[0];
                    form.CancelButton = CmdButtons[1];
                    break;
            }

            return CmdButtons;
        }

        public static string GetCurrentLanguage(string pDefault = "en")
        {
            string _ret = pDefault;
            string CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (IsCurrentLanguageSupported(["en", "es", "fr", "de", "ru", "ko", "pt"], CurrentLanguage))
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
            if (_rawTranslations.TryGetValue(pLanguage, out string? raw))
            {
                var Words = raw.Split('|');
                // Basic check to ensure we have enough words before mapping
                if (Words.Length >= 9)
                {
                    return new Dictionary<string, string> {
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
            }
            // Fallback to English if translation is missing or malformed
            return GetButtonTranslations("en");
        }

        private static bool IsCurrentLanguageSupported(List<string> languages, string currentLanguage)
        {
            if (languages == null || currentLanguage == null)
            {
                throw new ArgumentNullException(nameof(languages));
            }

            currentLanguage = currentLanguage.ToLowerInvariant();
            if (languages.Contains(currentLanguage))
            {
                return true;
            }

            if (currentLanguage.Length >= 2)
            {
                return languages.Contains(currentLanguage[..2]);
            }

            return false;
        }

        private static Bitmap? GetSafeSystemIcon(MsgIcon icon, int size)
        {
            Icon? sysIcon = icon switch
            {
                MsgIcon.Warning => SystemIcons.Warning,
                MsgIcon.Error => SystemIcons.Error,
                MsgIcon.Question => SystemIcons.Question,
                MsgIcon.Success or MsgIcon.Info => SystemIcons.Information,
                _ => null
            };

            if (sysIcon == null) return null;

            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawIcon(sysIcon, new Rectangle(0, 0, size, size));
            return bmp;
        }

        #endregion Private Stuff
    }

    public enum MsgIcon
    {
        None = 0, Info, Success, Warning, Error, Question, Lock, User, Forbidden, AddNew, Cancel, Edit, List
    }

    public class KeyValue
    {
        private string _value = string.Empty;

        public KeyValue() { }

        public KeyValue(string pKey, string pValue, ValueTypes pType = ValueTypes.String, List<KeyValue>? pDataSet = null)
        {
            Key = pKey;
            Value = pValue;
            ValueType = pType;
            DataSet = pDataSet;
        }

        public enum ValueTypes { String = 0, Integer = 1, Decimal = 2, Date = 3, Time, Boolean, Dynamic, Password, Multiline }

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

        public class ValidateEventArgs(string? newValue) : EventArgs
        {
            public string? NewValue { get; } = newValue; public string OldValue { get; set; } = string.Empty;
            public bool Cancel { get; set; } = false; public string ErrorText { get; set; } = string.Empty;
        }

        public event EventHandler<ValidateEventArgs>? Validate;
        protected virtual void OnValidate(ref string newValue)
        {
            Validate?.Invoke(this, new ValidateEventArgs(newValue) { OldValue = _value });
        }

        public override string ToString() => string.Format("{0} - {1}", Key, Value);
    }

    public class Base64Image(string pName, string pBase64Data)
    {
        private Image? _cachedImage;

        public string Name { get; set; } = pName;
        public string Base64Data { get; set; } = pBase64Data;

        public Image? Image
        {
            get
            {
                if (_cachedImage != null)
                {
                    return _cachedImage;
                }

                if (string.IsNullOrEmpty(Base64Data))
                {
                    return null;
                }

                try
                {
                    using var ms = new MemoryStream(Convert.FromBase64String(Base64Data));
                    _cachedImage = new Bitmap(Image.FromStream(ms));
                }
                catch { return null; }

                return _cachedImage;
            }
        }
    }

    public class Base64Icons
    {
        // Stores large icon string constant
        private List<Base64Image> _Icons = [
            new Base64Image("Info", "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")
        ];
        public List<Base64Image> Icons
        {
            get => _Icons;
            set => _Icons = value;
        }

        public Image? GetIcon(string pName) => _Icons?.Find(x => x.Name == pName)?.Image;

        public Image? GetIcon(MsgIcon pIcon) => GetIcon(pIcon.ToString());

        public bool AddIcon(string pName, string pFilePath)
        {
            if (string.IsNullOrEmpty(pFilePath) || !File.Exists(pFilePath))
            {
                return false;
            }

            _Icons ??= [];
            _Icons.Add(new Base64Image(pName, Convert.ToBase64String(File.ReadAllBytes(pFilePath))));
            return true;
        }
    }
}

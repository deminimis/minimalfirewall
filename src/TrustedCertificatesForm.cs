
using System.Data;
using System.Security.Cryptography.X509Certificates;
using DarkModeForms;

namespace MinimalFirewall
{
    public partial class TrustedCertificatesForm : Form
    {
        

        public TrustedCertificatesForm(AppSettings appSettings)
        {
            InitializeComponent();

            bool isDark = appSettings.Theme == "Dark" || (appSettings.Theme == "Auto" && Theme.IsSystemDarkMode());
            Theme.Colors = Theme.GetSystemColors(isDark ? 0 : 1);
            Theme.ApplyTitleBarTheme(this.Handle, isDark ? Theme.DisplayMode.DarkMode : Theme.DisplayMode.ClearMode);
            this.BackColor = Theme.Colors.Background;
            this.ForeColor = Theme.Colors.TextInactive;


            LoadCertificates();
        }

        private void LoadCertificates()
        {
            var table = new DataTable();
            table.Columns.Add("Certificate Name", typeof(string));
            table.Columns.Add("Issued By", typeof(string));
            table.Columns.Add("Expires", typeof(string));
            table.Columns.Add("Store", typeof(string));

            var seen = new HashSet<string>();

            foreach (StoreLocation location in Enum.GetValues(typeof(StoreLocation)))
            {
                using var store = new X509Store(StoreName.Root, location);
                store.Open(OpenFlags.ReadOnly);

                foreach (var cert in store.Certificates)
                {
                    string thumbprint = cert.Thumbprint ?? "";
                    if (!seen.Add(thumbprint))
                    {
                        continue;
                    }

                    string subject = cert.Subject ?? "";
                    string friendlyName = cert.FriendlyName ?? "";
                    string issuer = cert.Issuer ?? "";
                    string expiry = cert.NotAfter.ToString("yyyy-MM-dd");
                    string locationLabel = location == StoreLocation.LocalMachine ? "System" : "User";

                    string displayName = !string.IsNullOrEmpty(friendlyName)
                        ? friendlyName
                        : ExtractCN(subject);

                    table.Rows.Add(
                        displayName,
                        issuer.Length > 60 ? issuer.Substring(0, 57) + "..." : issuer,
                        expiry,
                        locationLabel);
                }
            }

            certGrid.DataSource = table;

            if (certGrid.Columns.Count >= 4)
            {
                certGrid.Columns[0].FillWeight = 180;
                certGrid.Columns[1].FillWeight = 180;
                certGrid.Columns[2].FillWeight = 70;
                certGrid.Columns[3].FillWeight = 40;
            }

            countLabel.Text = $"{certGrid.Rows.Count} trusted root certificate authorities found";
        }

        private static string ExtractCN(string subject)
        {
            const string cnPrefix = "CN=";
            int cnStart = subject.IndexOf(cnPrefix, StringComparison.Ordinal);
            if (cnStart < 0)
            {
                return subject;
            }

            cnStart += cnPrefix.Length;
            int cnEnd = subject.IndexOf(',', cnStart);
            return cnEnd < 0 ? subject.Substring(cnStart) : subject.Substring(cnStart, cnEnd - cnStart);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void infoLabel_Click(object sender, EventArgs e)
        {

        }
    }
}

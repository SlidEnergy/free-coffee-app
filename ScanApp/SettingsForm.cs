using ScanApp.Properties;
using ScanApp.Utils;
using System.Windows.Forms;

namespace ScanApp
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();

            string apiToken = "";

            try
            {
                if(!string.IsNullOrWhiteSpace(Settings.Default.EncryptedApiToken))
                    apiToken = Encrypt.DecryptString(Properties.Settings.Default.EncryptedApiToken, Configuration.SecureKey);
            }
            catch
            {

            }

            baseUrlTextBox.Text = Settings.Default.BaseUrl;
            checkPointsEndpointTextBox.Text = Settings.Default.CheckPointsEndPoint;
            consumeEndpointTextBox.Text = Settings.Default.ConsumeEndpoint;
            ScannerInputTimeoutNumericUpDown.Value = Settings.Default.ScannerInputTimeoutInMilliseconds;
            apiTokenTextBox.Text = apiToken;
        }

        private void saveButton_Click(object sender, System.EventArgs e)
        {
            if (!ValidateInput())
                return;

            Settings.Default.BaseUrl = baseUrlTextBox.Text.Trim();
            Settings.Default.CheckPointsEndPoint = checkPointsEndpointTextBox.Text.Trim();
            Settings.Default.ConsumeEndpoint = consumeEndpointTextBox.Text.Trim();
            Settings.Default.ScannerInputTimeoutInMilliseconds = (int)ScannerInputTimeoutNumericUpDown.Value;

            try
            {
                Settings.Default.EncryptedApiToken = Encrypt.EncryptString(apiTokenTextBox.Text.Trim(), Configuration.SecureKey);
            }
            catch
            {
                Settings.Default.EncryptedApiToken = "";

                MessageBox.Show(this, "Can't encrypt apiToken and save it. Please call to administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Settings.Default.Save();

            DialogResult = DialogResult.OK;

            Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(baseUrlTextBox.Text.Trim()))
            {
                MessageBox.Show(this, "Base url not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(checkPointsEndpointTextBox.Text.Trim()))
            {
                MessageBox.Show(this, "Check points endpoint not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(consumeEndpointTextBox.Text.Trim()))
            {
                MessageBox.Show(this, "Consume endpoint not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(apiTokenTextBox.Text.Trim()))
            {
                MessageBox.Show(this, "Api token not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }
}

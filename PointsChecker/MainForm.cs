using Newtonsoft.Json;
using PointsChecker.Properties;
using PointsChecker.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static PointsChecker.KeyEventUtility;
using Timer = System.Threading.Timer;

namespace PointsChecker
{
    public partial class MainForm : Form
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private KeyboardHook hook;
        private StringBuilder keyDownBuffer = new StringBuilder();
        private StringBuilder debugBuffer = new StringBuilder();
        private bool lAltNumPadComboMode = false;
        public StringBuilder lAltNumPadComboModeBuffer = new StringBuilder(3);
        private Timer timer;
        private BarcodeScannerProvider barcodeScannerProvider;
        private const string PLEASE_SCAN_MESSAGE = "Please scan QR code.";
        private IProductsService productService;
        private string userId = null;
        private FormWindowState prevWindowState = FormWindowState.Maximized;
        private bool isLoading = false;
        private Configuration configuration;

        public MainForm()
        {
            InitializeComponent();

            UpdateConfiguration();

            userIdLabel.Text = PLEASE_SCAN_MESSAGE;
            barcodeScannerProvider = new BarcodeScannerProvider(configuration);
            productService = new ProductsService(configuration);

            HideInTaskBar();

            Logger.Debug("MainForm {0}", JsonConvert.SerializeObject(configuration));
        }

        private void UpdateConfiguration()
        {
            string apiToken = "";

            try
            {
                if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.EncryptedApiToken))
                    apiToken = Encrypt.DecryptString(Properties.Settings.Default.EncryptedApiToken, Configuration.SecureKey);
            }
            catch
            {
                Logger.Error("Can't decrypt api token");
            }

            configuration = new Configuration()
            {
                CheckPointsEndPoint = Properties.Settings.Default.CheckPointsEndPoint,
                ApiToken = apiToken,
                BaseUrl = Properties.Settings.Default.BaseUrl,
                ConsumeEndpoint = Properties.Settings.Default.ConsumeEndpoint,
                ScannerInputTimeoutInMilliseconds = Properties.Settings.Default.ScannerInputTimeoutInMilliseconds,
                UserCodePrefix = Properties.Settings.Default.UserIdPrefix
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hook = new KeyboardHook(true);
            hook.KeyDown += Hook_KeyDown;
        }

        public delegate void OnTimerElapsedDelegate(object state);

        private void OnTimerElapsed(object state)
        {
            Logger.Debug("timer elapsed");
            ClearTimer();

            this.Invoke((Action)CaptureScanCodeWithUserId);
        }

        private async void CaptureScanCodeWithUserId()
        {
            try
            {
                Logger.Debug("Hook_KeyDown keyDownBuffer {0}", keyDownBuffer);
                var userId = barcodeScannerProvider.MatchAndGetUserIdFromQRCode(keyDownBuffer.ToString());
                Logger.Debug("Hook_KeyDown capture userId {0}", userId);
                if (string.IsNullOrEmpty(userId))
                    return;

                CloseSessionWithUserQRCode();

                StartSessionWithUserQRCode(userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Thrown error while scan QR code." + Environment.NewLine + Environment.NewLine + ex.Message,
                "Error", MessageBoxButtons.OK);
            }

            try
            {
                ShowLoadingIndicator();

                var products = await productService.GetFreeProductsAsync(userId);

                ShowProducts(products);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(this, "Your request forbidden, perhapse your authorization token expired or blocked. Call to administrator." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HideInTaskBar();
            }
            catch (ApiErrorException ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HideInTaskBar();
            }
            catch (UnhandledApiErrorException ex)
            {
                MessageBox.Show(this, "Trown error while send request." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HideInTaskBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown unhandled error while send request. Please try later or call to administrator." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Unhandled error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                HideInTaskBar();
            }
            finally
            {
                HideLoadingIndicator();
            }
        }

        private void ShowUserId(string userId)
        {
            userIdLabel.Text = userId ?? "";
        }

        private void ShowProducts(List<Product> products)
        {
            productListBox.DisplayMember = nameof(Product.Name);
            productListBox.DataSource = products ?? default(List<Product>);

            UpdateOrderButtonState();
        }

        public void StartLAltNumPadComboMode()
        {
            Logger.Debug("Start lAlt + numpad combo mode");
            lAltNumPadComboMode = true;
            lAltNumPadComboModeBuffer.Clear();
        }

        public void FinishLAltNumPadComboMode()
        {
            lAltNumPadComboMode = false;
            lAltNumPadComboModeBuffer.Clear();
            Logger.Debug("Successful finished Lalt + numpad combo mode");
        }

        public void ParseLAltNumPadCombo()
        {
            Logger.Debug($"Convert to LAlt+numpad combo to symbol. Buffer has {lAltNumPadComboModeBuffer} symbols");
            try
            {
                int code = Convert.ToInt32(lAltNumPadComboModeBuffer.ToString());
                var charkey = Convert.ToChar(code);

                debugBuffer.Append(charkey);
                keyDownBuffer.Append(charkey);

                FinishLAltNumPadComboMode();
            }
            catch
            {
                Logger.Debug("Can't convert LAlt+numpad sentense to symbol.");
            }
        }

        private bool Hook_KeyDown(int wParam, KeyboardHookData lParam)
        {
            try
            {
                if (timer == null)
                {
                    Logger.Debug("Hook_KeyDown timer is null, clear timer");
                    // Clear data and capacity
                    debugBuffer = new StringBuilder();
                    keyDownBuffer = new StringBuilder();
                }

                if (wParam == KeyboardHook.WM_SYSKEYDOWN)
                {
                    if((lParam.vkCode & KeyboardHook.VK_LALT) == KeyboardHook.VK_LALT)
                    {
                        StartLAltNumPadComboMode();
                        debugBuffer.Append("LAlt");
                    }
                    else if (lAltNumPadComboMode == true)
                    {
                        var numberCharKey = UsKeyboardScanCodes.GetCharByNumPadScanCode(lParam.scanCode);

                        lAltNumPadComboModeBuffer.Append(numberCharKey);
                        debugBuffer.Append(numberCharKey);

                        if (lAltNumPadComboModeBuffer.Length >= 3)
                        {
                            ParseLAltNumPadCombo();
                        }
                    }
                }
                else
                {
                    if (lParam.vkCode == KeyboardHook.VK_BACK)
                    {
                        // backspace do nothing
                        debugBuffer.Append("Back");
                    }
                    else if (lParam.vkCode == KeyboardHook.VK_ENTER && keyDownBuffer.Length > Settings.Default.UserIdPrefix.Length)
                    {
                        debugBuffer.Append("Enter");
                        var userId = barcodeScannerProvider.MatchAndGetUserIdFromQRCode(keyDownBuffer.ToString());

                        if (userId != null)
                        {
                            Logger.Debug("Block enter");
                            Logger.Debug("Hook_KeyDown keyDownBuffer {0}", debugBuffer);
                            return true;
                        }
                    }
                    else
                    {
                        var charkey = lParam.vkCode == KeyboardHook.VK_PACKAGE ? Convert.ToChar(lParam.scanCode) : KeyEventUtility.GetCharFromKey(lParam);

                        Logger.Debug("Hook_KeyDown charkey: {0}", charkey);

                        keyDownBuffer.Append(charkey);
                        debugBuffer.Append(charkey);
                    }
                }

                Logger.Debug("Hook_KeyDown keyDownBuffer {0}", debugBuffer);

                ClearTimer();

                timer = new Timer(OnTimerElapsed, null, configuration.ScannerInputTimeoutInMilliseconds, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown error while hook key down. Restart app please." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void ClearTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ClearTimer();

            if (hook != null)
            {
                hook.KeyDown -= Hook_KeyDown;
            }
        }

        private void productListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateOrderButtonState();
        }

        private void UpdateOrderButtonState()
        {
            if (GetSelectedProduct() == null || isLoading)
            {
                orderButton.Enabled = false;
            }
            else
                orderButton.Enabled = true;
        }

        private Product GetSelectedProduct()
        {
            return (Product)productListBox.SelectedItem;
        }

        private async void orderButton_Click(object sender, EventArgs e)
        {
            await ConsumePoints();
        }

        private async Task ConsumePoints()
        {
            if (MessageBox.Show(this, "Do you want to finish transaction and consume points?", "Confirm transaction", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                ShowLoadingIndicator();

                var product = GetSelectedProduct();

                if (product == null)
                {
                    MessageBox.Show(this, "Please choose product.", "Info", MessageBoxButtons.OK);
                    return;
                }

                var successMessage = await productService.OrderProductAsync(product, userId);

                Logger.Debug("ConsumePoints success {0}", successMessage ?? "Product has been ordered.");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(this, "Your request forbidden, perhapse your authorization token expired or blocked. Call to administrator." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ApiErrorException ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnhandledApiErrorException ex)
            {
                MessageBox.Show(this, "Trown error while send request." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown unhandled error while send request. Please try later or call to administrator." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Unhandled error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                HideLoadingIndicator();
                CloseSessionWithUserQRCode();
                HideInTaskBar();
            }
        }

        private void StartSessionWithUserQRCode(string userId)
        {
            Logger.Debug("StartSessionWithUserQRCode");
            this.userId = userId;
            ShowUserId(userId);
            ShowMainWindow();
        }


        private void CloseSessionWithUserQRCode()
        {
            Logger.Debug("CloseSessionWithUserQRCode");
            ShowProducts(null);
            ShowUserId(null);
            userId = null;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            WindowState = prevWindowState;
            ShowInTaskbar = true;
            Activate();
            Focus();
        }

        private void HideInTaskBar()
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            //notifyIcon1.BalloonTipText = "Application Minimized.";
            //notifyIcon1.BalloonTipTitle = "Free coffee";
            //notifyIcon1.ShowBalloonTip(1000);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                HideInTaskBar();

            }
            else
            {
                prevWindowState = WindowState;
                if (isLoading)
                {
                    ShowLoadingIndicator();
                }
            }
        }

        private async void productListBox_DoubleClick(object sender, EventArgs e)
        {
            await ConsumePoints();
        }

        private void productListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 40;
        }

        private void productListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var products = (List<Product>)productListBox.DataSource;

            if (products == null || products.Count == 0)
                return;

            var product = products[e.Index];

            e.DrawBackground();

            Brush backBrush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? new SolidBrush(BrandPallete.RedColor) : new SolidBrush(e.BackColor);
            Brush foreBrush = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Brushes.White : Brushes.Black;

            e.Graphics.FillRectangle(backBrush, e.Bounds);

            var textBounds = new Rectangle(e.Bounds.X, e.Bounds.Y + 10, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawString(product.Name, e.Font, foreBrush, textBounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            //e.DrawFocusRectangle();
        }

        private void ShowLoadingIndicator()
        {
            isLoading = true;

            loadingPanel.Location = new Point(productListBox.Width / 2 - loadingPanel.Width / 2, productListBox.Height / 2 - loadingPanel.Height / 2);

            loadingPanel.Visible = true;
            UpdateOrderButtonState();
            productListBox.Enabled = false;
        }

        private void HideLoadingIndicator()
        {
            isLoading = false;
            loadingPanel.Visible = false;
            UpdateOrderButtonState();
            productListBox.Enabled = true;
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    UpdateConfiguration();

                    productService = new ProductsService(configuration);
                }

                productListBox.Focus();
            }
        }
    }
}

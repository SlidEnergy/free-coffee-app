using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Timer = System.Threading.Timer;

namespace ScanApp
{
    public partial class Form1 : Form
    {
        private KeyboardHook hook;
        private StringBuilder keyDownBuffer = new StringBuilder();
        private Timer timer;
        private BarcodeScannerProvider barcodeScannerProvider;
        private const string PLEASE_SCAN_MESSAGE = "Please scan QR code.";
        private IProductsService productService;
        private string userId = null;
        private Configuration configuration;

        public Form1()
        {
            InitializeComponent();

            userIdLabel.Text = PLEASE_SCAN_MESSAGE;
            barcodeScannerProvider = new BarcodeScannerProvider();
            configuration = new Configuration();
            productService = new ProductsService(configuration);

            HideInTaskBar();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hook = new KeyboardHook(true);
            hook.KeyDown += Hook_KeyDown;
        }

        public delegate void OnTimerElapsedDelegate(object state);

        private void OnTimerElapsed(object state)
        {
            ClearTimer();

            this.Invoke((Action)CaptureScanCodeWithUserId);
        }

        private async void CaptureScanCodeWithUserId()
        {
            try
            {
                var userId = barcodeScannerProvider.MatchAndGetUserIdFromQRCode(keyDownBuffer.ToString());

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
                var products = await productService.GetFreeProductsAsync(userId);

                ShowProducts(products);
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

        private void Hook_KeyDown(int wParam, KeyboardHookData lParam)
        {
            try
            {
                var charkey = KeyEventUtility.GetCharFromKey(KeyInterop.KeyFromVirtualKey(lParam.vkCode)).ToString();

                if (timer == null)
                {
                    // Clear data and capacity
                    keyDownBuffer = new StringBuilder();
                }

                keyDownBuffer.Append(charkey);

                ClearTimer();

                timer = new Timer(OnTimerElapsed, null, configuration.ScannerTypeOneCharTimeoutInMilliseconds, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown error while hook key down. Restart app please." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (GetSelectedProduct() == null)
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
            try
            {
                var product = GetSelectedProduct();

                if (product == null)
                {
                    MessageBox.Show(this, "Please choose product.", "Info", MessageBoxButtons.OK);
                    return;
                }

                var successMessage = await productService.OrderProductAsync(product, userId);

                MessageBox.Show(this, successMessage ?? "Product has been ordered.", "Success", MessageBoxButtons.OK);
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
                CloseSessionWithUserQRCode();
                HideInTaskBar();
            }
        }

        private void StartSessionWithUserQRCode(string userId)
        {
            this.userId = userId;
            ShowUserId(userId);
            ShowMainWindow();
        }


        private void CloseSessionWithUserQRCode()
        {
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
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
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
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
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
        private FormWindowState prevWindowState = FormWindowState.Maximized;
        private bool isLoading = false;

        public Form1()
        {
            InitializeComponent();

            userIdLabel.Text = PLEASE_SCAN_MESSAGE;
            barcodeScannerProvider = new BarcodeScannerProvider();
            configuration = new Configuration();
            productService = new ProductsService(configuration);



            //HideInTaskBar();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            hook = new KeyboardHook(true);
            hook.KeyDown += Hook_KeyDown;

            ShowLoadingIndicator();
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
                ShowLoadingIndicator();

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
                HideLoadingIndicator();
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
            WindowState = prevWindowState;
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
            e.Graphics.DrawString(product.Name, e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
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
            productListBox.Enabled = false;
        }
    }
}

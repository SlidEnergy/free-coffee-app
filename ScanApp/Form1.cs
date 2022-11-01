using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Timer = System.Threading.Timer;

namespace ScanApp
{
    public partial class Form1 : Form
    {
        KeyboardHook hook;
        StringBuilder keyDownBuffer = new StringBuilder();
        Timer timer;
        BarcodeScannerProvider barcodeScannerProvider;
        private const string PLEASE_SCAN_MESSAGE = "Please scan QR code.";
        IProductsService productService;

        public Form1()
        {
            InitializeComponent();

            userIdLabel.Text = PLEASE_SCAN_MESSAGE;
            barcodeScannerProvider = new BarcodeScannerProvider();
            productService = new ProductsService();
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

                ShowUserId(userId);

                var products = await productService.GetFreeProductsAsync(userId);

                ShowProducts(products);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown error while hook key down. Restart app please." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK);
            }
        }

        private void ShowUserId(string userId)
        {
            userIdLabel.Text = userId ?? "";
        }

        private void ShowProducts(List<Product> products)
        {
            productListBox.Items.Clear();

            productListBox.DataSource = products;

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

                timer = new Timer(OnTimerElapsed, null, 1000, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Trown error while hook key down. Restart app please." + Environment.NewLine + Environment.NewLine + ex.Message,
                    "Error", MessageBoxButtons.OK);
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

        private async  void orderButton_Click(object sender, EventArgs e)
        {
            try
            {
                var product = GetSelectedProduct();

                if (product == null)
                {
                    MessageBox.Show(this, "Please choose product.", "Info", MessageBoxButtons.OK);
                    return;
                }

                var result = await productService.OrderProductAsync(product);

                if (!string.IsNullOrEmpty(result))
                {
                    MessageBox.Show(this, result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else 
                {
                    MessageBox.Show(this, "Product has been ordered.", "Success", MessageBoxButtons.OK);
                }

                ShowProducts(null);
                ShowUserId(null);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "Trown error while order product. Please try later or call to administrator." + Environment.NewLine + Environment.NewLine + ex.Message,
                   "Error", MessageBoxButtons.OK);
            }
        }
    }
}

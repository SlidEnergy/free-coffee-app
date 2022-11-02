using ScanApp;

namespace UnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void MatchAndGetUserIdFromQRCode_Should()
        {
            var barcodeScannerProvider = new BarcodeScannerProvider();

            var result = barcodeScannerProvider.MatchAndGetUserIdFromQRCode("othertextscanapp-1234othertext");

            Assert.IsNotNull(result);

            Assert.AreEqual("1234", result);
        }
    }
}
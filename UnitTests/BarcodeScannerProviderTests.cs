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
        public void Test1()
        {
            var barcodeScannerProvider = new BarcodeScannerProvider();

            var result = barcodeScannerProvider.MatchAndGetUserIdFromQRCode("othertextscanapp-sdkj324othertext");

            Assert.IsNotNull(result);

            Assert.AreEqual("sdkj324", result);
        }
    }
}
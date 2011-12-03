using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MethodReturnValueTest
    {
        interface ITest
        {
            int GetInt();

            string GetString();
        }

        [Test]
        public void TestParameterlessIntMethodCall()
        {
            var test = new Mirror<ITest>();

            test.Arrange(s => s.GetInt()).Returns(987);

            Assert.AreEqual(987, test.It.GetInt());
        }


        [Test]
        public void TestParameterlessStringMethodCall()
        {
            var test = new Mirror<ITest>();

            test.Arrange(s => s.GetString()).Returns("234");

            Assert.AreEqual("234", test.It.GetString());
        }
    }
}

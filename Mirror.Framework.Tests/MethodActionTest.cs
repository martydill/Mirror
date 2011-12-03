using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MethodActionTest
    {
        interface ITest
        {
            void DoStuff();
        }

        [Test]
        public void TestMethodArrangeWithCalls()
        {
            var test = new Mirror<ITest>();
            int counter = 0;
            test.Arrange(s => s.DoStuff()).Calls(() => ++counter);

            test.It.DoStuff();
            test.It.DoStuff();
            test.It.DoStuff();
            Assert.AreEqual(3, counter);
        }
    }
}

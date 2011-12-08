using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MirrorCallsTest
    {
        interface ITest
        {
            void DoStuff();
        }


        [Test]
        public void TestCallsWithNullLambdaThrowsException()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<MirrorArrangeException>(() => test.Calls(s => s.DoStuff(), null));
        }


        [Test]
        public void TestCallsWithValidLambdaCallsLambda()
        {
            var test = new Mirror<ITest>();
            int counter = 0;
            test.Calls(s => s.DoStuff(), () => ++counter);

            test.It.DoStuff();
            test.It.DoStuff();
            test.It.DoStuff();
            Assert.AreEqual(3, counter);
        }
    }
}

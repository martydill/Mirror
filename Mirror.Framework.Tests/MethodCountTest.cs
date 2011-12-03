using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    class MethodCountTest
    {
        interface ITest
        {
            void DoStuff();
            void DoStuff(int i);
        }

        [Test]
        public void TestCountReturnsCorrectValueForNoParameterMethod()
        {
            var mock = new Mirror<ITest>();
            mock.It.DoStuff();
            mock.It.DoStuff();
            mock.It.DoStuff();

            Assert.AreEqual(3, mock.Count(s => s.DoStuff()));
        }
    }
}

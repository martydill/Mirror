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
            mock.Arrange(m => m.DoStuff());
            mock.It.DoStuff();
            mock.It.DoStuff();
            mock.It.DoStuff();

            Assert.AreEqual(3, mock.Count(s => s.DoStuff()));
        }


        [Test]
        public void TestCountReturnsCorrectValueForSingleParameterMethod()
        {
            var mock = new Mirror<ITest>();
            mock.Arrange(m => m.DoStuff(1));
            mock.It.DoStuff(1);
            mock.It.DoStuff(2);
            mock.It.DoStuff(1);

            Assert.AreEqual(2, mock.Count(s => s.DoStuff(1)));
        }
    }
}

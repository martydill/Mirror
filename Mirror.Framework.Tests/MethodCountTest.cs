using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    class MethodCountTest
    {
        class test
        {
        }

        interface ITest
        {
            void DoStuff();
            void DoStuff(int i);
            void DoStuff(test t, string s);
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


        [Test]
        public void TestCountReturnsCorrectValuesForMultiParameterMethod()
        {
            var t = new test();
            var mock = new Mirror<ITest>();
            mock.Arrange(m => m.DoStuff(t, "a"));
            mock.It.DoStuff(t, "a");
            mock.It.DoStuff(t, "b");
            mock.It.DoStuff(null, "a");
            mock.It.DoStuff(t, "a");
            mock.It.DoStuff(null, "a");

            Assert.AreEqual(2, mock.Count(s => s.DoStuff(t, "a")));
            Assert.AreEqual(1, mock.Count(s => s.DoStuff(t, "b")));
            Assert.AreEqual(2, mock.Count(s => s.DoStuff(null, "a")));
        }

    }
}

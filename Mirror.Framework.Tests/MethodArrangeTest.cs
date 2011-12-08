using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MethodArrangeTest
    {
        interface ITest
        {
            void DoStuff();

            int GetInt();

            string GetString();

            int GetInt(int value);

            string GetString(int p1, string p2);
        }

        [Test]
        public void TestCallNonArrangedMethodDoesNothing()
        {
            var test = new Mirror<ITest>();
            test.It.DoStuff();
        }

        [Test]
        public void TestCallNonArrangedMethodWithReturnValueReturnsDefault()
        {
            var test = new Mirror<ITest>();
            Assert.AreEqual(0, test.It.GetInt(3));
        }

        [Test]
        public void TestParameterlessIntMethodCall()
        {
            var test = new Mirror<ITest>();

            test.Returns(s => s.GetInt(), 987);

            Assert.AreEqual(987, test.It.GetInt());
        }


        [Test]
        public void TestParameterlessStringMethodCall()
        {
            var test = new Mirror<ITest>();

            test.Returns(s => s.GetString(), "234");

            Assert.AreEqual("234", test.It.GetString());
        }


        [Test]
        public void TestOneParameterMethodArranging()
        {
            var test = new Mirror<ITest>();

            test.Returns(s => s.GetInt(123), 111);
            test.Returns(s => s.GetInt(456), 222);

            Assert.AreEqual(111, test.It.GetInt(123));
            Assert.AreEqual(222, test.It.GetInt(456));
        }

        [Test]
        public void TestTwoParameterMethodArranging()
        {
            var test = new Mirror<ITest>();

            test.Returns(s => s.GetString(123, "a"), "123a");
            test.Returns(s => s.GetString(123, "b"), "123b");
            test.Returns(s => s.GetString(456, "a"), "456a");

            Assert.AreEqual("123a", test.It.GetString(123, "a"));
            Assert.AreEqual("123b", test.It.GetString(123, "b"));
            Assert.AreEqual("456a", test.It.GetString(456, "a"));
        }

        [Test]
        public void TestMethodParameterMethodArranging()
        {
            var test = new Mirror<ITest>();

            test.Returns(s => s.GetInt(IntReturner()), 999);

            Assert.AreEqual(999, test.It.GetInt(IntReturner()));
        }

  
        private int IntReturner()
        {
            return 12345;
        }
    }
}

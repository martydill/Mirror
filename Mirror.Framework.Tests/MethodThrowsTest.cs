using System;
using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MethodThrowsTest
    {
        interface ITest
        {
            void DoStuff();
        }

        [Test]
        public void TestMethodArrangeWithThrowsThrowsExceptionIfParameterIsNull()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<MirrorArrangeException>(() => test.Arrange(s => s.DoStuff()).Throws(null));
        }

        [Test]
        public void TestMethodArrangeWithThrows()
        {
            var test = new Mirror<ITest>();
            test.Arrange(s => s.DoStuff()).Throws(new Exception());

            Assert.Throws<Exception>(() => test.It.DoStuff());
        }
    }
}

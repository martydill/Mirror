using System;
using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MirrorThrowsTest
    {
        interface ITest
        {
            void DoStuff();
        }

        [Test]
        public void TestThrowsThrowsExceptionIfExceptionIsNull()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<MirrorArrangeException>(() => test.Throws(s => s.DoStuff(), null));
        }

        [Test]
        public void TestMethodArrangeWithThrows()
        {
            var test = new Mirror<ITest>();
            test.Throws(s => s.DoStuff(), new Exception());

            Assert.Throws<Exception>(() => test.It.DoStuff());
        }
    }
}

// Copyright 2011 Marty Dill
// See License.txt for details

using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    class MirrorTest
    {
        interface ITest
        {
        }

        [Test]
        public void TestItPropertyReturnsAValidObject()
        {
            var mock = new Mirror<ITest>();
            var it = mock.It;

            Assert.IsNotNull(it);
        }

        [Test]
        public void TestItPropertyAlwaysReturnsSameObject()
        {
            var mock = new Mirror<ITest>();
            var it1 = mock.It;
            var it2 = mock.It;

            Assert.AreSame(it1, it2);
        }
    }
}

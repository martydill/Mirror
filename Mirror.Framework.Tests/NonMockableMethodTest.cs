using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MockCreationTest
    {
        class ConcreteClass
        {
            public void DoStuff()
            {
            }
        }

        class MarshalByRefConcreteClass : MarshalByRefObject
        {
        }
        
        [Test]
        public void TestMockNonVirtualMethodThrowsException()
        {
            Assert.Throws<MirrorCreationException>(() => new Mirror<ConcreteClass>());
        }

        [Test]
        public void TestMockMarshalByRefClassDoesNotThrowException()
        {
            Assert.DoesNotThrow(() => new Mirror<MarshalByRefConcreteClass>());
        }
    }
}

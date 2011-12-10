// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MirrorCreationTest
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

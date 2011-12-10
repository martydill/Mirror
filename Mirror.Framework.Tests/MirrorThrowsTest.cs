// Copyright 2011 Marty Dill
// See License.txt for details

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

            void DoStuff(int i);

            string Name { get; }
        }

        [Test]
        public void TestThrowsThrowsExceptionIfLambdaIsNull()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<ArgumentNullException>(() => test.Throws(null, new Exception()));
        }

        [Test]
        public void TestThrowsThrowsExceptionIfExceptionIsNull()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<ArgumentNullException>(() => test.Throws(s => s.DoStuff(), null));
        }

        [Test]
        public void TestThrowsWithNoParameterMethodThrows()
        {
            var test = new Mirror<ITest>();
            test.Throws(s => s.DoStuff(), new Exception());

            Assert.Throws<Exception>(() => test.It.DoStuff());
        }

        [Test]
        public void TestThrowsWithParametersThrowsWhenParametersMatch()
        {
            var test = new Mirror<ITest>();
            test.Throws(s => s.DoStuff(5), new Exception());

            Assert.DoesNotThrow(() => test.It.DoStuff());
            Assert.DoesNotThrow(() => test.It.DoStuff(1));
            Assert.Throws<Exception>(() => test.It.DoStuff(5));
            Assert.DoesNotThrow(() => test.It.DoStuff(55));
        }

        [Test]
        public void TestThrowsForPropertyThrowsException()
        {
            var test = new Mirror<ITest>();
            test.Throws(s => s.Name, new ArgumentException());

            Assert.Throws<ArgumentException>(() => { var n = test.It.Name; });
        }


        [Test]
        public void TestThrowsWithAnyParameterThrowsWhenParametersMatch()
        {
            var test = new Mirror<ITest>();
            test.Throws(s => s.DoStuff(Any<int>.Value), new Exception());
            
            Assert.Throws<Exception>(() => test.It.DoStuff(1));
            Assert.Throws<Exception>(() => test.It.DoStuff(5));
            Assert.Throws<Exception>(() => test.It.DoStuff(55));
        }
    }
}

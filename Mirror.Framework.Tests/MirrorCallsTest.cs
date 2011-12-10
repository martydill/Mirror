// Copyright 2011 Marty Dill
// See License.txt for details

using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MirrorCallsTest
    {
        interface ITest
        {
            void DoStuff();

            void DoStuff(int i);

            string Name { get; set;  }
        }


        [Test]
        public void TestCallsWithNullLambdaThrowsException()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<MirrorArrangeException>(() => test.Calls(s => s.DoStuff(), null));
        }


        [Test]
        public void TestCallsWithValidLambdaCallsLambda()
        {
            var test = new Mirror<ITest>();
            int counter = 0;
            test.Calls(s => s.DoStuff(), () => ++counter);

            test.It.DoStuff();
            test.It.DoStuff();
            test.It.DoStuff();
            Assert.AreEqual(3, counter);
        }


        [Test]
        public void TestCallsForPropertyGetter()
        {
            var test = new Mirror<ITest>();

            bool called = false;
            test.Calls(t => t.Name, () => called = true);

            string s = test.It.Name;
            Assert.IsTrue(called);
        }


        [Test]
        public void TestCallsForAnyParameter()
        {
            var test = new Mirror<ITest>();
            int counter = 0;
            test.Calls(s => s.DoStuff(Any<int>.Value), () => ++counter);

            test.It.DoStuff(1);
            test.It.DoStuff(234234);
            test.It.DoStuff(55);
            Assert.AreEqual(3, counter);
        }
    }
}

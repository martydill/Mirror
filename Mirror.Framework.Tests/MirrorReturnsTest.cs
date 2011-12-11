// Copyright 2011 Marty Dill
// See License.txt for details

using System;
using Mirror.Framework;
using NUnit.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class MirrorReturnsTest
    {
        interface ITest
        {
            int Number { get; set; }

            string Name { get; }

            float Value { set; }

            void DoStuff();

            int GetInt();

            string GetString();

            int GetInt(int value);

            string GetString(int p1, string p2);
        }


        [Test]
        public void TestCallNonArrangedMethodDoesNothingReturnsDefaultValueForReferenceType()
        {
            var test = new Mirror<ITest>();
            string s = test.It.GetString(1, "a");
            Assert.IsNull(s);
        }


        [Test]
        public void TestCallNonArrangedMethodDoesNothingReturnsDefaultValueForValueType()
        {
            var test = new Mirror<ITest>();
            int result = test.It.GetInt(1);
            Assert.AreEqual(0, result);
        }


        [Test]
        public void TestReturnsNullLambdaThrowsException()
        {
            var test = new Mirror<ITest>();
            Assert.Throws<ArgumentNullException>(() => test.Returns(null, 5));
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


        [Test]
        public void TestCallNonArrangedPropertyDoesNothing()
        {
            var test = new Mirror<ITest>();
            test.It.Number = 7;
        }


        [Test]
        public void TestReturnValueForGetter()
        {
            var test = new Mirror<ITest>();

            test.Returns(t => t.Name, "abc");

            Assert.AreEqual("abc", test.It.Name);
        }


        private int IntReturner()
        {
            return 12345;
        }



        [Test]
        public void TestReturnValueForMethodWithAnyParameters()
        {
            var test = new Mirror<ITest>();

            test.Returns(t => t.GetInt(Any<int>.Value), 5);

            Assert.AreEqual(5, test.It.GetInt(999));
            Assert.AreEqual(5, test.It.GetInt(1));
        }

        [Test]
        public void TestReturnValueForMethodWithSomeAnyParameters()
        {
            var test = new Mirror<ITest>();

            test.Returns(t => t.GetString(1 ,Any<string>.Value), "a");
            test.Returns(t => t.GetString(2, Any<string>.Value), "b");

            Assert.AreEqual("a", test.It.GetString(1, "asdf"));
            Assert.AreEqual("b", test.It.GetString(2, "abcd"));
        }
    }
}

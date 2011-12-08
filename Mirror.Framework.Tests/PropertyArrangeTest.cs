using NUnit.Framework;
using Mirror.Framework;

namespace Mirror.Tests
{
    [TestFixture]
    public class PropertyArrangeTest
    {
        interface ITest
        {
            int Number { get; set; }

            string Name { get; }

            float Value { set; }
        }

        [Test]
        public void TestCallNonArrangedPropertyDoesNothing()
        {
            var test = new Mirror<ITest>();
            test.It.Number = 7;
        }

        [Test]
        public void TestArrangeGetter()
        {
            var test = new Mirror<ITest>();

            test.Returns(t => t.Name, "abc");

            Assert.AreEqual("abc", test.It.Name);
        }
    }
}

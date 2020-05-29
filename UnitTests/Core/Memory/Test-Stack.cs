using System;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Core.Memory.Helpers;

namespace UnitTests.Core.Memory
{
    [TestClass]
    public class TestStack
        : TestMemoryBase
    {
        public TestStack()
        {
        }

        [TestMethod]
        public void TestStackIntRoundTrip()
        {
            const byte input = (byte)10;

            Vm.Memory
                .StackPushInt(input);

            Assert.IsTrue(input == Vm.Memory.StackPopInt());
        }

        [TestMethod]
        public void TestStackMultiplePop()
        {
            for (var i = 0; i < 10; i++)
            {
                Vm.Memory.StackPushInt(i);
            }

            for (var j = 9; j >= 0; j--)
            {
                Assert.IsTrue(j == Vm.Memory.StackPopInt());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestStackPopEmpty()
        {
            _ = Vm.Memory.StackPopInt();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestStackPopExceedCapacity()
        {
            for (var i = 0; i < 10; i++)
            {
                Vm.Memory.StackPushInt(i);
            }

            for (var j = 10; j >= 0; j--)
            {
                _ = Vm.Memory.StackPopInt();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(StackOutOfRangeException))]
        public void TestStackPushExceedCapacity()
        {
            for (var i = 0; i < 101; i++)
            {
                Vm.Memory.StackPushInt(i);
            }
        }
    }
}

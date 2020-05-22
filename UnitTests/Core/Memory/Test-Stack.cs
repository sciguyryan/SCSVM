using System;
using VMCore;
using VMCore.VM;
using VMCore.VM.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Memory
{
    [TestClass]
    public class Test_Stack
        : Test_Memory_Base
    {
        public Test_Stack()
        {
        }

        [TestMethod]
        public void TestStackIntRoundTrip()
        {
            var input = (byte)10;

            _vm.Memory
                .StackPushInt(input);

            Assert.IsTrue(input == _vm.Memory.StackPopInt());
        }

        [TestMethod]
        public void TestStackMultiplePop()
        {
            for (var i = 0; i < 10; i++)
            {
                _vm.Memory.StackPushInt(i);
            }

            for (var j = 9; j >= 0; j--)
            {
                Assert.IsTrue(j == _vm.Memory.StackPopInt());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestStackPopEmpty()
        {
            _ = _vm.Memory.StackPopInt();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestStackPopExceedCapacity()
        {
            for (var i = 0; i < 10; i++)
            {
                _vm.Memory.StackPushInt(i);
            }

            for (var j = 10; j >= 0; j--)
            {
                _ = _vm.Memory.StackPopInt();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(StackOutOfRangeException))]
        public void TestStackPushExceedCapacity()
        {
            for (var i = 0; i < 101; i++)
            {
                _vm.Memory.StackPushInt(i);
            }
        }
    }
}

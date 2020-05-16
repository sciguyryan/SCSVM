using System;
using VMCore;
using VMCore.VM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace UnitTests.Memory
{
    [TestClass]
    public class Test_Memory_Type_IO
        : Test_Memory_Base
    {
        public Test_Memory_Type_IO()
        {
        }

        [TestMethod]
        public void TestByteRoundTrip()
        {
            var input = (byte)10;

            _vm.Memory
                .SetValue(0, input, SecurityContext.System, false);

            var output = 
                _vm.Memory.GetValue(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestRegisterRoundTrip()
        {
            var input = Registers.R1;

            _vm.Memory
                .SetRegisterIdent(0, input,SecurityContext.System, false);

            var output =
                _vm.Memory.GetRegisterIdent(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestIntegerRoundTrip()
        {
            var input = 10;

            _vm.Memory
                .SetInt(0, input, SecurityContext.System, false);

            var output =
                _vm.Memory.GetInt(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestOpCodeRoundTrip()
        {
            var input = OpCode.ADD_LIT_REG;

            _vm.Memory
                .SetOpCode(0, input, SecurityContext.System, false);

            var output =
                _vm.Memory.GetOpCode(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestStringRoundTrip()
        {
            var input = "banana";

            var byteLen = 
                sizeof(int) +
                Encoding.UTF8.GetBytes(input).Length;

            _vm.Memory
                .SetString(0, input, SecurityContext.System, false);

            // Tuple argument 1 is the number of bytes
            // taken to write the length of the string (4)
            // and the number of bytes taken to create the
            // string.
            (var outL, var outS) =
                _vm.Memory.GetString(0, SecurityContext.System, false);

            Assert.IsTrue(input == outS);
            Assert.IsTrue(byteLen == outL);
        }
    }
}

using System.Text;
using VMCore.VM.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Core.Memory.Helpers;

namespace UnitTests.Core.Memory
{
    [TestClass]
    public class TestMemoryTypeIo
        : TestMemoryBase
    {
        public TestMemoryTypeIo()
        {
        }

        [TestMethod]
        public void TestByteRoundTrip()
        {
            const byte input = (byte)10;

            Vm.Memory
                .SetValue(0, input, SecurityContext.System, false);

            var output = 
                Vm.Memory.GetValue(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestRegisterRoundTrip()
        {
            const VMCore.VM.Core.Register.Registers input = VMCore.VM.Core.Register.Registers.R1;

            Vm.Memory
                .SetRegisterIdent(0, input,SecurityContext.System, false);

            var output =
                Vm.Memory.GetRegisterIdent(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestIntegerRoundTrip()
        {
            const int input = 10;

            Vm.Memory
                .SetInt(0, input, SecurityContext.System, false);

            var output =
                Vm.Memory.GetInt(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestOpCodeRoundTrip()
        {
            const OpCode input = OpCode.ADD_LIT_REG;

            Vm.Memory
                .SetOpCode(0, input, SecurityContext.System, false);

            var output =
                Vm.Memory.GetOpCode(0, SecurityContext.System, false);

            Assert.IsTrue(input == output);
        }

        [TestMethod]
        public void TestStringRoundTrip()
        {
            const string input = "banana";

            var byteLen = 
                sizeof(int) +
                Encoding.UTF8.GetBytes(input).Length;

            Vm.Memory
                .SetString(0, input, SecurityContext.System, false);

            // Tuple argument 1 is the number of bytes
            // taken to write the length of the string (4)
            // and the number of bytes taken to create the
            // string.
            var (outL, outS) =
                Vm.Memory.GetString(0, SecurityContext.System, false);

            Assert.IsTrue(input == outS);
            Assert.IsTrue(byteLen == outL);
        }
    }
}

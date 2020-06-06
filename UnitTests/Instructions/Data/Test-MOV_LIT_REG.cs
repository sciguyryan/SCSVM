using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;
using VMCore.VM.Core.Register;

namespace UnitTests.Instructions.Data
{
    [TestClass]
    public class TestMovLitReg
        : TestInstructionBase
    {
        public TestMovLitReg()
        {
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run from user-code assembly.
        /// </summary>
        [TestMethod]
        public void TestUserAssemblyWriteToRegister()
        {
            const Registers register = Registers.R1;
            const int expected = 0x123;

            var program = new []
            {
                new CompilerIns(OpCode.MOV_LIT_REG, 
                             new object[] { expected, register }),
            };


            Vm.Run(Utils.QuickRawCompile(program));

            Assert.IsTrue(Vm.Cpu.Registers[register] == expected);
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run directly from code with a user context.
        /// </summary>
        [TestMethod]
        public void TestUserDirectWriteToRegister()
        {
            const Registers register = Registers.R1;
            const int expected = 0x123;

            Vm.Cpu.Registers[(register, SecurityContext.User)] = 
                expected;

            Assert.IsTrue(Vm.Cpu.Registers[register] == expected);
        }

        /// <summary>
        /// Test if writing to a valid register succeeds when
        /// run directly from code with a system context.
        /// </summary>
        [TestMethod]
        public void TestSystemDirectWriteToRegister()
        {
            const Registers register = Registers.R1;
            const int expected = 0x123;

            Vm.Cpu.Registers[(register, SecurityContext.System)] = 
                expected;

            Assert.IsTrue(Vm.Cpu.Registers[register] == expected);
        }
    }
}

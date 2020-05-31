using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Instructions.Helpers;

namespace UnitTests.Instructions.Bit
{
    [TestClass]
    public class TestBit
        : TestInstructionBase
    {
        public TestBit()
        {
        }

        /// <summary>
        /// Test the functionality of a the bit test instruction.
        /// </summary>
        [TestMethod]
        public void TestBitTest()
        {
            var table = new []
            {
                #region TESTS
                // Note that these are little endian so the
                // least significant byte is first and the
                // most significant byte is last.
                // As 32-bit integers are 4 bytes in size that 
                // means that the layout will be like this:
                // byte 4 (last 8 bits),
                // byte 3 (next 8 bits),
                // byte 2 (next 8 bits),
                // byte 1 (next 8 bits).
                // For ease of reference I have added
                // the binary representations to the
                // right of the test case.

                new object[] { 0, 0, true },           // 0b00000000_00000000_00000000_00000000
                new object[] { 0, 4, true },           // 0b00000000_00000000_00000000_00000100
                new object[] { 0, 5, false },          // 0b00000000_00000000_00000000_00000101
                new object[] { 1, 5, true },           // 0b00000000_00000000_00000000_00000101
                new object[] { 1, 6, false },          // 0b00000000_00000000_00000000_00000111
                new object[] { 0, -1, false },         // 0b11111111_11111111_11111111_11111111
                new object[] { 0, 2147473160, true },  // 0b‭01111111_11111111_11010111_00001000
                new object[] { 3, 2147473160, false }, // 0b‭01111111_11111111_11010111_00001000
                new object[] { 13, 2147473160, true }, // 0b‭01111111_11111111_11010111_00001000
                #endregion
            };

            for (var i = 0; i < table.Length; i++)
            {
                var entry = table[i];

                var program = new []
                {
                    new QuickIns(OpCode.MOV_LIT_REG, 
                                new object[] { (int)entry[1], Registers.R1 }),
                    new QuickIns(OpCode.BIT, 
                                new object[] { (int)entry[0], Registers.R1 }),
                };

                Vm.Run(Utils.QuickRawCompile(program));

                var success =
                    Vm.Cpu.IsFlagSet(CpuFlags.Z) == (bool)entry[2];

                Assert.IsTrue(success,
                              $"Zero flag for test {i} is incorrect. " +
                              $"Expected {(bool)entry[2]}, got " +
                              $"{Vm.Cpu.IsFlagSet(CpuFlags.Z)}.");
            }
        }
    }
}

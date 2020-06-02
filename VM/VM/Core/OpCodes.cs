namespace VMCore.VM.Core
{
    public enum OpCode
    {
        /// <summary>
        /// Subroutine - a pseudo-opcode used to identify a
        /// subroutine position.
        /// </summary>
        SUBROUTINE = -2,
        /// <summary>
        /// Label - a pseudo-opcode used to identify a labels position.
        /// </summary>
        LABEL = -1,

        /// <summary>
        /// No Operation - a non-operation instruction.
        /// No action to be performed.
        /// </summary>
        NOP,

        /// <summary>
        /// Move Literal to Register - copy a literal into a register.
        /// </summary>
        MOV_LIT_REG,
        /// <summary>
        /// Move Register to Register - copy the value of register A
        /// into register B.
        /// </summary>
        MOV_REG_REG,
        /// <summary>
        /// Move Register to Memory - copy the value of the register
        /// into memory.
        /// </summary>
        MOV_REG_MEM,
        /// <summary>
        /// Move Register to Memory via Literal Expression - 
        /// copy the value of the register into memory starting from
        /// the index given by an expression into the register.
        /// </summary>
        MOV_REG_LIT_EXP_MEM,
        /// <summary>
        /// Move Memory to Register - copy a value from memory
        /// into the register.
        /// </summary>
        MOV_MEM_REG,
        /// <summary>
        /// Move from Memory via Literal Expression to Register - 
        /// copy a value from memory starting from the index given
        /// by an expression into the register.
        /// </summary>
        MOV_LIT_EXP_MEM_REG,
        /// <summary>
        /// Move Literal to Memory - copy a literal into memory.
        /// </summary>
        MOV_LIT_MEM,
        /// <summary>
        /// Move Register* to Register - 
        /// copy a value from memory into Register B starting
        /// from the index given by the value of register A.
        /// </summary>
        MOV_REG_PTR_REG,
        /// <summary>
        /// Move from Memory via Literal Offset to Register - 
        /// copy a value from memory starting from the index
        /// given by a literal plus the value of register A
        /// into register B.
        /// </summary>
        MOV_LIT_OFF_REG,
        /// <summary>
        /// Swap - swap the value of registers A and B.
        /// </summary>
        SWAP,

        /// <summary>
        /// Add Register to Register - add the contents of
        /// register A into register B.
        /// Result is moved into the accumulator.
        /// </summary>
        ADD_REG_REG,
        /// <summary>
        /// Add Literal to Register - add a literal to the
        /// value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        ADD_LIT_REG,
        /// <summary>
        /// Subtract Literal from Register - subtract a literal
        /// from the value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        SUB_LIT_REG,
        /// <summary>
        /// Subtract Register from Literal - subtract the value
        /// of the register from a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        SUB_REG_LIT,
        /// <summary>
        /// Subtract Register from Register - subtract the value
        /// of register B from the value of register A.
        /// Result is moved into the accumulator.
        /// </summary>
        SUB_REG_REG,
        /// <summary>
        /// Increment Register - increment the value of the register.
        /// </summary>
        INC_REG,
        /// <summary>
        /// Decrement Register - decrement the value of the register.
        /// </summary>
        DEC_REG,
        /// <summary>
        /// Multiply Literal by Register - multiply a literal by the
        /// value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        MUL_LIT_REG,
        /// <summary>
        /// Multiply Register by Register - multiply the value of
        /// register A by the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        MUL_REG_REG,
        /// <summary>
        /// Modulo Register by Literal - modulo the value of a
        /// register by a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_LIT_REG,
        /// <summary>
        /// Modulo Literal by Register - modulo a literal by
        /// the value of a register.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_REG_LIT,
        /// <summary>
        /// Modulo Register by Register - modulo the value of
        /// register B by the value of register A.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_REG_REG,

        /// <summary>
        /// Bit Test - test is a given bit within a register is set.
        /// The Zero (Z) flag will be set to the value of the bit.
        /// </summary>
        BIT,
        /// <summary>
        /// Left Shift a register value by a literal.
        /// Calculation is performed directly on the register.
        /// </summary>
        LSF_REG_LIT,
        /// <summary>
        /// Left Shift the value of register A by the value of register B.
        /// Calculation is performed directly on register A.
        /// </summary>
        LSF_REG_REG,
        /// <summary>
        /// Right Shift a register value by a literal.
        /// Calculation is performed directly on the register.
        /// </summary>
        RSF_REG_LIT,
        /// <summary>
        /// Right Shift the value of register A by the value
        /// of register B.
        /// Calculation is performed directly on register A.
        /// </summary>
        RSF_REG_REG,
        /// <summary>
        /// Add Register to Literal - add the value of
        /// register A to a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        AND_REG_LIT,
        /// <summary>
        /// Add Register to Register - add the value of register A
        /// to the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        AND_REG_REG,
        /// <summary>
        /// Or Register and Literal - bitwise OR the value of
        /// register A with a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        OR_REG_LIT,
        /// <summary>
        /// Or Register and Register - bitwise OR the value of
        /// register A with the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        OR_REG_REG,
        /// <summary>
        /// XOR Register and Literal - bitwise XOR the value of
        /// register A with a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        XOR_REG_LIT,
        /// <summary>
        /// XOR Register and Register - bitwise XOR the value of
        /// register A with the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        XOR_REG_REG,
        /// <summary>
        /// NOT - bitwise NOT the value of register A.
        /// Result is moved into the accumulator.
        /// </summary>
        NOT,

        /// <summary>
        /// Jump If Not Equal To Literal - if the accumulator is not equal
        /// to the literal A then jump to an address specified by the 
        /// literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JMP_NOT_EQ,
        /// <summary>
        /// Jump If Not Equal Register - if the accumulator is not equal
        /// to the value of the register then jump to an address given by
        /// the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JNE_REG,
        /// <summary>
        /// Jump If Equal Register - if the accumulator is equal to the
        /// value of the register then jump to an address given by
        /// the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JEQ_REG,
        /// <summary>
        /// Jump If Equal To Literal - if the accumulator is equal to the
        /// literal A then jump to an address given by the literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JEQ_LIT,
        /// <summary>
        /// Jump If Less Than Register - if the value of the register
        /// is less than the accumulator then jump to an address specified
        /// by the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JLT_REG,
        /// <summary>
        /// Jump If Less Than Literal - if literal A is less than the
        /// the accumulator then jump to an address specified by
        /// the literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JLT_LIT,
        /// <summary>
        /// Jump If Greater Than Register - if the value of the register
        /// is greater than the accumulator then jump to an address given
        /// by the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JGT_REG,
        /// <summary>
        /// Jump If Greater Than Literal - if literal A is greater than
        /// the accumulator then jump to an address specified by
        /// the literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JGT_LIT,
        /// <summary>
        /// Jump If Less Than Or Equal To Register - if the value of the
        /// register is less than or equal to the accumulator then jump
        /// to an address specified by the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JLE_REG,
        /// <summary>
        /// Jump If Less Than Or Equal To Literal - if literal A is less
        /// than or equal to the accumulator then jump to an address
        /// specified by the literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JLE_LIT,
        /// <summary>
        /// Jump If Greater Than Or Equal To Register - if the value of
        /// the register is greater than or equal to the accumulator
        /// then jump to an address given by the literal.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JGE_REG,
        /// <summary>
        /// Jump If Greater Than Or Equal To Literal - if literal A is
        /// greater or equal to than the accumulator then jump to an
        /// address specified by the literal B.
        /// </summary>
        /// <remarks>
        /// This address is treated as being relative to the base address
        /// of the executable memory region in which the program resides.
        /// </remarks>
        JGE_LIT,

        /// <summary>
        /// Push (integer) Literal to stack - push a literal value onto
        /// the stack.
        /// </summary>
        PSH_LIT,
        /// <summary>
        /// Push (integer) Register value to stack - push the value of a
        /// register onto the stack.
        /// </summary>
        PSH_REG,
        /// <summary>
        /// POP (integer) to Register - pop a value from the stack
        /// into a register.
        /// </summary>
        POP,
        CAL_LIT,
        CAL_REG,
        /// <summary>
        /// Return from subroutine.
        /// </summary>
        RET,

        PUSHL,
        OUT,

        /// <summary>
        /// Halt - halt the execution of the virtual machine.
        /// </summary>
        HLT = int.MaxValue
    }
}

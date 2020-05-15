﻿namespace VMCore
{
    public enum OpCode
    {
        /// <summary>
        /// No Operation - a non-operation instruction. No action to be performed.
        /// </summary>
        NOP,

        /// <summary>
        /// Move Literal to Register - copy a literal into a register.
        /// </summary>
        MOV_LIT_REG,
        /// <summary>
        /// Move Register to Register - copy the value of register A into register B.
        /// </summary>
        MOV_REG_REG,
        /// <summary>
        /// Move Register to Memory - copy the value of the register into memory.
        /// </summary>
        MOV_REG_MEM,
        /// <summary>
        /// Move Register to Memory via Literal Expression - 
        /// copy the value of the register into memory starting from the index given by an expression into the register.
        /// </summary>
        MOV_REG_LIT_EXP_MEM,
        /// <summary>
        /// Move Memory to Register - copy a value from memory into the register.
        /// </summary>
        MOV_MEM_REG,
        /// <summary>
        /// Move from Memory via Literal Expression to Register - 
        /// copy a value from memory starting from the index given by an expression into the register.
        /// </summary>
        MOV_LIT_EXP_MEM_REG,
        /// <summary>
        /// Move Literal to Memory - copy a literal into memory.
        /// </summary>
        MOV_LIT_MEM,
        /// <summary>
        /// Move Register* to Register - 
        /// copy a value from memory into Register B starting from the index given by the value of register A.
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
        /// Add Register to Register - add the contents of register A into register B.
        /// Result is moved into the accumulator.
        /// </summary>
        ADD_REG_REG,
        /// <summary>
        /// Add Literal to Register - add a literal to the value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        ADD_LIT_REG,
        /// <summary>
        /// Subtract Literal from Register - subtract a literal from the value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        SUB_LIT_REG,
        /// <summary>
        /// Subtract Register from Literal - subtract the value of the register from a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        SUB_REG_LIT,
        /// <summary>
        /// Subtract Register from Register - subtract the value of register B from the value of register A.
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
        /// Multiply Literal by Register - multiply a literal by the value of the register.
        /// Result is moved into the accumulator.
        /// </summary>
        MUL_LIT_REG,
        /// <summary>
        /// Multiply Register by Register - multiply the value of register A by the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        MUL_REG_REG,
        /// <summary>
        /// Modulo Register by Literal - modulo the value of a register by a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_LIT_REG,
        /// <summary>
        /// Modulo Literal by Register - modulo a literal by the value of a register.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_REG_LIT,
        /// <summary>
        /// Modulo Register by Register - modulo the value of register B by the value of register A.
        /// Result is moved into the accumulator.
        /// </summary>
        MOD_REG_REG,

        /// <summary>
        /// Bit Test - test is a given bit within a register is set. The Zero (Z)
        /// flag will be set to the value of the bit.
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
        /// Right Shift the value of register A by the value of register B.
        /// Calculation is performed directly on register A.
        /// </summary>
        RSF_REG_REG,
        /// <summary>
        /// Add Register to Literal - add the value of register A to a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        AND_REG_LIT,
        /// <summary>
        /// Add Register to Register - add the value of register A to the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        AND_REG_REG,
        /// <summary>
        /// Or Register and Literal - bitwise OR the value of register A with a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        OR_REG_LIT,
        /// <summary>
        /// Or Register and Register - bitwise OR the value of register A with the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        OR_REG_REG,
        /// <summary>
        /// XOR Register and Literal - bitwise XOR the value of register A with a literal.
        /// Result is moved into the accumulator.
        /// </summary>
        XOR_REG_LIT,
        /// <summary>
        /// XOR Register and Register - bitwise XOR the value of register A with the value of register B.
        /// Result is moved into the accumulator.
        /// </summary>
        XOR_REG_REG,
        /// <summary>
        /// NOT - bitwise NOT the value of register A.
        /// Result is moved into the accumulator.
        /// </summary>
        NOT,

        JMP_NOT_EQ,
        /// <summary>
        /// Jump If Not Equal Register - jump to an address equal to the sum of the value of the register
        /// and the main memory base size (e.g. offset within the program memory space),
        /// if the value is not equal to the accumulator.
        /// </summary>
        JNE_REG,
        JEQ_REG,
        JEQ_LIT,
        JLT_REG,
        JLT_LIT,
        JGT_REG,
        JGT_LIT,
        JLE_REG,
        JLE_LIT,
        JGE_REG,
        JGE_LIT,

        PSH_LIT,
        PSH_REG,
        POP,
        CAL_LIT,
        CAL_REG,

        PUSHL,
        OUT,

        /// <summary>
        /// Halt - halt the execution of the virtual machine.
        /// </summary>
        HLT = int.MaxValue
    }
}

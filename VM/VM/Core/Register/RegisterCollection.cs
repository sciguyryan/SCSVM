using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VMCore.VM.Core.Register
{
    public class RegisterCollection
    {
        /// <summary>
        /// A list of available registers.
        /// </summary>
        public Dictionary<Registers, Register> Registers { get; } =
            new Dictionary<Registers, Register>();

        /// <summary>
        /// The CPU to which this register collection is bound.
        /// </summary>
        public Cpu Cpu { get; }

        public RegisterCollection(Cpu aCpu)
        {
            Cpu = aCpu;

            const RegisterAccess rw =
                RegisterAccess.R | RegisterAccess.W;

            const RegisterAccess r = RegisterAccess.R;

            const RegisterAccess pw = RegisterAccess.PW;

            const RegisterAccess rpw = r | pw;

            const RegisterAccess prpw =
                RegisterAccess.PR | pw;

            #region Register Decelerations

            // --------------- Data Registers ---------------
            var regR1 =
                new Register(aCpu, rw, Core.Register.Registers.R1);
            var regR2 =
                new Register(aCpu, rw, Core.Register.Registers.R2);
            var regR3 =
                new Register(aCpu, rw, Core.Register.Registers.R3);
            var regR4 =
                new Register(aCpu, rw, Core.Register.Registers.R4);
            var regR5 =
                new Register(aCpu, rw, Core.Register.Registers.R5);
            var regR6 =
                new Register(aCpu, rw, Core.Register.Registers.R6);
            var regR7 =
                new Register(aCpu, rw, Core.Register.Registers.R7);
            var regR8 =
                new Register(aCpu, rw, Core.Register.Registers.R8);

            // --------------- Special Registers ---------------
            var regAc =
                new Register(aCpu, rw, Core.Register.Registers.AC);
            var regIp =
                new Register(aCpu, rw, Core.Register.Registers.IP);
            var regSp =
                new Register(aCpu, prpw, Core.Register.Registers.SP);
            var regFp =
                new Register(aCpu, rpw, Core.Register.Registers.FP);
            var regFl =
                new Register(aCpu,
                             rw,
                             Core.Register.Registers.FL,
                             typeof(CpuFlags));
            var regPc =
                new Register(aCpu, rpw, Core.Register.Registers.PC);

            #endregion // Register Decelerations

            #region Register Binding

            Registers.Add(Core.Register.Registers.R1, regR1);
            Registers.Add(Core.Register.Registers.R2, regR2);
            Registers.Add(Core.Register.Registers.R3, regR3);
            Registers.Add(Core.Register.Registers.R4, regR4);
            Registers.Add(Core.Register.Registers.R5, regR5);
            Registers.Add(Core.Register.Registers.R6, regR6);
            Registers.Add(Core.Register.Registers.R7, regR7);
            Registers.Add(Core.Register.Registers.R8, regR8);
            Registers.Add(Core.Register.Registers.AC, regAc);
            Registers.Add(Core.Register.Registers.IP, regIp);
            Registers.Add(Core.Register.Registers.SP, regSp);
            Registers.Add(Core.Register.Registers.FP, regFp);
            Registers.Add(Core.Register.Registers.FL, regFl);
            Registers.Add(Core.Register.Registers.PC, regPc);

            #endregion // Register Binding
        }

        /// <summary>
        /// Get or set a register with a security context.
        /// </summary>
        /// <param name="aRegTuple">
        /// A tuple of the register identifier and the security context.
        /// </param>
        /// <returns>
        /// The value of the register if accessed via get,
        /// nothing otherwise.
        /// </returns>
        public int this[(Registers r, SecurityContext c) aRegTuple]
        {
            get =>
                Registers[aRegTuple.r].GetValue(aRegTuple.c);
            set =>
                Registers[aRegTuple.r].SetValue(value, aRegTuple.c);
        }

        /// <summary>
        /// Get or set a register with a security context.
        /// </summary>
        /// <param name="aRegTuple">
        /// A tuple of the register ID and the security context.
        /// </param>
        /// <returns>
        /// The value of the register if accessed via get,
        /// nothing otherwise.
        /// </returns>
        public int this[(int rID, SecurityContext c) aRegTuple]
        {
            get =>
                Registers[(Registers)aRegTuple.rID]
                    .GetValue(aRegTuple.c);
            set =>
                Registers[(Registers)aRegTuple.rID]
                    .SetValue(value, aRegTuple.c);
        }

        /// <summary>
        /// Short hand get or set a register with a default (user)
        /// security context.
        /// </summary>
        /// <returns>
        /// The value of the register if accessed via get,
        /// nothing otherwise.
        /// </returns>
        public int this[Registers aReg]
        {
            get =>
                Registers[aReg].GetValue(SecurityContext.User);
            set =>
                Registers[aReg].SetValue(value, SecurityContext.User);
        }

        /// <summary>
        /// Short hand get or set a register with a default (user)
        /// security context.
        /// </summary>
        /// <returns>
        /// The value of the register if accessed via get,
        /// nothing otherwise.
        /// </returns>
        public int this[int aRegId]
        {
            get =>
                Registers[(Registers)aRegId]
                    .GetValue(SecurityContext.User);
            set =>
                Registers[(Registers)aRegId]
                    .SetValue(value, SecurityContext.User);
        }

        /*public int this[Registers reg]
        {
            get
            {
                int ret = 0;

                //return Registers[(int)reg].GetValue();
                switch (reg)
                {
                    // Bits  : 8 16
                    // Bytes : 1  2
                    // Only provide the two highest order bytes.
                    // from the integer.
                    case VMCore.Registers.AX:
                        ret = Registers[reg].GetValue() >> 16;
                        break;

                    // Bits  : 8
                    // Byte  : 1
                    // Only provide the highest order byte.
                    // from the integer.
                    case VMCore.Registers.AL:
                        ret = Registers[reg].GetValue() >> 24;
                        break;

                    // Bits  : 16
                    // Byte  : 2
                    // Only provide the second highest order byte.
                    // from the integer.
                    case VMCore.Registers.AH:
                        // The same as the ?X fetch, however this time we also
                        // clear the uper byte (8 bits).
                        ret = (Registers[reg].GetValue() >> 16) & ~0xFF00;
                        break;

                    default:
                        ret = Registers[reg].GetValue();
                        break;
                }

                return ret;
            }
            set
            {
                Registers[reg].SetValue(value);
            }
        }*/

        /// <summary>
        /// Print a formatted list registers and their
        /// contents directly.
        /// </summary>
        public void PrintRegisters(bool aToDebug = false)
        {
            foreach (var (key, value) in Registers)
            {
                var reg = Enum.GetName(typeof(Registers), key);
                var val = value.GetValue(SecurityContext.System);

                var str = $"{reg,5}{val,10:X8}";

                if (value.IsFlagRegister())
                {
                    str += $" ({value.ToFlagStateString()})";
                }

                if (!aToDebug)
                {
                    Console.WriteLine(str);
                }
                else
                {
                    Debug.WriteLine(str);
                }
            }
        }
    }
}

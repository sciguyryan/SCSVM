using System;
using System.Collections.Generic;

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

            // Initialization here is done manually as, if I ever
            // decide to re-add the shadow registers this
            // will be important.
            // Auto-initializing by enumerating over the enum is
            // cleaner but will add registers such as AX, which
            // is a shadow register for EAX and cannot be counted
            // as a separate register.

            const RegisterAccess rw =
                RegisterAccess.R | RegisterAccess.W;

            const RegisterAccess r = RegisterAccess.R;

            const RegisterAccess pw = RegisterAccess.PW;

            const RegisterAccess rpw = r | pw;

            const RegisterAccess prpw =
                RegisterAccess.PR | pw;

            // Data registers.
            Registers.Add(Core.Register.Registers.R1,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R2,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R3,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R4,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R5,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R6,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R7,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.R8,
                          new Register(aCpu, rw));

            Registers.Add(Core.Register.Registers.AC,
                new Register(aCpu, rw));

            // Special registers.
            Registers.Add(Core.Register.Registers.IP,
                          new Register(aCpu, rw));
            Registers.Add(Core.Register.Registers.SP,
                          new Register(aCpu, prpw));
            Registers.Add(Core.Register.Registers.FP,
                          new Register(aCpu, rpw));
            Registers.Add(Core.Register.Registers.FL,
                          new Register(aCpu, rw, typeof(CpuFlags)));
            Registers.Add(Core.Register.Registers.PC,
                          new Register(aCpu, r | pw));

#if DEBUG
            // For debugging.
            foreach (var (key, _) in Registers)
            {
                Registers[key].SetId(key);
            }
#endif
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
        /// Apply a hook to a register.
        /// </summary>
        /// <param name="aReg">
        /// The register to which the hook should be applied.
        /// </param>
        /// <param name="aHook">
        /// The hook to be executed when the hook is triggered.
        /// </param>
        /// <param name="aHookType">
        /// The type of hook to be applied.
        /// </param>
        public void Hook(Registers aReg,
                         Action<int> aHook,
                         Register.HookTypes aHookType)
        {
            switch (aHookType)
            {
                case Register.HookTypes.Change:
                    Registers[aReg].OnChange = aHook;
                    break;

                case Register.HookTypes.Read:
                    Registers[aReg].OnRead = aHook;
                    break;

                default:
                    // TODO - handle this better.
                    break;
            }
        }

        /// <summary>
        /// Print a formatted list registers and their
        /// contents directly to the console.
        /// </summary>
        public void PrintRegisters()
        {
            foreach (var (key, value) in Registers)
            {
                var reg = Enum.GetName(typeof(Registers), key);
                var val = value.GetValue(SecurityContext.System);

                Console.Write("{0,5}{1,10:X8}", reg, val);

                if (!value.IsFlagRegister())
                {
                    Console.WriteLine();
                }
                else
                {
                    var fs = value.ToFlagStateString();
                    Console.WriteLine(" (" + fs + ")");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VMCore.VM.IO.InterruptHandlers;

namespace VMCore.VM.Core.Interrupts
{
    public static class InterruptManager
    {
        /// <summary>
        /// A dictionary of the identified interrupt handlers.
        /// </summary>
        public static Dictionary<InterruptTypes, IInterruptHandler> Handlers { get; set; } = 
            new Dictionary<InterruptTypes, IInterruptHandler>();

        /// <summary>
        /// Handle an interrupt of a specified type.
        /// </summary>
        /// <param name="interruptType">The interrupt type.</param>
        /// <param name="vm">The virtual machine instance in which the interrupt should be handled.</param>
        public static void Interrupt(InterruptTypes interruptType, VirtualMachine vm)
        {
            if (Handlers.TryGetValue(interruptType, out IInterruptHandler handler))
            {
                handler.Handle(vm);
                return;
            }

            throw new Exception($"Interrupt: interrupt type {interruptType} has no registered handler.");
        }
    }
}
using System;
using System.Collections.Generic;
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
        /// <param name="aInterruptType">The interrupt type.</param>
        /// <param name="aVm">
        /// The virtual machine instance in which the interrupt should be handled.
        /// </param>
        public static void Interrupt(InterruptTypes aInterruptType,
                                     VirtualMachine aVm)
        {
            if (!Handlers.TryGetValue(aInterruptType, out var handler))
            {
                throw new Exception
                (
                    $"Interrupt: interrupt type " +
                    $"{aInterruptType} has no registered handler."
                );
            }

            handler.Handle(aVm);
        }
    }
}

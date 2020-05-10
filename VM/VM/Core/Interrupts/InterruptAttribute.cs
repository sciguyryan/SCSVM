using System;
using VMCore.VM.IO.InterruptHandlers;

namespace VMCore.VM.Core.Interrupts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InterruptAttribute
        : Attribute
    {
        public InterruptTypes InterruptType { get; set; }

        public InterruptAttribute(InterruptTypes aInterruptType)
        {
            InterruptType = aInterruptType;
        }
    }
}

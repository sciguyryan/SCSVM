namespace VMCore.VM.Core.Interrupts
{
    public interface IInterruptHandler
    {
        /// <summary>
        /// Handle an interrupt of a specified type.
        /// </summary>
        /// <param name="interruptType">The interrupt type.</param>
        /// <param name="aVm">The virtual machine instance in which the interrupt should be handled.</param>
        void Handle(VirtualMachine aVm);
    }
}

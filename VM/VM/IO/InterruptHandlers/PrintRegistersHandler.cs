using VMCore.VM.Core.Interrupts;

namespace VMCore.VM.IO.InterruptHandlers
{
    [Interrupt(InterruptTypes.PrintRegisters)]
    internal class PrintRegistersHandler
        : IInterruptHandler
    {
        public void Handle(VirtualMachine aVm)
        {
            aVm.Cpu.Registers.PrintRegisters();
        }
    }
}

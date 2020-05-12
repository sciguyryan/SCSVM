using System;
using System.Collections.Generic;

namespace VMCore.VM
{
    // TODO - this should have a low overhead when breakpoints are not
    // being used, but I might make the hooks DEBUG only to reduce that
    // even further.
    public class Debugger
    {
        /// <summary>
        /// The virtual machine instance to which this debugger is bound.
        /// </summary>
        private VirtualMachine _vm;

        public Debugger(VirtualMachine aVm)
        {
            _vm = aVm;
        }

        /// <summary>
        /// A list of the currently requested breakpoints and the action
        /// to be performed when they execute.
        /// </summary>
        public List<Breakpoint> Breakpoints { get; set; } =
            new List<Breakpoint>();

        /// <summary>
        /// Add a break point for a given position and type.
        /// </summary>
        /// <param name="aBreakAt">The position at which the breakpoint should trigger.</param>
        /// <param name="aType">The type of breakpoint to apply.</param>
        public void AddBreakpoint(int aBreakAt, 
                                  Breakpoint.BreakpointType aType,
                                  Breakpoint.BreakpointAction aAction)
        {
            Breakpoints.Add(new Breakpoint(aBreakAt, aType, aAction));
        }

        /// <summary>
        /// Remove a break point for a given position and type.
        /// </summary>
        /// <param name="aBreakAt">The position at which the breakpoint should trigger.</param>
        /// <param name="aType">The type of breakpoint to apply.</param>
        public void RemoveBreakpoint(int aBreakAt,
                                     Breakpoint.BreakpointType aType)
        {
            var bpClone = Breakpoints.ToArray();
            for (var i = 0; i < bpClone.Length; i++)
            {
                var bp = bpClone[i];
                if (bp.BreakAt == aBreakAt && bp.Type == aType)
                {
                    Breakpoints.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Remove any breakpoints that are registered.
        /// </summary>
        public void RemoveAllBreakpoints()
        {
            Breakpoints.Clear();
        }

        /// <summary>
        /// Returns a breakpoint for a given position and type if one exists.
        /// </summary>
        /// <param name="aBreakAt"></param>
        /// <param name="aType"></param>
        /// <returns></returns>
        public Breakpoint GetBreakpoint(int aBreakAt,
                                        Breakpoint.BreakpointType aType)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType && bp.BreakAt == aBreakAt)
                {
                    return bp;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if a breakpoint exists for a given position and for a given type.
        /// </summary>
        /// <param name="aBreakAt">The position at which the breakpoint is expected to trigger.</param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <returns>A boolean indicating true if a breakpoint exists, false otherwise.</returns>
        public bool HasBreakpoint(int aBreakAt,
                                  Breakpoint.BreakpointType aType)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType && bp.BreakAt == aBreakAt)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a breakpoint exists for a given register value.
        /// </summary>
        /// <param name="aReg">The register who's value should be checked.</param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <returns>A boolean indicating true if a breakpoint exists, false otherwise.</returns>
        public bool HasBreakpoint(Registers aReg,
                                  Breakpoint.BreakpointType aType)
        {
            var breakAt = 
                _vm.CPU.Registers[(aReg, SecurityContext.System)];

            return HasBreakpoint(breakAt, aType);
        }

        /// <summary>
        /// Checks if one or more breakpoints of a given type exists.
        /// </summary>
        /// <param name="aType">The type of breakpoint.</param>
        /// <returns>A boolean true if one or more breakpoints have been added of this breakpoint type, false otherwise.</returns>
        public bool HasBreakpointOfType(Breakpoint.BreakpointType aType)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trigger an action associated with a breakpoint, if one is set.
        /// </summary>
        /// <param name="aBreakAt">The position at which the breakpoint is expected to trigger.</param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <returns>A boolean true if the breakpoint should trigger a halt in the CPU, false otherwise.</returns>
        public bool TriggerBreakpoint(int aBreakAt,
                                      Breakpoint.BreakpointType aType)
        {
            if (!HasBreakpoint(aBreakAt, aType))
            {
                return false;
            }

            var breakpoint = GetBreakpoint(aBreakAt, aType);
            if (breakpoint == null || breakpoint.Action == null)
            {
                return false;
            }

            return breakpoint.Action.Invoke(aBreakAt);
        }

        public void Step()
        {
            throw new NotImplementedException();
        }
    }
}
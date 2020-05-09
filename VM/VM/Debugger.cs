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

        public Debugger(VirtualMachine vm)
        {
            _vm = vm;
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
        /// <param name="breakAt">The position at which the breakpoint should trigger.</param>
        /// <param name="type">The type of breakpoint to apply.</param>
        public void AddBreakpoint(int breakAt, Breakpoint.BreakpointType type, Breakpoint.BreakpointAction action)
        {
            Breakpoints.Add(new Breakpoint(breakAt, type, action));
        }

        /// <summary>
        /// Remove a break point for a given position and type.
        /// </summary>
        /// <param name="breakAt">The position at which the breakpoint should trigger.</param>
        /// <param name="type">The type of breakpoint to apply.</param>
        public void RemoveBreakpoint(int breakAt, Breakpoint.BreakpointType type)
        {
            var bpClone = Breakpoints.ToArray();
            for (var i = 0; i < bpClone.Length; i++)
            {
                var bp = bpClone[i];
                if (bp.BreakAt == breakAt && bp.Type == type)
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
        /// <param name="breakAt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Breakpoint GetBreakpoint(int breakAt, Breakpoint.BreakpointType type)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == type && bp.BreakAt == breakAt)
                {
                    return bp;
                }
            }

            return null;
        }

        /// <summary>
        /// Check if a breakpoint exists for a given position and for a given type.
        /// </summary>
        /// <param name="breakAt">The position at which the breakpoint is expected to trigger.</param>
        /// <param name="type">The type of breakpoint.</param>
        /// <returns>A boolean indicating true if a breakpoint exists, false otherwise.</returns>
        public bool HasBreakPoint(int breakAt, Breakpoint.BreakpointType type)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == type && bp.BreakAt == breakAt)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a breakpoint exists for a given register value.
        /// </summary>
        /// <param name="reg">The register who's value should be checked.</param>
        /// <param name="type">The type of breakpoint.</param>
        /// <returns></returns>
        public bool HasBreakPoint(Registers reg, Breakpoint.BreakpointType type)
        {
            var breakAt = _vm.CPU.Registers[(reg, SecurityContext.System)];

            return HasBreakPoint(breakAt, type);
        }

        /// <summary>
        /// Checks if one or more breakpoints of a given type exists.
        /// </summary>
        /// <param name="type">The type of breakpoint.</param>
        /// <returns>A boolean true if one or more breakpoints have been added of this breakpoint type, false otherwise.</returns>
        public bool HasBreakPointOfType(Breakpoint.BreakpointType type)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trigger an action associated with a breakpoint, if one is set.
        /// </summary>
        /// <param name="breakAt">The position at which the breakpoint is expected to trigger.</param>
        /// <param name="type">The type of breakpoint.</param>
        /// <returns>A boolean true if the breakpoint should trigger a halt in the CPU, false otherwise.</returns>
        public bool TriggerBreakPoint(int breakAt, Breakpoint.BreakpointType type)
        {
            if (!HasBreakPoint(breakAt, type))
            {
                return false;
            }

            var breakpoint = GetBreakpoint(breakAt, type);
            if (breakpoint == null || breakpoint.Action == null)
            {
                return false;
            }

            return breakpoint.Action.Invoke(breakAt);
        }

        public void Step()
        {
            throw new NotImplementedException();
        }
    }
}
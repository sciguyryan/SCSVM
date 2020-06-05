#nullable enable

using System;
using System.Collections.Generic;
using VMCore.VM.Core;
using VMCore.VM.Core.Breakpoints;
using VMCore.VM.Core.Register;

namespace VMCore.VM
{
    // TODO - this should have a low overhead when breakpoints are not
    // being used, but I might make the hooks DEBUG only to reduce that
    // even further.
    public class Debugger
    {
        /// <summary>
        /// The virtual machine instance to which this debugger belongs.
        /// </summary>
        private readonly VirtualMachine _vm;

        public Debugger(VirtualMachine aVm)
        {
            _vm = aVm;
        }

        /// <summary>
        /// A list of the currently requested breakpoints and the action
        /// to be performed when they execute.
        /// </summary>
        public List<Breakpoint> Breakpoints { get; set; } =
            new List<Breakpoint>(1024);

        /// <summary>
        /// Add a break point for a given position and type.
        /// </summary>
        /// <param name="aBreakAt">
        /// The position at which the breakpoint should trigger.
        /// </param>
        /// <param name="aType">
        /// The type of breakpoint to apply.
        /// </param>
        /// <param name="aAction">
        /// The action to be performed when the breakpoint is triggered.
        /// </param>
        /// <param name="aRegId">
        /// The register to which this breakpoint should be hooked.
        /// </param>
        /// <param name="aBreakAtAny">
        /// If this breakpoint should disregard the break at
        /// value and trigger whenever the base condition is met.
        /// </param>
        public void AddBreakpoint(int aBreakAt,
                                  BreakpointType aType,
                                  Breakpoint.BreakpointAction aAction,
                                  Registers? aRegId = null,
                                  bool aBreakAtAny = false)
        {
            if (aRegId is null &&
                (aType == BreakpointType.RegisterRead ||
                 aType == BreakpointType.RegisterWrite))
            {
                throw new ArgumentNullException
                (
                    "AddBreakpoint: no register ID was specified " +
                    "a register-type breakpoint. This is not permitted."
                );
            }

            var bp =
                new Breakpoint(aBreakAt,
                    aType,
                    aAction,
                    aRegId,
                    aBreakAtAny);
            Breakpoints.Add(bp);
        }

        /// <summary>
        /// Remove a break point for a given position and type.
        /// </summary>
        /// <param name="aBreakAt">
        /// The position at which the breakpoint should trigger.
        /// </param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <param name="aRegId">
        /// The register to which this breakpoint is hooked.
        /// Expected to be null when dealing with non-register
        /// breakpoints.
        /// </param>
        public void RemoveBreakpoint(int aBreakAt,
                                     BreakpointType aType,
                                     Registers? aRegId = null)
        {
            var bpClone = Breakpoints.ToArray();
            for (var i = 0; i < bpClone.Length; i++)
            {
                var bp = bpClone[i];
                if (bp.Type == aType &&
                    (bp.BreakAt == aBreakAt || bp.BreakAtAnyValue) &&
                    bp.RegisterId == aRegId)
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
        /// Returns an array of breakpoints that match the criteria.
        /// </summary>
        /// <param name="aBreakAt">
        /// The position at which the breakpoint should trigger.
        /// </param>
        /// <param name="aType">The type of breakpoint to apply.</param>
        /// <param name="aRegId">
        /// The register to which this breakpoint is hooked.
        /// Expected to be null when dealing with non-register
        /// breakpoints.
        /// </param>
        /// <returns>
        /// An array of objects that match the criteria. An empty array
        /// if none were found.
        /// </returns>
        public Breakpoint[] GetBreakpoints(int aBreakAt,
                                           BreakpointType aType,
                                           Registers? aRegId = null)
        {
            List<Breakpoint> bps = new List<Breakpoint>();

            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType &&
                    (bp.BreakAt == aBreakAt || bp.BreakAtAnyValue) &&
                    bp.RegisterId == aRegId)
                {
                    bps.Add(bp);
                }
            }

            return bps.ToArray();
        }

        /// <summary>
        /// Check if a breakpoint exists for a given position and
        /// for a given type.
        /// </summary>
        /// <param name="aBreakAt">
        /// The position at which the breakpoint is expected to trigger.
        /// </param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <param name="aRegId">
        /// The register to which this breakpoint is hooked.
        /// Expected to be null when dealing with non-register
        /// breakpoints.
        /// </param>
        /// <returns>
        /// A boolean indicating true if a breakpoint exists, false otherwise.
        /// </returns>
        public bool HasBreakpoint(int aBreakAt,
                                  BreakpointType aType,
                                  Registers? aRegId = null)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType && 
                    (bp.BreakAt == aBreakAt || bp.BreakAtAnyValue) &&
                    bp.RegisterId == aRegId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if one or more breakpoints of a given type exists.
        /// </summary>
        /// <param name="aType">The type of breakpoint.</param>
        /// <param name="aRegId">
        /// The register to which the breakpoint is hooked.
        /// Expected to be null when dealing with non-register
        /// breakpoints.
        /// </param>
        /// <returns>
        /// A boolean true if one or more breakpoints of this type
        /// have been specified, false otherwise.
        /// </returns>
        public bool HasBreakpointOfType(BreakpointType aType,
                                        Registers? aRegId = null)
        {
            foreach (var bp in Breakpoints)
            {
                if (bp.Type == aType &&
                    bp.RegisterId == aRegId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Trigger an action associated with one or more 
        /// </summary>
        /// <param name="aBreakAt">
        /// The position at which the breakpoint is expected to trigger.
        /// </param>
        /// <param name="aType">The type of breakpoint.</param>
        /// <param name="aRegId">
        /// The register to which the breakpoint is hooked.
        /// Expected to be null when dealing with non-register
        /// breakpoints.
        /// </param>
        /// <returns>
        /// A boolean true if any breakpoint has specified that the CPU
        /// should halt, false otherwise.
        /// </returns>
        public bool TriggerBreakpoint(int aBreakAt,
                                      BreakpointType aType,
                                      Registers? aRegId = null)
        {
            if (!HasBreakpoint(aBreakAt, aType, aRegId))
            {
                return false;
            }

            var shouldHalt = false;
            var bpd = 
                GetBreakpoints(aBreakAt, aType, aRegId);

            foreach (var bp in bpd)
            {
                if (bp?.Action != null && bp.Action.Invoke(aBreakAt))
                {
                    shouldHalt = true;
                }
            }

            return shouldHalt;
        }
    }
}
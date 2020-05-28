using System;
using System.Collections.Generic;
using System.Text;

namespace VMCore.VM
{
    public class Breakpoint
    {
        /// <summary>
        /// An delegate function to be fired when a breakpoint
        /// is triggered.
        /// </summary>
        /// <param name="aDataValue">The value passed from the breakpoint.</param>
        /// <returns>A boolean, true if the Cpu should halt executing, false otherwise.</returns>
        public delegate bool BreakpointAction(int aDataValue);

        /// <summary>
        /// A type of breakpoint to be fired.
        /// </summary>
        public enum BreakpointType
        {
            /// <summary>
            /// Instruction pointer
            /// </summary>
            IP,
            /// <summary>
            /// Memory
            /// </summary>
            Memory,
            /// <summary>
            /// Program counter
            /// </summary>
            PC
        }

        /// <summary>
        /// The position at which this breakpoint should trigger.
        /// </summary>
        public int BreakAt { get; set; }

        /// <summary>
        /// The type of this breakpoint.
        /// </summary>
        public BreakpointType Type { get; set; }

        /// <summary>
        /// The action that should be triggered when this breakpoint is encountered.
        /// </summary>
        public BreakpointAction Action { get; set; }

        public Breakpoint(int breakAt, BreakpointType aType,
                          BreakpointAction aAction)
        {
            BreakAt = breakAt;
            Type = aType;
            Action = aAction;
        }
    }
}

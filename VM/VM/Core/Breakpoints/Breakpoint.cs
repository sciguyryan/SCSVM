using VMCore.VM.Core.Register;

namespace VMCore.VM.Core.Breakpoints
{
    public class Breakpoint
    {
        /// <summary>
        /// An delegate function to be fired when a breakpoint
        /// is triggered.
        /// </summary>
        /// <param name="aDataValue">
        /// The value passed from the breakpoint.
        /// </param>
        /// <returns>
        /// A boolean, true if the CPU should halt executing,
        /// false otherwise.
        /// </returns>
        public delegate bool BreakpointAction(int aDataValue);

        /// <summary>
        /// The value at which this breakpoint should trigger.
        /// </summary>
        public int BreakAt { get; set; }

        /// <summary>
        /// The type of this breakpoint.
        /// </summary>
        public BreakpointType Type { get; set; }

        /// <summary>
        /// The action to be invoked when this breakpoint is triggered.
        /// </summary>
        public BreakpointAction Action { get; set; }

        /// <summary>
        /// The ID of the register to which this breakpoint
        /// is hooked. This value is expected to be null
        /// for non-register breakpoints.
        /// </summary>
        public Registers? RegisterId { get; set; }

        /// <summary>
        /// If this breakpoint should disregard the break at
        /// value and trigger whenever the base condition is
        /// met.
        /// </summary>
        public bool BreakAtAnyValue { get; set; }

        /// <summary>
        /// Create a breakpoint.
        /// </summary>
        /// <param name="aBreakAt">
        /// The value at which this breakpoint should trigger.
        /// </param>
        /// <param name="aType">The type of this breakpoint.</param>
        /// <param name="aAction">
        /// The action to be invoked when this breakpoint is triggered.
        /// </param>
        /// <param name="aRegId">
        /// The ID of the register to which this breakpoint
        /// is hooked. This value is expected to be null
        /// for non-register breakpoints.
        /// </param>
        /// <param name="aBreakAtAny">
        /// If this breakpoint should disregard the break at
        /// value and trigger whenever the base condition is
        /// met.
        /// </param>
        public Breakpoint(int aBreakAt,
                          BreakpointType aType,
                          BreakpointAction aAction,
                          Registers? aRegId = null,
                          bool aBreakAtAny = false)
        {
            BreakAt = aBreakAt;
            Type = aType;
            Action = aAction;
            RegisterId = aRegId;
            BreakAtAnyValue = aBreakAtAny;
        }
    }
}

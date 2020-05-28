using System;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Utilities;

namespace VMCore.VM.Core.Reg
{
    public class Register
    {
        /// <summary>
        /// The type of hook requested for this register.
        /// </summary>
        public enum HookTypes
        {
            /// <summary>
            /// A hook that fires when a value is written to the register.
            /// </summary>
            Change,
            /// <summary>
            /// A hook that fires when a value is read from the register.
            /// </summary>
            Read
        }

        // TODO - would a security violation hook be useful?

        /// <summary>
        /// The hook that should be fired when the value of this register
        /// is changed.
        /// </summary>
        public Action<int> OnChange;

        /// <summary>
        /// The hook that should be fired when the value of this register
        /// is read.
        /// </summary>
        public Action<int> OnRead;

        /// <summary>
        /// The permission access flags for this register.
        /// </summary>
        public RegisterAccess AccessFlags { get; private set; }

        /// <summary>
        /// The Cpu instance associated with this register.
        /// </summary>
        public Cpu CPU { get; private set; }

        /// <summary>
        /// The internal value of this register.
        /// </summary>
        private int _value;

        /// <summary>
        /// The internal ID of this register, mainly used for debugging.
        /// </summary>
        private Registers _registerID;

        /// <summary>
        /// The internal enum instance for the flag type of
        /// this register. Will be null if the register
        /// is not a flag-type register.
        /// </summary>
        private Type _flagType;

        public Register(Cpu aCpu,
                        RegisterAccess aAccess,
                        Type aFlagType = null)
        {
            CPU = aCpu;
            AccessFlags = aAccess;
            _flagType = aFlagType;
        }

        /// <summary>
        /// Set the internal ID of this register.
        /// Mainly used for debugging.
        /// </summary>
        /// <param name="aId">
        /// The ID to be applied to this register.
        /// </param>
        public void SetID(Registers aId)
        {
            _registerID = aId;
        }

        /// <summary>
        /// Gets the value from register.
        /// </summary>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <exception cref="RegisterAccessViolationException">
        /// Thrown if the permission flag is not set for the register.
        /// </exception>
        public int GetValue(SecurityContext aContext)
        {
            ValidateAccess(DataAccessType.Read, aContext);

            OnRead?.Invoke(_value);

            return _value;
        }

        /// <summary>
        /// Sets the value of the register.
        /// </summary>
        /// <param name="aValue">
        /// The value to which the register should be set.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <exception cref="RegisterAccessViolationException">
        /// Thrown if the permission flag is not set for the register.
        /// </exception>
        public void SetValue(int aValue, SecurityContext aContext)
        {
            ValidateAccess(DataAccessType.Write, aContext);
            _value = aValue;

            OnChange?.Invoke(aValue);
        }

        /// <summary>
        /// If this register is a flag-type register.
        /// </summary>
        /// <returns>
        /// A boolean, true indicating that this register is a 
        /// flag-type register, false otherwise.
        /// </returns>
        public bool IsFlagRegister()
        {
            return _flagType != null;
        }

        /// <summary>
        /// Convert the value of this register into a string.
        /// </summary>
        /// <returns>A string giving the value of the register.</returns>
        public override string ToString()
        {
            // We do not need to validate the permissions
            // here as this cannot be called from user code.
            return _value.ToString();
        }

        /// <summary>
        /// Return a string giving the state of the flags
        /// within this register based on the enumeration
        /// types.
        /// </summary>
        /// <returns>A string giving the state of the flags.</returns>
        public string ToFlagStateString()
        {
            return Utils.MapIntegerBitsToFlagsEnum(_flagType, _value);
        }

        /// <summary>
        /// Checks if the register can be accessed in a specified way.
        /// Using a system-level security context will always grant access.
        /// </summary>
        /// <param name="aType">The data access type to check.</param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <exception cref="RegisterAccessViolationException">
        /// Thrown if the permission flag is not set for the register.
        /// </exception>
        private void ValidateAccess(DataAccessType aType,
                                    SecurityContext aContext)
        {
            bool hasFlags;
            if (aType == DataAccessType.Read)
            {
                hasFlags =
                    AccessFlags.HasFlag(RegisterAccess.R) ||
                    (AccessFlags.HasFlag(RegisterAccess.PR) &&
                     aContext == SecurityContext.System);
            }
            else if (aType == DataAccessType.Write)
            {
                hasFlags =
                    AccessFlags.HasFlag(RegisterAccess.W) ||
                    (AccessFlags.HasFlag(RegisterAccess.PW) &&
                     aContext == SecurityContext.System);
            }
            else
            {
                throw new NotSupportedException
                (
                    $"ValidateAccess: attempted to check a non-valid " +
                    $"data access type."
                );
            }

            if (!hasFlags)
            {
                throw new RegisterAccessViolationException
                (
                    $"ValidateAccess: attempted to access a register without " +
                    $"the correct security context or access flags. " +
                    $"Access Type = {aType}, AccessFlags = {AccessFlags}"
                );
            }
        }
    }
}

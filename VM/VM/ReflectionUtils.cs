using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VMCore.VM.Core;
using VMCore.VM.Core.Interrupts;
using VMCore.VM.Core.Sockets;

namespace VMCore.VM
{
    /// <summary>
    /// A set of utility functions to work with reflection.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// An array of all of the types present in this assembly.
        /// </summary>
        public static Type[] TypesCache { get; private set; }

        /// <summary>
        /// A cached of the opcodes mapped to their instruction instances.
        /// </summary>
        public static Dictionary<OpCode, Instruction> InstructionCache { get; private set; } =
            new Dictionary<OpCode, Instruction>();

        /// <summary>
        /// Build the type caches used by this application and applies
        /// any hooked types that are being used.
        /// </summary>
        /// <param name="aInsOnly">A boolean, true if only the instruction cache should be built, false otherwise.</param>
        /// <param name="aForceRebuild">A boolean, true if the caches should be cleared and rebuilt from scratch, false otherwise.</param>
        public static void BuildCachesAndHooks(bool aInsOnly = false,
                                               bool aForceRebuild = false)
        {
            if (aForceRebuild)
            {
                ClearCachesAndHooks();
            }

            // If there is anything in our instruction cache
            // then we do not need to run this again.
            if (InstructionCache.Count > 0)
            {
                return;
            }

            var types = new HashSet<Type>();
            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.IsSubclassOf(typeof(Instruction)))
                {
                    HandleInstructionType(t);
                    continue;
                }
                else
                {
                    if (aInsOnly)
                    {
                        // We have been instructed to only build
                        // the instruction cache. We can skip this
                        // entry.
                        continue;
                    }

                    // Interrupt handlers.
                    if (t.GetInterfaces()
                        .Contains(typeof(IInterruptHandler)))
                    {
                        HookInterruptHandler(t);
                    }

                    // Socket device handlers.
                    if (t.GetInterfaces()
                        .Contains(typeof(ISocketDevice)))
                    {
                        HookSocketDevice(t);
                        continue;
                    }
                }

                types.Add(t);
            }

            TypesCache = types.ToArray();

#if false
            InstructionDebugData();
#endif

            return;
        }

        /// <summary>
        /// Handle the building of the instruction cache.
        /// </summary>
        /// <param name="aType">The type of instruction.</param>
        public static void HandleInstructionType(Type aType)
        {
            var instance = 
                (Instruction)Activator.CreateInstance(aType);

            if (!ReflectionUtils.InstructionCache.TryAdd(instance.OpCode,
                                                         instance))
            {
                throw new Exception($"HandleInstructionType: failed to add instruction implementation class '{instance.GetType()}' for opcode '{instance.OpCode}'. An implementation has already been found for the given opcode.");
            }
        }

        /// <summary>
        /// Handle the hooking of the interrupt handlers.
        /// </summary>
        /// <param name="aType">The type of the interruption.</param>
        public static void HookInterruptHandler(Type aType)
        {
            var handler = 
                (IInterruptHandler)Activator.CreateInstance(aType);
            var attr = aType.GetCustomAttribute<InterruptAttribute>();

            InterruptManager.Handlers.Add(attr.InterruptType, handler);
        }

        /// <summary>
        /// Handle the hooking of the socket device read/write handlers.
        /// </summary>
        /// <param name="aType">The type of the socket device.</param>
        public static void HookSocketDevice(Type aType)
        {
            var device = 
                (ISocketDevice)Activator.CreateInstance(aType);
            var attrs = aType.GetCustomAttributes<SocketAttribute>();

            foreach (var att in attrs)
            {
                if (att.Access == SocketAccess.Read)
                {
                    SocketDeviceManager
                        .ReadSockets.Add(att.SocketAddress, device);
                }
                else
                {
                    SocketDeviceManager
                        .WriteSockets.Add(att.SocketAddress, device);
                }
            }
        }

        /// <summary>
        /// Debug logging and validation for the instruction cache.
        /// </summary>
        private static void InstructionDebugData()
        {
            // Debug logging to indicate any opcodes that are
            // missing implementations.
            List<string> entries = new List<string>
             {
                 $"There are {Enum.GetValues(typeof(OpCode)).Length} OpCodes listed."
             };

            var logPath = Utils.GetProgramDirectory() + @"\OpCodes.txt";
            foreach (OpCode op in Enum.GetValues(typeof(OpCode)))
            {
                if (!ReflectionUtils.InstructionCache.ContainsKey(op))
                {
                    entries.Add($"OpCode '{op}' does not have an associated instruction class.");
                }
                else
                {
                    entries.Add($"OpCode '{op}' is associated with the instruction class {InstructionCache[op].GetType().Name}.");
                }
            }

            entries.Add(new string('-', 100));

            Utils.WriteLogFile(logPath, true, entries.ToArray());
        }

        /// <summary>
        /// Clear any caches and hooks that have might
        /// have already been set up or created.
        /// </summary>
        public static void ClearCachesAndHooks()
        {
            InstructionCache.Clear();
            InterruptManager.Handlers.Clear();
            SocketDeviceManager.ReadSockets.Clear();
            SocketDeviceManager.WriteSockets.Clear();
        }

        /// <summary>
        /// Create an instance of an object by its type name.
        /// </summary>
        /// <param name="aTypeName">The instance type to be created.</param>
        /// <returns>An object giving a new instance of the type.</returns>
        public static object GetInstance(string aTypeName)
        {
            var type = (from t in ReflectionUtils.TypesCache
                        where t.Name == aTypeName
                        select t).FirstOrDefault();
            if (type == null)
            {
                throw new InvalidOperationException($"GetInstance: the type {aTypeName} could not be found.");
            }

            return Activator.CreateInstance(type);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Factories
{
    public static class Factory<TValue, TKey>
    {
        // exact delegate - instance producer
        public delegate TValue FactoryDelegate();

        private static readonly Dictionary<TKey, FactoryDelegate> CreationDelegates = new();

        /// <summary>
        /// Method used to register creation delegates for further use with <see cref="Create"/>.
        /// </summary>
        /// <param name="key">Unique key.</param>
        /// <param name="creationDelegate"> Delegate which returns instance based on <see cref="key"/></param>
        /// <exception cref="InvalidOperationException"> Thrown when key collision occurs.</exception>
        public static void Register(in TKey key, in FactoryDelegate creationDelegate)
        {
            if (!CreationDelegates.TryAdd(key, creationDelegate))
                throw new InvalidOperationException($"Creation delegate with key \"{key}\" already added");
        }

        /// <summary>
        /// Creates new instance based on <see cref="key"/>
        /// </summary>
        /// <param name="key"> Unique key identifying required type. </param>
        /// <returns> Instance of <see cref="TValue"/> derived type based on <see cref="key"/>. </returns>
        /// <exception cref="KeyNotFoundException"> Thrown when key not registered. </exception>
        public static TValue Create(in TKey key)
        {
            if (!CreationDelegates.TryGetValue(key, out var creationDelegate))
                throw new KeyNotFoundException($"Creation delegate with key \"{key}\" not found");

            return creationDelegate();
        }

        /// <summary>
        /// Tries to create new instance based on <see cref="key"/>
        /// </summary>
        /// <param name="key"> Unique key identifying required type. </param>
        /// <param name="result"> Instance of <see cref="TValue"/> derived type based on <see cref="key"/>.</param>
        /// <returns> Is creation was success. </returns>
        public static bool Create(in TKey key, out TValue result) {
            var found = CreationDelegates.TryGetValue(key, out var creationDelegate);
            result = found ? creationDelegate() : default;
            return found;
        }
    }
}

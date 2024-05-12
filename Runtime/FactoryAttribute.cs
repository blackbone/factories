using System;

namespace Factories {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class FactoryAttribute : Attribute {
        public Type BaseType { get; }
        public object Key { get; }

        public FactoryAttribute(Type baseType, object key)
        {
            BaseType = baseType;
            Key = key;
        }
    }
}
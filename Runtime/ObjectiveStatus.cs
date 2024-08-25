using System;
using Unity.Netcode;
using Unity.Collections;

namespace ObjectiveSystem
{
    public struct ObjectiveStatus : INetworkSerializable, IEquatable<ObjectiveStatus>
    {
        public FixedString128Bytes Id;
        public bool IsCompleted;

        public ObjectiveStatus(string id, bool isCompleted)
        {
            Id = id;
            IsCompleted = isCompleted;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref IsCompleted);
        }

        public bool Equals(ObjectiveStatus other)
        {
            return Id.Equals(other.Id) && IsCompleted == other.IsCompleted;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectiveStatus other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Id.GetHashCode();
                hash = hash * 31 + IsCompleted.GetHashCode();
                return hash;
            }
        }
    }
}
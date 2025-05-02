using _Project.Scripts.Network;
using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Items
{
    public class Gun1State : IState
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public bool Equipped;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref AngularVelocity);
            serializer.SerializeValue(ref Equipped);
        }

        static Gun1State() => StateFactory.Register((int) StateTypes.Gun,() => new Gun1State());
        
        public int GetStateType() => (int) StateTypes.Gun;
    }
}
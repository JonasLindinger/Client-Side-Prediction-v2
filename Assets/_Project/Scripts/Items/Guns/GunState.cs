using _Project.Scripts.Network;
using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace CSP.Items
{
    public class GunState : IState
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;
        public bool Equipped;
        public int CurrentBullets;
        public int MagazinesLeft;
        public float FireRateTimer;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref Rotation);
            serializer.SerializeValue(ref Velocity);
            serializer.SerializeValue(ref AngularVelocity);
            serializer.SerializeValue(ref Equipped);
            serializer.SerializeValue(ref CurrentBullets);
            serializer.SerializeValue(ref MagazinesLeft);
            serializer.SerializeValue(ref FireRateTimer);
        }

        static GunState() => StateFactory.Register((int) StateTypes.Gun,() => new GunState());
        
        public int GetStateType() => (int) StateTypes.Gun;
    }
}
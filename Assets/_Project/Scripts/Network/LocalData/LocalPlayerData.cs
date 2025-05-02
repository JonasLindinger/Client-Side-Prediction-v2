using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class LocalPlayerData : IData
    {
        public Vector2 PlayerRotation;
        public long ItemToDrop;
        public long ItemToPickUp;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerRotation);
            serializer.SerializeValue(ref ItemToDrop);
            serializer.SerializeValue(ref ItemToPickUp);
        }

        static LocalPlayerData() => DataFactory.Register((int) LocalDataTypes.LocalPlayer,() => new LocalPlayerData());

        public int GetDataType() => (int) LocalDataTypes.LocalPlayer;
    }
}
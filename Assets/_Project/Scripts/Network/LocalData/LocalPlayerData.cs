using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class LocalPlayerData : IData
    {
        public Vector2 PlayerRotation;
        public long ItemToPickUp;
        public bool DropItem;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerRotation);
            serializer.SerializeValue(ref ItemToPickUp);
            serializer.SerializeValue(ref DropItem);
        }

        static LocalPlayerData() => DataFactory.Register((int) LocalDataTypes.LocalPlayer,() => new LocalPlayerData());

        public int GetDataType() => (int) LocalDataTypes.LocalPlayer;
    }
}
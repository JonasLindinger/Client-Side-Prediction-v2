using CSP.Simulation;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class LocalPlayerData : IData
    {
        public Vector2 PlayerRotation; 
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerRotation);
        }

        static LocalPlayerData() => DataFactory.Register((int) DataTypes.LocalPlayer,() => new LocalPlayerData());

        public int GetDataType() => (int) DataTypes.LocalPlayer;
    }
}
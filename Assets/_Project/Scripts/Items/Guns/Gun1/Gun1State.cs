using _Project.Scripts.Network;
using CSP.Simulation;
using Unity.Netcode;

namespace CSP.Items
{
    public class Gun1State : IState
    {
        public bool Equipped;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Equipped);
        }

        public int GetStateType() => (int) StateTypes.Gun;
    }
}
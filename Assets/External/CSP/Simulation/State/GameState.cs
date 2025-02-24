using System.Collections.Generic;
using CSP.Simulation.State;
using Unity.Netcode;

namespace LindoNoxStudio.Network.Simulation
{
    public class GameState : INetworkSerializable
    {
        public uint Tick;
        public Dictionary<ulong, IState> States = new Dictionary<ulong, IState>(); // Id, data
        
        // Implement the NetworkSerialize method for both sending and receiving
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Serializing Tick
            serializer.SerializeValue(ref Tick);

            #region Dictionary
            if (serializer.IsWriter) // Sending side
            {
                int count = States.Count;
                serializer.SerializeValue(ref count); // Serialize the count

                foreach (var kvp in States)
                {
                    ulong networkId = kvp.Key;
                    IState state = kvp.Value;

                    // Serialize the networkId
                    serializer.SerializeValue(ref networkId);
        
                    // Serialize the state type
                    int stateType = state.GetStateType();
                    serializer.SerializeValue(ref stateType);

                    // Let the state serialize itself
                    state.NetworkSerialize(serializer);
                }
            }
            else // Receiving side
            {
                int count = 0;
                serializer.SerializeValue(ref count); // Read the count

                for (int i = 0; i < count; i++)
                {
                    ulong networkId = 0;
                    serializer.SerializeValue(ref networkId); // Read the networkId

                    int stateType = 0;
                    serializer.SerializeValue(ref stateType); // Read the state type

                    // Create an instance using a factory/registry
                    IState state = StateFactory.Create(stateType);
                    state.NetworkSerialize(serializer);
                    States[networkId] = state;
                }
            }
            #endregion
        }
    }
}
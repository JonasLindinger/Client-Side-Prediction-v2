using Unity.Netcode;

namespace CSP.Simulation.State
{
    public interface IState : INetworkSerializable
    {
        int GetStateType();
    }
}
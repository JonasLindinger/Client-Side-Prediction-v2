using Unity.Netcode;

namespace CSP.Simulation
{
    public interface IState : INetworkSerializable
    {
        int GetStateType();
    }
}
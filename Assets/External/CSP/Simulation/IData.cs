using Unity.Netcode;

namespace CSP.Simulation
{
    public interface IData : INetworkSerializable
    {
        int GetDataType();
    }
}
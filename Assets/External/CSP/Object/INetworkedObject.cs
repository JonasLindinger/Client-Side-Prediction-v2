using CSP.Simulation;

namespace CSP.Object
{
    public interface INetworkedObject
    {
        #if Client
        ulong NetworkObjectId { get; }
        
        void Register();
        IState GetCurrentState();
        void ApplyState(uint tick, IState state);
        #endif
    }
}
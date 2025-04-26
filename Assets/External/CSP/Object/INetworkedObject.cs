using CSP.Simulation;

namespace CSP.Object
{
    public interface INetworkedObject
    {
        ulong NetworkObjectId { get; }
        
        void Register();
        IState GetCurrentState();
        void ApplyState(IState state);
    }
}
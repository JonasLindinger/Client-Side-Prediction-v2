using Unity.Netcode;

namespace CSP.Simulation.State
{
    public abstract class NetworkedObject : NetworkBehaviour, INetworkedObject
    {
        protected virtual void Start()
        {
            Register();
        }
        
        public void Register()
        {
            SnapshotManager.RegisterNetworkedObject(NetworkObjectId, this);
        }

        public abstract IState GetCurrentState();
        public abstract void ApplyState(IState state);
        public abstract bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData);
    }
}
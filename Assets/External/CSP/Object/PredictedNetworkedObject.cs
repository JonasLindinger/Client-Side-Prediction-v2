using CSP.Simulation;

namespace CSP.Object
{
    public abstract class PredictedNetworkedObject : NetworkedObject
    {
        #if Client
        public bool canBeIgnored;
        #endif
        
        public abstract bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData);
    }
}
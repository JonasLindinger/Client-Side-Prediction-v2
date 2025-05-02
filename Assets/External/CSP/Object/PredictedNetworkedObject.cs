using CSP.Simulation;

namespace CSP.Object
{
    public abstract class PredictedNetworkedObject : NetworkedObject
    {
        public bool canBeIgnored;
        public abstract bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData);
    }
}
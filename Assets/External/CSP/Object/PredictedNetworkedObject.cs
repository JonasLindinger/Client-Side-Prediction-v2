using CSP.Simulation;
using UnityEngine;

namespace CSP.Object
{
    public abstract class PredictedNetworkedObject : NetworkedObject
    {
        [HideInInspector] public bool canBeIgnored;
        public abstract ReconciliationType DoWeNeedToReconcile(IState predictedStateData, IState serverStateData);
    }
}
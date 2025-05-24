using CSP.Simulation;
using UnityEngine;

namespace CSP.Object
{
    public abstract class PredictedNetworkedObject : NetworkedObject
    {
        [HideInInspector] public bool canBeIgnored;
        public abstract ReconciliationMethod DoWeNeedToReconcile(uint tick, IState predictedStateData, IState serverStateData);
    }
}
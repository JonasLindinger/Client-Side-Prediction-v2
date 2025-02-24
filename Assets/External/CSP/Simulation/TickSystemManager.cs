using _Project.Scripts.Utility;
using Singletons;
using UnityEngine;

namespace CSP.Simulation
{
    public class TickSystemManager : MonoBehaviourSingleton<TickSystemManager>
    {
        public static uint CurrentTick => GetInstance().physicsTickSystem.CurrentTick;
        public static uint PhysicsTickRate => (uint) GetInstance().physicsTickSystem.TickRate;
        public static uint NetworkTickRate => (uint) GetInstance().networkTickSystem.TickRate;
        
        [Header("Tick Systems")]
        [SerializeField] private PhysicsTickSystem physicsTickSystem;
        [SerializeField] private NetworkTickSystem networkTickSystem;
        [SerializeField] private TickAdjustmentTickSystem tickAdjustmentTickSystem;
        
        public void StartTickSystems(uint physicsTickRate, uint networkTickRate, uint tickAdjustmentTickRate, uint startingPhysicsTickSystem = 0)
        {
            physicsTickSystem.Run(physicsTickRate, startingPhysicsTickSystem + NetworkRunner.NetworkSettings.physicsTickClientOffset);
            networkTickSystem.Run(networkTickRate);
            
            #if Server
            tickAdjustmentTickSystem.Run(tickAdjustmentTickRate);
            #endif
        }
    }
}
using _Project.Scripts.Utility;
using CSP.Player;
using Singletons;
using UnityEngine;

namespace CSP.Simulation
{
    public class TickSystemManager : MonoBehaviourSingleton<TickSystemManager>
    {
        public static uint CurrentTick => GetInstance().physicsTickSystem.CurrentTick;
        public static uint PhysicsTickRate => (uint) GetInstance().physicsTickSystem.TickRate;
        public static float PhysicsTimeBetweenTicks => GetInstance().physicsTickSystem.TimeBetweenTicks;
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

        #if Client
        public static void RecalculatePhysicsTick(uint tick)
        {
            // 1. Simulate Physics
            Physics.Simulate(PhysicsTimeBetweenTicks);
            
            // 2. Update all Players (Server moves everyone, Client predicts his own player)
            PlayerInputBehaviour.UpdatePlayersWithAuthority(tick, true);
        }
        
        public void CalculateExtraTicks(int amount)
        {
            // Todo: Uncomment
            //physicsTickSystem.CalculateExtraTicks(amount);
            //networkTickSystem.CalculateExtraTicks(1);
        }

        public void SkipTicks(int amount)
        {
            // Todo: Uncomment
            //physicsTickSystem.SkipTick(amount);
        }
        #endif
    }
}
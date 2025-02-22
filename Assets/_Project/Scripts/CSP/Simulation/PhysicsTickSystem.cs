using UnityEngine;

namespace _Project.Scripts.CSP.Simulation
{
    public class PhysicsTickSystem : TickSystem
    {
        public override void OnTick(uint tick)
        {
            Debug.Log("OnPhysicsTick");
        }
    }
}
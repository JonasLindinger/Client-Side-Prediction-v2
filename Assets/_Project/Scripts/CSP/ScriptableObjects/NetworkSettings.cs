using UnityEngine;

namespace _Project.Scripts.CSP.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Scriptable Objects/CSP/Network Settings", fileName = "Network Settings")]
    public class NetworkSettings : ScriptableObject
    {
        [Header("Tick-System")] 
        public uint physicsTickRate = 64;
        public uint networkTickRate = 64;
    }
}
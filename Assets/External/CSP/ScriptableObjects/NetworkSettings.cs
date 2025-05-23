using UnityEngine;

namespace CSP.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Scriptable Objects/CSP/Network Settings", fileName = "Network Settings")]
    public class NetworkSettings : ScriptableObject
    {
        [Header("Tick-System")]
        public uint physicsTickRate = 64;
        public uint networkTickRate = 64;
        public uint inputBufferOnTheServer = 8;
        public uint tickOffsetBuffer = 5;
        public int maxColliderRollbackOffset = 20;
        [Space(10)] 
        [Header("Buffer")] 
        public uint inputBufferSize = 1024;
        public uint stateBufferSize = 1024;
        [Space(10)] 
        [Header("Connection")]
        public string defaultIp = "127.0.0.1";
        public ushort defaultPort = 7777;
    }
}
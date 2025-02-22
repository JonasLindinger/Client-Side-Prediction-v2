using Unity.Collections;

namespace _Project.Scripts.CSP.Connection.Data
{
    public struct ConnectionPayload
    {
        public FixedString32Bytes DisplayName;
        public ulong ClientId;
    }
}
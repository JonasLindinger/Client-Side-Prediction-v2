using System.Text;
using Unity.Collections;
using UnityEngine;

namespace _Project.Scripts.CSP.Data
{
    public struct ConnectionPayload
    {
        public FixedString32Bytes DisplayName;
        public ulong ClientId;

        #region Encoding
        
        private string GetAsJson()
        {
            return JsonUtility.ToJson(this);
        }

        public byte[] GetAsPayload()
        {
            return Encoding.UTF8.GetBytes(GetAsJson());
        }
        
        #endregion

        #region Decoding
        
        private void LoadFromJson(string json)
        {
            this = JsonUtility.FromJson<ConnectionPayload>(json);
        }
        
        public void LoadFromPayload(byte[] payload)
        {
            LoadFromJson(Encoding.UTF8.GetString(payload));
        }
        
        #endregion
    }
}
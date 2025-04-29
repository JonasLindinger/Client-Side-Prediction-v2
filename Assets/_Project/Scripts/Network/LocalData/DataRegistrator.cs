using UnityEngine;

namespace _Project.Scripts.Network
{
    public class DataRegistrator : MonoBehaviour
    {
        private void Start()
        {
            LocalPlayerData localPlayerData = new LocalPlayerData();
            Destroy(this);
        }
    }
}
using CSP.Items;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class DataRegistrator : MonoBehaviour
    {
        private void Start()
        {
            // Initializing states / data
            LocalPlayerData localPlayerData = new LocalPlayerData();
            Gun1State gun1State = new Gun1State();
            
            Destroy(this);
        }
    }
}
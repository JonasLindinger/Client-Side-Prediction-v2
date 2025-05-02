using CSP.Items;
using CSP.Simulation;
using UnityEngine;

namespace _Project.Scripts.Network
{
    public class DataRegistrator : MonoBehaviour
    {
        private void Start()
        {
            // Initializing states / data
            LocalPlayerData localPlayerData = new LocalPlayerData();
            PlayerState playerState = new PlayerState();
            Gun1State gun1State = new Gun1State();
            GameState gameState = new GameState();
            
            Destroy(this);
        }
    }
}
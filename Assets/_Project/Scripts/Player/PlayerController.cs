using CSP.Data;
using CSP.Player;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerController : PlayerInputBehaviour
    {
        public override void OnSpawn()
        {
            Debug.Log("Player Spawn");
        }

        public override void OnTick(ClientInputState input)
        {
            Debug.Log(input.DirectionalInputs["Move"].ToString());
        }

        public override void OnDespawn()
        {
            Debug.Log("Player Despawn");
        }
    }
}
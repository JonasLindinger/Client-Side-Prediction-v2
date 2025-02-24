using _Project.Scripts.Network.States;
using CSP.Data;
using CSP.Player;
using CSP.Simulation.State;
using UnityEngine;

namespace _Project.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : PlayerInputBehaviour
    {
        private Rigidbody _rb;
        
        public override void OnSpawn()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public override void OnTick(ClientInputState input)
        {
            Debug.Log(input.DirectionalInputs["Move"].ToString());
        }

        public override void OnDespawn()
        {
            
        }

        public override IState GetCurrentState()
        {
            return new PlayerState()
            {
                Position = transform.position,
                Rotation = transform.eulerAngles,
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity,
            };
        }

        public override void ApplyState(IState state)
        {
            // Return early if state is not PlayerState
            if (!(state is PlayerState playerState))
                return;

            transform.position = playerState.Position;
            transform.eulerAngles = playerState.Rotation;
            _rb.linearVelocity = playerState.Velocity;
            _rb.angularVelocity = playerState.AngularVelocity;
        }
    }
}
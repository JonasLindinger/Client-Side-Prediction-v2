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

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            PlayerState predictedState = (PlayerState) predictedStateData;
            PlayerState serverState = (PlayerState) serverStateData;
            
            // If our position is of, we reconcile
            if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.001f)
                return true;
            // If our rotation is off, we reconcile
            /* We don't do that (at least for now)
            else if (Vector3.Distance(clientState.Rotation, serverState.Rotation) >= 0.001f)
                return true;
            */
            // If our Velocity is of, we reconcile
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.01f)
                return true;
            // If our AngularVelocity is of, we reconcile
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.01f)
                return true;

            return false;
        }
    }
}
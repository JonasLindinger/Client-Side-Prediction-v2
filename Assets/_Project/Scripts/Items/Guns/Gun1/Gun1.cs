using _Project.Scripts.Items;
using CSP.Simulation;
using UnityEngine;

namespace CSP.Items
{
    public class Gun1 : PickUpItem
    {
        private Rigidbody _rb;
        
        protected override void SetUp()
        {
            _rb = GetComponent<Rigidbody>();
        }

        protected override void Use()
        {
            Debug.Log("Shooting");
        }

        protected override void OnPickedUp()
        {
            Debug.Log("Picked Up");
        }

        protected override void OnDropped()
        {
            Debug.Log("Dropped");
        }

        protected override void Highlight()
        {
            Debug.Log("Highlight");
        }

        protected override void UnHighlight()
        {
            Debug.Log("UnHighlight");
        }

        public override int GetItemType() => (int) ItemType.Gun;

        public override IState GetCurrentState()
        {
            Gun1State state = new Gun1State
            {
                Position = transform.position,
                Rotation = transform.eulerAngles,
                Velocity = _rb.linearVelocity,
                AngularVelocity = _rb.angularVelocity,
                Equipped = pickedUp
            };
            return state;
        }

        public override void ApplyState(IState state)
        {
            Gun1State gunState = (Gun1State)state;
            transform.position = gunState.Position;
            transform.eulerAngles = gunState.Rotation;
            _rb.linearVelocity = gunState.Velocity;
            _rb.angularVelocity = gunState.AngularVelocity;
            pickedUp = gunState.Equipped;
        }

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            Gun1State predictedState = (Gun1State) predictedStateData;
            Gun1State serverState = (Gun1State) serverStateData;
            
            // If our position is of, we reconcile
            if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.001f)
                return true;
            // If our rotation is off, we reconcile
            // We don't do that (at least for now)
            else if (Vector3.Distance(predictedState.Rotation, serverState.Rotation) >= 0.001f)
                return true;
            // If our Velocity is of, we reconcile
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.01f)
                return true;
            // If our AngularVelocity is of, we reconcile
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.01f)
                return true;
            else if (predictedState.Equipped != serverState.Equipped)
                return true;

            return false;
        }
    }
}
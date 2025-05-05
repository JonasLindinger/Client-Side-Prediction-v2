using _Project.Scripts.Items;
using CSP.Simulation;
using UnityEngine;

namespace CSP.Items
{
    public class Gun1 : PickUpItem
    {
        protected override void SetUp()
        {
            
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
                Velocity = rb.linearVelocity,
                AngularVelocity = rb.angularVelocity,
                Equipped = pickedUp
            };
            
            return state;
        }

        public override void ApplyState(uint tick, IState state)
        {
            if (rb.isKinematic) return;
            
            Gun1State gunState = (Gun1State)state;
            transform.position = gunState.Position;
            transform.eulerAngles = gunState.Rotation;
            rb.linearVelocity = gunState.Velocity;
            rb.angularVelocity = gunState.AngularVelocity;
            pickedUp = gunState.Equipped;
        }

        public override ReconciliationType DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            Gun1State predictedState = (Gun1State) predictedStateData;
            Gun1State serverState = (Gun1State) serverStateData;
            
            
            // Can't be ignored, because the player is engaged.
            if (predictedState.Equipped != serverState.Equipped)
            {
                return ReconciliationType.Everything;
            }
            
            
            // This can be ignored, because it isn't connected with other objects.
            else if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.01f)
            {
                return ReconciliationType.SingleObject;
            }
            else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.01f)
            {
                return ReconciliationType.SingleObject;
            }
            else  if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.001f)
            {
                return ReconciliationType.SingleObject;
            }
            else if (Vector3.Distance(predictedState.Rotation, serverState.Rotation) >= 0.001f)
            {
                return ReconciliationType.SingleObject;
            }

            return ReconciliationType.None;
        }
    }
}
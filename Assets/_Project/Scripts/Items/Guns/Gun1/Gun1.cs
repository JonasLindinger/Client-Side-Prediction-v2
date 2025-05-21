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
            
            if (predictedState.Equipped != serverState.Equipped)
            {
                Debug.LogWarning("Reconciliation Gun: Equipped");
                return ReconciliationType.Everything;
            }

            if (!serverState.Equipped)
            {
                if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Velocity");
                    return ReconciliationType.Everything;
                }
                else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Angular Velocity");
                    return ReconciliationType.Everything;
                }
                else  if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Position");
                    return ReconciliationType.Everything;
                }
                else if (Quaternion.Angle(Quaternion.Euler(predictedState.Rotation), Quaternion.Euler(serverState.Rotation)) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Rotation");
                    return ReconciliationType.Everything;
                }
            }

            return ReconciliationType.None;
        }
    }
}
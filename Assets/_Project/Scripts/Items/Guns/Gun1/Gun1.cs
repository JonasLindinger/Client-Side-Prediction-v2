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
            
        }

        protected override void OnDropped()
        {
            
        }

        protected override void Highlight()
        {
            
        }

        protected override void UnHighlight()
        {
            
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

        public override bool DoWeNeedToReconcile(IState predictedStateData, IState serverStateData)
        {
            Gun1State predictedState = (Gun1State) predictedStateData;
            Gun1State serverState = (Gun1State) serverStateData;
            
            if (predictedState.Equipped != serverState.Equipped)
            {
                Debug.LogWarning("Reconciliation Gun: Equipped");
                return true;
            }

            if (!serverState.Equipped)
            {
                if (Vector3.Distance(predictedState.Velocity, serverState.Velocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Velocity");
                    return true;
                }
                else if (Vector3.Distance(predictedState.AngularVelocity, serverState.AngularVelocity) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Angular Velocity");
                    return true;
                }
                else  if (Vector3.Distance(predictedState.Position, serverState.Position) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Position");
                    return true;
                }
                else if (Quaternion.Angle(Quaternion.Euler(predictedState.Rotation), Quaternion.Euler(serverState.Rotation)) >= 0.1f)
                {
                    Debug.LogWarning("Reconciliation Gun: Rotation");
                    return true;
                }
            }

            return false;
        }
    }
}
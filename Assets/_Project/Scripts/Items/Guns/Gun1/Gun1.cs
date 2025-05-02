using _Project.Scripts.Items;
using CSP.Simulation;
using UnityEngine;

namespace CSP.Items
{
    public class Gun1 : PickUpItem
    {
        protected override void SetUp()
        {
            Debug.Log("Set Up");
        }

        protected override void Use()
        {
            Debug.Log("Shooting");
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
                Equipped = Equipped
            };
            return state;
        }

        public override void ApplyState(IState state)
        {
            Gun1State gunState = (Gun1State)state;
            gunState.Equipped = Equipped;
        }
    }
}
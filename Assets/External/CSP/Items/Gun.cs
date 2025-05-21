using _Project.Scripts.Items;

namespace CSP.Items
{
    public abstract class Gun : PickUpItem
    {
        public override int GetItemType() => (int) ItemType.Gun;
    }
}
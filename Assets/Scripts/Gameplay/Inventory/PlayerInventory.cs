namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// MonoBehaviour wrapper around the pure <see cref="InventoryData"/>.
    /// Attach to the player. Consumables are used through <see cref="UseItem"/>;
    /// ammo is never consumed here (weapons spend it via InventoryData.SpendAmmo).
    /// </summary>
    public sealed class PlayerInventory : MonoBehaviour
    {
        /// <summary>Fired whenever the inventory contents change (add, use, drop).</summary>
        public event System.Action OnInventoryChanged;

        public InventoryData Data { get; private set; }

        private void Awake()
        {
            if (Data == null) Data = new InventoryData();
        }

        public bool AddItem(InventoryItem item)
        {
            if (Data == null) Data = new InventoryData();
            bool added = Data.Add(item);
            if (added) RaiseChanged();
            return added;
        }

        /// <summary>
        /// Uses the item in slot <paramref name="index"/> on <paramref name="target"/>.
        /// HealthPack heals 50, ShieldCell adds 50 shield; both are consumed.
        /// Ammo and weapons do nothing on use and are not consumed.
        /// Returns false for an invalid slot, a null target for a consumable,
        /// or an item kind that cannot be used this way.
        /// </summary>
        public bool UseItem(int index, HealthComponent target)
        {
            if (Data == null) return false;
            InventoryItem item = Data.Slot(index);
            if (item == null) return false;
            switch (item.Kind)
            {
                case ItemKind.HealthPack:
                    if (target == null || !target.IsAlive) return false;
                    target.Heal(50f);
                    Consume(index);
                    return true;
                case ItemKind.ShieldCell:
                    if (target == null || !target.IsAlive) return false;
                    target.AddShield(50f);
                    Consume(index);
                    return true;
                case ItemKind.Ammo:
                case ItemKind.WeaponBlaster:
                case ItemKind.WeaponMelee:
                default:
                    return false;
            }
        }

        public bool DropItem(int index)
        {
            if (Data == null) return false;
            bool removed = Data.RemoveAt(index);
            if (removed) RaiseChanged();
            return removed;
        }

        private void Consume(int index)
        {
            InventoryItem item = Data.Slot(index);
            if (item == null) return;
            item.Amount -= 1;
            if (item.Amount <= 0) Data.RemoveAt(index);
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            if (OnInventoryChanged != null) OnInventoryChanged();
        }
    }
}

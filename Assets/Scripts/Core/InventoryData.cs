namespace KobnexaRoyal
{
    using System;
    using System.Collections.Generic;

    public enum ItemKind { WeaponBlaster, WeaponMelee, Ammo, HealthPack, ShieldCell }

    [Serializable]
    public sealed class InventoryItem
    {
        public ItemKind Kind;
        public string Name;
        public int Amount;
    }

    /// <summary>Pure inventory: 8 slots, stackable consumables, one weapon slot each.</summary>
    public sealed class InventoryData
    {
        public const int Capacity = 8;
        private readonly List<InventoryItem> slots = new List<InventoryItem>();

        public int Count { get { return slots.Count; } }

        public InventoryItem Slot(int index)
        {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        public bool Add(InventoryItem item)
        {
            if (item == null) return false;
            if (item.Kind == ItemKind.Ammo || item.Kind == ItemKind.HealthPack || item.Kind == ItemKind.ShieldCell)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].Kind == item.Kind)
                    {
                        slots[i].Amount += item.Amount;
                        return true;
                    }
                }
            }
            if (slots.Count >= Capacity) return false;
            slots.Add(item);
            return true;
        }

        public bool RemoveAt(int index)
        {
            if (index < 0 || index >= slots.Count) return false;
            slots.RemoveAt(index);
            return true;
        }

        public int AmmoCount()
        {
            for (int i = 0; i < slots.Count; i++)
                if (slots[i].Kind == ItemKind.Ammo) return slots[i].Amount;
            return 0;
        }

        public bool SpendAmmo(int amount)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Kind == ItemKind.Ammo && slots[i].Amount >= amount)
                {
                    slots[i].Amount -= amount;
                    if (slots[i].Amount <= 0) slots.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}

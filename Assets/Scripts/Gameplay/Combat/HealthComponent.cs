namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Health + shield for players and bots. Shields absorb damage first
    /// (see CombatMath.ApplyDamage); shields regenerate after a damage-free
    /// delay. Death fires exactly once.
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        public float MaxHealth = 100f;
        public float MaxShield = 100f;
        public float Health;
        public float Shield;
        public float RegenDelay = 5f;
        public float RegenRate = 8f;

        /// <summary>Attacker name + damage dealt, fired on every damaging hit.</summary>
        public System.Action<string, float> OnDamaged;
        /// <summary>Attacker name of the killing blow, fired exactly once.</summary>
        public System.Action<string> OnDeath;

        /// <summary>Name passed to the most recent TakeDamage call ("" if none).</summary>
        public string LastAttacker { get; private set; }

        public bool IsAlive { get { return Health > 0f; } }

        private float timeSinceDamage;
        private bool deathFired;

        private void Awake()
        {
            if (Health <= 0f) Health = MaxHealth;
            Health = Mathf.Clamp(Health, 0f, MaxHealth);
            Shield = Mathf.Clamp(Shield, 0f, MaxShield);
            LastAttacker = string.Empty;
            timeSinceDamage = RegenDelay;
        }

        private void Update()
        {
            if (!IsAlive) return;
            if (Shield >= MaxShield) return;
            timeSinceDamage += Time.deltaTime;
            if (timeSinceDamage >= RegenDelay)
            {
                Shield = Mathf.Min(MaxShield, Shield + RegenRate * Time.deltaTime);
            }
        }

        public void TakeDamage(float damage, string attackerName)
        {
            if (!IsAlive) return;
            if (damage < 0f) damage = 0f;
            string attacker = attackerName ?? string.Empty;
            LastAttacker = attacker;
            float newHealth;
            float newShield;
            CombatMath.ApplyDamage(Health, Shield, damage, out newHealth, out newShield);
            Health = newHealth;
            Shield = newShield;
            timeSinceDamage = 0f;
            if (OnDamaged != null) OnDamaged(attacker, damage);
            if (Health <= 0f && !deathFired)
            {
                deathFired = true;
                if (OnDeath != null) OnDeath(attacker);
            }
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Health = Mathf.Min(MaxHealth, Health + amount);
        }

        public void AddShield(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Shield = Mathf.Min(MaxShield, Shield + amount);
        }
    }
}

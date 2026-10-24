namespace KobnexaRoyal
{
    /// <summary>Pure damage and zone math. Shields absorb first; overflow hits health.</summary>
    public static class CombatMath
    {
        public static void ApplyDamage(float health, float shield, float damage,
            out float newHealth, out float newShield)
        {
            if (damage < 0f) damage = 0f;
            float shieldAbsorbed = shield < damage ? shield : damage;
            newShield = shield - shieldAbsorbed;
            newHealth = health - (damage - shieldAbsorbed);
            if (newHealth < 0f) newHealth = 0f;
            if (newShield < 0f) newShield = 0f;
        }

        public static float ZoneRadiusAt(float elapsed, float shrinkDelay, float shrinkDuration,
            float startRadius, float endRadius)
        {
            if (elapsed <= shrinkDelay) return startRadius;
            float t = (elapsed - shrinkDelay) / shrinkDuration;
            if (t > 1f) t = 1f;
            if (t < 0f) t = 0f;
            return startRadius + (endRadius - startRadius) * t;
        }

        public static bool IsInsideZone(UnityEngine.Vector3 position, UnityEngine.Vector3 center, float radius)
        {
            float dx = position.x - center.x;
            float dz = position.z - center.z;
            return dx * dx + dz * dz <= radius * radius;
        }

        public static float ZoneDamagePerSecond(float elapsed)
        {
            return elapsed < 120f ? 2f : 5f;
        }
    }
}

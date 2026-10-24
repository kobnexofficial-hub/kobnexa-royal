namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Short-range melee swing: right mouse or F, 0.5s cooldown, damages every
    /// HealthComponent (except the wielder's own) inside a sphere in front of
    /// the wielder. Attach to the player.
    /// </summary>
    public class MeleeWeapon : MonoBehaviour
    {
        public float Damage = 40f;
        public float Range = 2.2f;
        public float Cooldown = 0.5f;

        private float nextSwingTime;

        private void Update()
        {
            // Only the active weapon responds to input: Y/Triangle (handled in
            // RoyalBlaster) swaps between blaster and melee. B/Circle swings
            // when melee is active; mouse/F still work as before.
            if (!WeaponMode.IsMelee) return;
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F) || ControllerInput.MeleePressed) Swing();
        }

        public void Swing()
        {
            if (Time.time < nextSwingTime) return;
            nextSwingTime = Time.time + Cooldown;

            Vector3 center = transform.position + transform.forward * Range * 0.5f;
            Collider[] hits = Physics.OverlapSphere(center, Range);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null) continue;
                if (hits[i].transform.root == transform.root) continue;
                HealthComponent health = hits[i].GetComponent<HealthComponent>();
                if (health == null) health = hits[i].GetComponentInParent<HealthComponent>();
                if (health == null) continue;
                health.TakeDamage(Damage, "Player");
                if (MatchManager.Instance != null &&
                    (health.GetComponent<BotController>() != null ||
                     health.GetComponentInParent<BotController>() != null))
                {
                    MatchManager.Instance.RegisterPlayerDamage(Damage);
                }
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();
        }
    }
}

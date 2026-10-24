namespace KobnexaRoyal
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// Hitscan blaster. Fires a ray from the main camera center on left mouse
    /// while CanFire is true, damages the first HealthComponent hit, and
    /// auto-reloads from reserve ammo when the magazine empties.
    /// No prefab required: attach to the player (or its camera rig).
    /// </summary>
    public class RoyalBlaster : MonoBehaviour
    {
        public int MagazineSize = 30;
        public int AmmoInMag;
        public int ReserveAmmo = 120;
        public float FireRate = 8f;
        public float Damage = 25f;
        public float Range = 80f;
        public float ReloadTime = 1.4f;
        public bool CanFire = true;

        /// <summary>Ammo values exposed for the HUD worker.</summary>
        public int AmmoMagDisplay { get { return AmmoInMag; } }
        public int AmmoReserveDisplay { get { return ReserveAmmo; } }
        public bool IsReloading { get; private set; }

        private float nextFireTime;

        private void Awake()
        {
            if (AmmoInMag <= 0) AmmoInMag = MagazineSize;
            AmmoInMag = Mathf.Clamp(AmmoInMag, 0, MagazineSize);
            if (ReserveAmmo < 0) ReserveAmmo = 0;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0)) Fire();
        }

        public void Fire()
        {
            if (!CanFire || IsReloading) return;
            if (Time.time < nextFireTime) return;
            if (AmmoInMag <= 0)
            {
                Reload();
                return;
            }
            float interval = FireRate > 0f ? 1f / FireRate : 0.1f;
            nextFireTime = Time.time + interval;
            AmmoInMag--;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Range))
                {
                    HealthComponent health = hit.collider.GetComponent<HealthComponent>();
                    if (health == null) health = hit.collider.GetComponentInParent<HealthComponent>();
                    if (health != null && health.transform.root != transform.root)
                    {
                        health.TakeDamage(Damage, "Player");
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();
                        if (MatchManager.Instance != null && health.GetComponent<BotController>() != null)
                        {
                            MatchManager.Instance.RegisterPlayerDamage(Damage);
                        }
                        else if (MatchManager.Instance != null && health.GetComponentInParent<BotController>() != null)
                        {
                            MatchManager.Instance.RegisterPlayerDamage(Damage);
                        }
                    }
                }
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayShot();
            if (AmmoInMag <= 0) Reload();
        }

        public void Reload()
        {
            if (IsReloading) return;
            if (AmmoInMag >= MagazineSize) return;
            if (ReserveAmmo <= 0) return;
            StartCoroutine(ReloadRoutine());
        }

        public void AddAmmo(int amount)
        {
            if (amount <= 0) return;
            ReserveAmmo += amount;
        }

        private IEnumerator ReloadRoutine()
        {
            IsReloading = true;
            yield return new WaitForSeconds(ReloadTime);
            int needed = MagazineSize - AmmoInMag;
            int taken = Mathf.Min(needed, ReserveAmmo);
            AmmoInMag += taken;
            ReserveAmmo -= taken;
            IsReloading = false;
        }
    }
}

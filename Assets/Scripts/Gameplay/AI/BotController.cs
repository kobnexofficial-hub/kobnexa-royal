namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Simple local AI opponent. Wanders to random points, seeks the player
    /// within 25 units, and attacks within 18 units (distance-only
    /// line-of-sight) for 8 damage every 0.8s. On death it notifies
    /// MatchManager and destroys itself after 1 second.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class BotController : MonoBehaviour
    {
        public enum BotState { Wander, Seek, Attack }

        public string BotName;
        public float WanderSpeed = 3.5f;
        public float SeekSpeed = 5f;
        public float SeekRange = 25f;
        public float AttackRange = 18f;
        public float AttackDamage = 8f;
        public float AttackInterval = 0.8f;
        public float WanderRadius = 60f;

        public BotState State { get; private set; }

        private HealthComponent health;
        private Transform playerTarget;
        private HealthComponent playerHealth;
        private Vector3 wanderTarget;
        private float wanderTimer;
        private float attackTimer;
        private bool isDead;
        private bool deathNotified;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            if (health != null)
            {
                health.OnDeath += HandleDeath;
            }
            EnsureBody();
            State = BotState.Wander;
            wanderTimer = 0f;
        }

        private void Start()
        {
            if (playerTarget == null) playerTarget = FindPlayerTransform();
            PickWanderTarget();
        }

        private static Transform FindPlayerTransform()
        {
            ThirdPersonPlayerController controller = FindObjectOfType<ThirdPersonPlayerController>();
            if (controller != null) return controller.transform;
            GameObject player = GameObject.Find("Player");
            return player != null ? player.transform : null;
        }

        /// <summary>Capsule body with a gold/dark material, created in code when none exists.</summary>
        private void EnsureBody()
        {
            if (GetComponent<Renderer>() == null && GetComponentInChildren<Renderer>() == null)
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "BotBody";
                Collider bodyCollider = body.GetComponent<Collider>();
                if (bodyCollider != null) Object.Destroy(bodyCollider);
                body.transform.SetParent(transform, false);
                // Bot root spawns at y=1, so a centered child sits feet-on-ground.
                body.transform.localPosition = Vector3.zero;
                Renderer renderer = body.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Shader shader = Shader.Find("Standard");
                    if (shader != null)
                    {
                        Material mat = new Material(shader);
                        mat.color = new Color(0.35f, 0.28f, 0.08f, 1f);
                        mat.SetColor("_EmissionColor", RoyalColors.RoyalGold * 0.35f);
                        mat.SetFloat("_Metallic", 0.7f);
                        mat.SetFloat("_Smoothness", 0.4f);
                        renderer.material = mat;
                    }
                }
            }
            if (GetComponent<CapsuleCollider>() == null)
            {
                CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
                collider.center = Vector3.zero;
                collider.height = 2f;
                collider.radius = 0.5f;
            }
        }

        public void SetPlayerTarget(Transform t)
        {
            playerTarget = t;
            playerHealth = null;
        }

        private void Update()
        {
            if (isDead) return;
            if (health != null && !health.IsAlive) return;
            if (playerTarget == null)
            {
                playerTarget = FindPlayerTransform();
                if (playerTarget == null) { Wander(); return; }
            }

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= AttackRange)
            {
                State = BotState.Attack;
                Attack(distance);
            }
            else if (distance <= SeekRange)
            {
                State = BotState.Seek;
                MoveTowards(playerTarget.position, SeekSpeed);
            }
            else
            {
                State = BotState.Wander;
                Wander();
            }
        }

        private void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f || Vector3.Distance(transform.position, wanderTarget) < 1f)
            {
                PickWanderTarget();
            }
            MoveTowards(wanderTarget, WanderSpeed);
        }

        private void PickWanderTarget()
        {
            Vector2 circle = Random.insideUnitCircle * WanderRadius;
            wanderTarget = new Vector3(circle.x, transform.position.y, circle.y);
            wanderTimer = Random.Range(4f, 8f);
        }

        private void Attack(float distance)
        {
            Vector3 flat = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
            if (flat != transform.position) transform.LookAt(flat);
            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;
            attackTimer = AttackInterval;
            if (playerHealth == null && playerTarget != null)
            {
                playerHealth = playerTarget.GetComponent<HealthComponent>();
                if (playerHealth == null) playerHealth = playerTarget.GetComponentInChildren<HealthComponent>();
                if (playerHealth == null) playerHealth = playerTarget.GetComponentInParent<HealthComponent>();
            }
            if (playerHealth != null && playerHealth.IsAlive)
            {
                playerHealth.TakeDamage(AttackDamage, BotName);
            }
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            Vector3 flat = new Vector3(target.x, transform.position.y, target.z);
            if (flat != transform.position) transform.LookAt(flat);
            transform.position = Vector3.MoveTowards(transform.position, flat, speed * Time.deltaTime);
        }

        private void HandleDeath(string attacker)
        {
            if (deathNotified) return;
            deathNotified = true;
            isDead = true;
            if (MatchManager.Instance != null) MatchManager.Instance.BotDied(this);
            Object.Destroy(gameObject, 1f);
        }
    }
}

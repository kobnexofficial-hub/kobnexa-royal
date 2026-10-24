namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Cinematic lobby camera: frames the hero on the platform with the castle
    /// behind, a slow orbital sway and subtle mouse parallax. Presentation only.
    /// </summary>
    public sealed class LobbyCameraController : MonoBehaviour
    {
        public Vector3 Target = new Vector3(0f, 2.4f, 0.6f);
        public float Distance = 10.5f;
        public float Height = 3.1f;
        public float SwayDegrees = 5f;
        public float SwaySpeed = 0.10f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.02f, 0.04f, 0.12f);
                cam.fieldOfView = 42f;
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 220f;
            }
        }

        private void LateUpdate()
        {
            float t = Time.time;
            float angle = Mathf.Sin(t * SwaySpeed) * SwayDegrees;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 mouse = new Vector2(0.5f, 0.5f);
            if (Screen.width > 0 && Screen.height > 0)
            {
                mouse = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            }
            float parallaxX = (mouse.x - 0.5f) * 1.4f;
            float parallaxY = (mouse.y - 0.5f) * 0.5f;

            Vector3 offset = new Vector3(Mathf.Sin(rad) * Distance + parallaxX, Height + parallaxY, -Mathf.Cos(rad) * Distance);
            transform.position = Target + offset;
            transform.LookAt(Target);
        }
    }
}

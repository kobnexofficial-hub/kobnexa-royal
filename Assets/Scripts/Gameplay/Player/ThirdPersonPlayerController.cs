namespace KobnexaRoyal
{
    using UnityEngine;

    /// <summary>
    /// Third-person character controller. CharacterController-based, no prefab
    /// required: a CharacterController is added in code if the GameObject lacks
    /// one. WASD movement is relative to the orbit camera's yaw, the mouse
    /// orbits the camera around the player, Space jumps, Shift sprints and
    /// Ctrl crouches (halved height and speed). Gravity and ground collision
    /// come from the CharacterController.
    /// </summary>
    public sealed class ThirdPersonPlayerController : MonoBehaviour
    {
        public float MoveSpeed = 6f;
        public float SprintSpeed = 10f;
        public float JumpHeight = 2f;
        public float CameraDistance = 6f;
        public float GravityStrength = 20f;

        public Vector3 Velocity { get; private set; }
        public bool IsGrounded { get; private set; }

        private CharacterController controller;
        private Camera playerCamera;
        private float yaw;
        private float pitch;
        private float verticalVelocity;
        private float standingHeight;
        private float standingCenterY;
        private float turnVelocity;

        private float Sensitivity
        {
            get
            {
                SettingsManager settings = SettingsManager.Instance;
                if (settings != null && settings.Data != null) return settings.Data.MouseSensitivity;
                return 1f;
            }
        }

        private bool InvertY
        {
            get
            {
                SettingsManager settings = SettingsManager.Instance;
                if (settings != null && settings.Data != null) return settings.Data.InvertY;
                return false;
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null) controller = gameObject.AddComponent<CharacterController>();
            standingHeight = controller.height > 0f ? controller.height : 2f;
            standingCenterY = controller.center.y;
            yaw = transform.eulerAngles.y;
            pitch = 15f;
        }

        private void Start()
        {
            playerCamera = Camera.main;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (controller == null) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            // Mouse-look orbit camera.
            float sensitivity = Sensitivity;
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
            pitch += InvertY ? mouseY : -mouseY;
            // Gamepad right-stick orbit camera (only when the gamepad was
            // used most recently - last-input switching, see ControllerInput).
            if (ControllerInput.IsGamepadActive)
            {
                float stickSpeed = 150f * ControllerInput.ControllerSensitivity * Time.deltaTime;
                yaw += ControllerInput.Camera.x * stickSpeed;
                float stickY = ControllerInput.Camera.y * stickSpeed;
                pitch += InvertY ? stickY : -stickY;
            }
            pitch = Mathf.Clamp(pitch, -80f, 80f);
            Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 cameraForward = cameraRotation * Vector3.forward;

            // Movement relative to camera yaw (horizontal plane only).
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");
            if (ControllerInput.IsGamepadActive)
            {
                inputX = ControllerInput.Move.x;
                inputZ = ControllerInput.Move.y;
            }
            Vector3 wish = forward * inputZ + right * inputX;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || ControllerInput.SprintHeld;
            bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || ControllerInput.CrouchHeld;
            float speed = sprinting ? SprintSpeed : MoveSpeed;
            if (crouching) speed *= 0.5f;

            if (crouching)
            {
                controller.height = standingHeight * 0.5f;
                controller.center = new Vector3(controller.center.x, standingCenterY * 0.5f, controller.center.z);
            }
            else
            {
                controller.height = standingHeight;
                controller.center = new Vector3(controller.center.x, standingCenterY, controller.center.z);
            }

            // Face the movement direction when moving.
            if (wish.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(wish.x, wish.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, 0.1f);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // Jump + gravity, grounded check from the CharacterController.
            IsGrounded = controller.isGrounded;
            if (IsGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                if (Input.GetKeyDown(KeyCode.Space) || ControllerInput.JumpPressed)
                    verticalVelocity = Mathf.Sqrt(2f * GravityStrength * JumpHeight);
            }
            else
            {
                verticalVelocity -= GravityStrength * Time.deltaTime;
            }

            Vector3 horizontal = wish * speed;
            Vector3 move = horizontal + Vector3.up * verticalVelocity;
            Velocity = move;
            controller.Move(move * Time.deltaTime);
            IsGrounded = controller.isGrounded;

            // Position the orbit camera behind the player.
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Vector3 eye = transform.position + Vector3.up * controller.height;
                Vector3 desired = eye - cameraForward * CameraDistance;
                if (desired.y < 0.5f) desired.y = 0.5f;
                playerCamera.transform.position = desired;
                playerCamera.transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }
    }
}

using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]

#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif

    public class ClikMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 5.335f;

        [Tooltip("How quickly the player accelerates/decelerates")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("How quickly the player rotates")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;


        [Header("Double Click Run")]

        [Tooltip("Maximum time between clicks to count as a double click")]
        public float DoubleClickTime = 0.3f;


        [Header("Movement Stopping")]

        [Tooltip("How close the player needs to get to the destination")]
        public float StopDistance = 0.15f;


        [Header("Click Settings")]

        [Tooltip("Layers that can be clicked to move")]
        public LayerMask ClickableLayers;

        [Tooltip("Prefab that appears where you click")]
        public GameObject ClickMarkerPrefab;

        [Tooltip("Height above the ground")]
        public float ClickMarkerHeight = 0.02f;


        [Header("Audio")]

        public AudioSource AudioFootsteps;
        public AudioSource LandingAudio;
        public AudioSource AudioFoley;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;

        [Range(0, 1)]
        public float FootstepAudioVolume = 0.5f;


        [Header("Jump")]

        public float JumpHeight = 1.2f;
        public float DoubleJumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;


        [Header("Grounded")]

        public bool Grounded = true;

        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;

        public LayerMask GroundLayers;


        [Header("Cinemachine")]

        public GameObject CinemachineCameraTarget;

        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;


        // =========================================================
        // PRIVATE VARIABLES
        // =========================================================

        // Camera
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;


        // Movement
        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;


        // Gravity
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;


        // Jump
        private bool _canDoubleJump;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;


        // Destination
        private Vector3 _targetPosition;
        private bool _hasDestination;


        // Running
        private bool _isRunning;
        private float _lastClickTime = -10f;


        // Click Marker
        private GameObject _currentClickMarker;


        // Animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif

        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;


        // =========================================================
        // AWAKE
        // =========================================================

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }


        // =========================================================
        // START
        // =========================================================

        private void Start()
        {
            if (CinemachineCameraTarget != null)
            {
                _cinemachineTargetYaw =
                    CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            }

            _hasAnimator = TryGetComponent(out _animator);

            _controller = GetComponent<CharacterController>();

            _input = GetComponent<StarterAssetsInputs>();

#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif

            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }


        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            HandleClickMovement();

            JumpAndGravity();

            GroundedCheck();

            Move();
        }


        // =========================================================
        // LATE UPDATE
        // =========================================================

        private void LateUpdate()
        {
            CameraRotation();
        }


        // =========================================================
        // CLICK TO MOVE
        // =========================================================

        private void HandleClickMovement()
        {
            bool clicked = false;

            Vector2 mousePosition = Vector2.zero;


#if ENABLE_INPUT_SYSTEM

            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                clicked = true;

                mousePosition =
                    Mouse.current.position.ReadValue();
            }

#else

            if (Input.GetMouseButtonDown(0))
            {
                clicked = true;

                mousePosition =
                    Input.mousePosition;
            }

#endif


            if (!clicked)
                return;


            // Make sure camera exists
            if (_mainCamera == null)
            {
                _mainCamera =
                    GameObject.FindGameObjectWithTag("MainCamera");
            }

            if (_mainCamera == null)
            {
                Debug.LogWarning(
                    "ClikMovement: No camera with the 'MainCamera' tag was found."
                );

                return;
            }


            Camera cameraComponent =
                _mainCamera.GetComponent<Camera>();


            if (cameraComponent == null)
            {
                Debug.LogWarning(
                    "ClikMovement: MainCamera does not have a Camera component."
                );

                return;
            }


            // Create ray from camera to mouse position
            Ray ray =
                cameraComponent.ScreenPointToRay(mousePosition);


            RaycastHit hit;


            // Check what we clicked
            if (Physics.Raycast(
                ray,
                out hit,
                1000f,
                ClickableLayers,
                QueryTriggerInteraction.Ignore))
            {
                // Set destination
                _targetPosition = hit.point;

                _hasDestination = true;


                // =================================================
                // DOUBLE CLICK
                // =================================================

                float timeSinceLastClick =
                    Time.time - _lastClickTime;


                if (timeSinceLastClick <= DoubleClickTime)
                {
                    _isRunning = true;
                }
                else
                {
                    _isRunning = false;
                }


                _lastClickTime = Time.time;


                // =================================================
                // CLICK MARKER
                // =================================================

                if (ClickMarkerPrefab != null)
                {
                    // Delete previous marker
                    if (_currentClickMarker != null)
                    {
                        Destroy(_currentClickMarker);
                    }


                    // Create new marker
                    _currentClickMarker =
                        Instantiate(
                            ClickMarkerPrefab,
                            hit.point +
                            Vector3.up * ClickMarkerHeight,
                            Quaternion.identity
                        );
                }
            }
        }


        // =========================================================
        // MOVEMENT
        // =========================================================

        private void Move()
        {
            Vector3 direction = Vector3.zero;


            // =====================================================
            // GET DIRECTION TO DESTINATION
            // =====================================================

            if (_hasDestination)
            {
                Vector3 target =
                    _targetPosition;


                // Ignore height difference
                target.y =
                    transform.position.y;


                direction =
                    target - transform.position;


                float distance =
                    direction.magnitude;


                // =================================================
                // DESTINATION REACHED
                // =================================================

                if (distance <= StopDistance)
                {
                    _hasDestination = false;

                    _isRunning = false;

                    _speed = 0f;


                    // Remove marker
                    if (_currentClickMarker != null)
                    {
                        Destroy(_currentClickMarker);

                        _currentClickMarker = null;
                    }
                }
                else
                {
                    direction.Normalize();
                }
            }


            // =====================================================
            // TARGET SPEED
            // =====================================================

            float targetSpeed = 0f;


            if (_hasDestination)
            {
                targetSpeed =
                    _isRunning
                        ? SprintSpeed
                        : MoveSpeed;
            }


            // =====================================================
            // CURRENT SPEED
            // =====================================================

            float currentHorizontalSpeed =
                new Vector3(
                    _controller.velocity.x,
                    0.0f,
                    _controller.velocity.z
                ).magnitude;


            float speedOffset = 0.1f;


            if (
                currentHorizontalSpeed <
                targetSpeed - speedOffset
                ||
                currentHorizontalSpeed >
                targetSpeed + speedOffset
            )
            {
                _speed =
                    Mathf.Lerp(
                        currentHorizontalSpeed,
                        targetSpeed,
                        Time.deltaTime *
                        SpeedChangeRate
                    );


                _speed =
                    Mathf.Round(
                        _speed * 1000f
                    ) / 1000f;
            }
            else
            {
                _speed =
                    targetSpeed;
            }


            // =====================================================
            // ANIMATION SPEED
            // =====================================================

            _animationBlend =
                Mathf.Lerp(
                    _animationBlend,
                    targetSpeed,
                    Time.deltaTime *
                    SpeedChangeRate
                );


            if (_animationBlend < 0.01f)
            {
                _animationBlend = 0f;
            }


            // =====================================================
            // ROTATE PLAYER
            // =====================================================

            if (
                _hasDestination &&
                direction != Vector3.zero
            )
            {
                float targetRotation =
                    Mathf.Atan2(
                        direction.x,
                        direction.z
                    ) * Mathf.Rad2Deg;


                float rotation =
                    Mathf.SmoothDampAngle(
                        transform.eulerAngles.y,
                        targetRotation,
                        ref _rotationVelocity,
                        RotationSmoothTime
                    );


                transform.rotation =
                    Quaternion.Euler(
                        0f,
                        rotation,
                        0f
                    );
            }


            // =====================================================
            // MOVE PLAYER
            // =====================================================

            Vector3 movement =
                direction *
                (_speed * Time.deltaTime);


            movement.y =
                _verticalVelocity *
                Time.deltaTime;


            _controller.Move(movement);


            // =====================================================
            // ANIMATOR
            // =====================================================

            if (_hasAnimator)
            {
                _animator.SetFloat(
                    _animIDSpeed,
                    _animationBlend
                );


                _animator.SetFloat(
                    _animIDMotionSpeed,
                    _hasDestination
                        ? 1f
                        : 0f
                );
            }
        }


        // =========================================================
        // JUMP + GRAVITY
        // =========================================================

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta =
                    FallTimeout;


                _canDoubleJump = true;


                if (_hasAnimator)
                {
                    _animator.SetBool(
                        _animIDJump,
                        false
                    );


                    _animator.SetBool(
                        _animIDFreeFall,
                        false
                    );
                }


                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity =
                        -2f;
                }


                // =================================================
                // NORMAL JUMP
                // =================================================

                if (
                    _input != null &&
                    _input.jump &&
                    _jumpTimeoutDelta <= 0.0f &&
                    JumpHeight > 0f
                )
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            JumpHeight *
                            -2f *
                            Gravity
                        );


                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            true
                        );
                    }


                    _input.jump = false;
                }


                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -=
                        Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta =
                    JumpTimeout;


                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -=
                        Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDFreeFall,
                            true
                        );
                    }
                }


                // =================================================
                // DOUBLE JUMP
                // =================================================

                if (
                    _input != null &&
                    _input.jump &&
                    _canDoubleJump &&
                    DoubleJumpHeight > 0f
                )
                {
                    _verticalVelocity =
                        Mathf.Sqrt(
                            DoubleJumpHeight *
                            -2f *
                            Gravity
                        );


                    _canDoubleJump = false;


                    if (_hasAnimator)
                    {
                        _animator.SetBool(
                            _animIDJump,
                            false
                        );


                        _animator.SetBool(
                            _animIDJump,
                            true
                        );


                        _animator.SetBool(
                            _animIDFreeFall,
                            false
                        );
                    }
                }


                if (_input != null)
                {
                    _input.jump = false;
                }
            }


            // =====================================================
            // GRAVITY
            // =====================================================

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity +=
                    Gravity *
                    Time.deltaTime;
            }
        }


        // =========================================================
        // GROUNDED CHECK
        // =========================================================

        private void GroundedCheck()
        {
            Vector3 spherePosition =
                new Vector3(
                    transform.position.x,
                    transform.position.y -
                    GroundedOffset,
                    transform.position.z
                );


            Grounded =
                Physics.CheckSphere(
                    spherePosition,
                    GroundedRadius,
                    GroundLayers,
                    QueryTriggerInteraction.Ignore
                );


            if (_hasAnimator)
            {
                _animator.SetBool(
                    _animIDGrounded,
                    Grounded
                );
            }
        }


        // =========================================================
        // CAMERA
        // =========================================================

        private void CameraRotation()
        {
            if (_input == null)
                return;


            if (
                _input.look.sqrMagnitude >=
                _threshold &&
                !LockCameraPosition
            )
            {
#if ENABLE_INPUT_SYSTEM

                float deltaTimeMultiplier =
                    IsCurrentDeviceMouse
                        ? 1.0f
                        : Time.deltaTime;

#else

                float deltaTimeMultiplier =
                    Time.deltaTime;

#endif


                _cinemachineTargetYaw +=
                    _input.look.x *
                    deltaTimeMultiplier;


                _cinemachineTargetPitch +=
                    _input.look.y *
                    deltaTimeMultiplier;
            }


            _cinemachineTargetYaw =
                ClampAngle(
                    _cinemachineTargetYaw,
                    float.MinValue,
                    float.MaxValue
                );


            _cinemachineTargetPitch =
                ClampAngle(
                    _cinemachineTargetPitch,
                    BottomClamp,
                    TopClamp
                );


            if (CinemachineCameraTarget != null)
            {
                CinemachineCameraTarget.transform.rotation =
                    Quaternion.Euler(
                        _cinemachineTargetPitch +
                        CameraAngleOverride,

                        _cinemachineTargetYaw,

                        0.0f
                    );
            }
        }


        // =========================================================
        // ANIMATION IDs
        // =========================================================

        private void AssignAnimationIDs()
        {
            _animIDSpeed =
                Animator.StringToHash(
                    "Speed"
                );


            _animIDGrounded =
                Animator.StringToHash(
                    "Grounded"
                );


            _animIDJump =
                Animator.StringToHash(
                    "Jump"
                );


            _animIDFreeFall =
                Animator.StringToHash(
                    "FreeFall"
                );


            _animIDMotionSpeed =
                Animator.StringToHash(
                    "MotionSpeed"
                );
        }


        // =========================================================
        // CURRENT INPUT DEVICE
        // =========================================================

#if ENABLE_INPUT_SYSTEM

        private bool IsCurrentDeviceMouse
        {
            get
            {
                return _playerInput != null &&
                       _playerInput.currentControlScheme ==
                       "KeyboardMouse";
            }
        }

#endif


        // =========================================================
        // UTILITIES
        // =========================================================

        private static float ClampAngle(
            float lfAngle,
            float lfMin,
            float lfMax
        )
        {
            if (lfAngle < -360f)
            {
                lfAngle += 360f;
            }


            if (lfAngle > 360f)
            {
                lfAngle -= 360f;
            }


            return Mathf.Clamp(
                lfAngle,
                lfMin,
                lfMax
            );
        }


        // =========================================================
        // AUDIO EVENTS
        // =========================================================

        private void OnFootstep(
            AnimationEvent animationEvent
        )
        {
            if (
                animationEvent.animatorClipInfo.weight >
                0.5f
            )
            {
                if (AudioFootsteps != null)
                {
                    AudioFootsteps.Play();
                }


                if (AudioFoley != null)
                {
                    AudioFoley.Play();
                }
            }
        }


        private void OnLand(
            AnimationEvent animationEvent
        )
        {
            if (
                animationEvent.animatorClipInfo.weight >
                0.5f
            )
            {
                if (LandingAudio != null)
                {
                    LandingAudio.Play();
                }
            }
        }


        // =========================================================
        // GIZMOS
        // =========================================================

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen =
                new Color(
                    0.0f,
                    1.0f,
                    0.0f,
                    0.35f
                );


            Color transparentRed =
                new Color(
                    1.0f,
                    0.0f,
                    0.0f,
                    0.35f
                );


            Gizmos.color =
                Grounded
                    ? transparentGreen
                    : transparentRed;


            Gizmos.DrawSphere(
                new Vector3(
                    transform.position.x,
                    transform.position.y -
                    GroundedOffset,
                    transform.position.z
                ),
                GroundedRadius
            );
        }
    }
}
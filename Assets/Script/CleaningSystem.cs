using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Animator spongeAnimator;
    [SerializeField] private GameObject spongeViewModel;
    [SerializeField] private Camera playerCamera;

    [Header("Settings / 设置")]
    [SerializeField] private string spongeToolName = "Sponge";
    [SerializeField] private float cleanRange = 2.5f;
    [SerializeField] private float cleanRadius = 0.3f;
    [SerializeField] private float followSpeed = 20f;
    [SerializeField] private float surfaceOffset = 0.05f;
    [SerializeField] private LayerMask cleanableLayer;

    [Header("SFX / 音效")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cleanStartSFX;
    [SerializeField] private AudioClip cleanLoopSFX;
    [SerializeField] private AudioClip cleanStopSFX;

    [Header("Input / 输入")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _cleanAction;
    private bool _spongeSelected = false;
    private bool _isCleaning = false;
    private AudioSource _loopSource;

    private UnityEngine.Vector3 _defaultLocalPos;
    private UnityEngine.Quaternion _defaultLocalRot;
    private bool _defaultSaved = false;

    private UnityEngine.Vector3 _lastCameraForward;
    private RaycastHit _lastHit;
    private bool _didHit = false;

    private static readonly int IsCleaningHash = Animator.StringToHash("IsCleaning");

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);
        }
        else
        {
            UnityEngine.Debug.LogError("[CleaningSystem] InputActionAsset not assigned!");
        }

        if (spongeViewModel != null)
            spongeViewModel.SetActive(false);

        if (cleanLoopSFX != null)
        {
            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.clip = cleanLoopSFX;
            _loopSource.loop = true;
            _loopSource.playOnAwake = false;
            if (audioSource != null)
            {
                _loopSource.volume = audioSource.volume;
                _loopSource.spatialBlend = audioSource.spatialBlend;
            }
        }
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        RadialMenuSystem.OnToolSelected += HandleToolSelected;
    }

    private void OnEnable() => _cleanAction?.Enable();
    private void OnDisable() => _cleanAction?.Disable();

    private void OnDestroy()
    {
        RadialMenuSystem.OnToolSelected -= HandleToolSelected;
    }

    private void HandleToolSelected(int index, string toolName)
    {
        _spongeSelected = (toolName == spongeToolName);
        _defaultSaved = false;

        if (spongeViewModel != null)
            spongeViewModel.SetActive(_spongeSelected);

        if (!_spongeSelected)
        {
            StopCleaning();
            if (spongeAnimator != null)
                spongeAnimator.SetBool(IsCleaningHash, false);
        }
    }

    private void Update()
    {
        if (!_spongeSelected) return;

        if (!_defaultSaved && spongeViewModel != null)
        {
            _defaultLocalPos = spongeViewModel.transform.localPosition;
            _defaultLocalRot = spongeViewModel.transform.localRotation;
            _defaultSaved = true;
            _lastCameraForward = playerCamera.transform.forward;
        }

        float lookDelta = UnityEngine.Vector3.Angle(playerCamera.transform.forward, _lastCameraForward);
        bool isMoving = lookDelta > 0.01f;
        _lastCameraForward = playerCamera.transform.forward;

        int mask = cleanableLayer.value == 0 ? ~0 : cleanableLayer.value;
        if (spongeViewModel != null)
            mask &= ~(1 << spongeViewModel.layer);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        _didHit = UnityEngine.Physics.Raycast(ray, out _lastHit, cleanRange, mask);

        UnityEngine.Debug.DrawRay(playerCamera.transform.position,
            playerCamera.transform.forward * cleanRange,
            _didHit ? UnityEngine.Color.green : UnityEngine.Color.red);

        if (_didHit && spongeViewModel != null)
        {
            UnityEngine.Vector3 targetPos = _lastHit.point + _lastHit.normal * surfaceOffset;

            UnityEngine.Quaternion targetRot = UnityEngine.Quaternion.FromToRotation(
                UnityEngine.Vector3.down, _lastHit.normal)
                * UnityEngine.Quaternion.Euler(90f, playerCamera.transform.eulerAngles.y, 0f);

            spongeViewModel.transform.position = UnityEngine.Vector3.Lerp(
                spongeViewModel.transform.position, targetPos, Time.deltaTime * followSpeed);
            spongeViewModel.transform.rotation = UnityEngine.Quaternion.Slerp(
                spongeViewModel.transform.rotation, targetRot, Time.deltaTime * followSpeed);
        }
        else if (spongeViewModel != null)
        {
            spongeViewModel.transform.localPosition = UnityEngine.Vector3.Lerp(
                spongeViewModel.transform.localPosition, _defaultLocalPos, Time.deltaTime * followSpeed);
            spongeViewModel.transform.localRotation = UnityEngine.Quaternion.Slerp(
                spongeViewModel.transform.localRotation, _defaultLocalRot, Time.deltaTime * followSpeed);
        }

        if (_didHit && _cleanAction != null && _cleanAction.IsPressed() && isMoving)
        {
            Collider[] hits = UnityEngine.Physics.OverlapSphere(_lastHit.point, cleanRadius);
            foreach (var col in hits)
            {
                DirtDecal decal = col.GetComponent<DirtDecal>();
                if (decal != null)
                    decal.StartFading();
            }
        }

        if (_cleanAction == null || spongeAnimator == null) return;

        if (_cleanAction.WasPressedThisFrame())
            StartCleaning();
        if (_cleanAction.WasReleasedThisFrame())
            StopCleaning();
    }

    private void StartCleaning()
    {
        if (_isCleaning) return;
        _isCleaning = true;
        spongeAnimator.SetBool(IsCleaningHash, true);
        if (audioSource != null && cleanStartSFX != null)
            audioSource.PlayOneShot(cleanStartSFX);
        _loopSource?.Play();
    }

    private void StopCleaning()
    {
        if (!_isCleaning) return;
        _isCleaning = false;
        if (spongeAnimator != null)
            spongeAnimator.SetBool(IsCleaningHash, false);
        _loopSource?.Stop();
        if (audioSource != null && cleanStopSFX != null)
            audioSource.PlayOneShot(cleanStopSFX);
    }
}
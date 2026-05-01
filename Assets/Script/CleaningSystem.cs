using UnityEngine;
using UnityEngine.InputSystem;

public class CleaningSystem : MonoBehaviour
{
    [Header("References / 引用")]
    [SerializeField] private Animator spongeAnimator;
    [SerializeField] private GameObject spongeViewModel;

    [Header("Settings / 设置")]
    [SerializeField] private string spongeToolName = "Sponge";

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

        // 单独一个 AudioSource 用于循环音效
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
        if (!_spongeSelected || _cleanAction == null || spongeAnimator == null) return;

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
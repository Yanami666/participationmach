using UnityEngine;
using UnityEngine.InputSystem;

public class SpongeCleaner : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _cleanAction;
    private bool _isCleaning = false;

    private void Awake()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player", throwIfNotFound: true);
            _cleanAction = map.FindAction("Clean", throwIfNotFound: true);
            _cleanAction.started += _ => _isCleaning = true;
            _cleanAction.canceled += _ => _isCleaning = false;
        }
    }

    private void OnEnable() => _cleanAction?.Enable();
    private void OnDisable() => _cleanAction?.Disable();

    private void OnTriggerStay(Collider other)
    {
        if (!_isCleaning) return;
        DirtDecal decal = other.GetComponent<DirtDecal>();
        if (decal != null) decal.StartFading();
    }
}
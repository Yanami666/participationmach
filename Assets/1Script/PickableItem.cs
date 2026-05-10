using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("Item Info / 物品信息")]
    [SerializeField] private string itemID = "item_default";
    [SerializeField] private string itemName = "Item";
    [SerializeField] private Sprite itemIcon;
    [SerializeField, TextArea] private string itemDescription = "";

    [Header("Pickup Settings / 拾取设置")]
    [SerializeField] private string interactPrompt = "[E] Pick Up";

    [Header("Throw Settings / 扔出设置")]
    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpwardForce = 2f;
    [SerializeField] private float throwTorque = 3f;

    private Rigidbody _rb;
    private Collider[] _colliders;
    private bool _isHeld = false;
    private UnityEngine.Vector3 _originalWorldScale;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public Sprite ItemIcon => itemIcon;
    public string ItemDescription => itemDescription;
    public string InteractPrompt => interactPrompt;
    public bool IsHeld => _isHeld;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _originalWorldScale = transform.lossyScale; // 记录初始世界 scale
    }

    public void OnPickedUp(Transform holdPoint)
    {
        _isHeld = true;
        if (_rb != null)
        {
            _rb.linearVelocity = UnityEngine.Vector3.zero;
            _rb.angularVelocity = UnityEngine.Vector3.zero;
            _rb.isKinematic = true;
        }
        SetCollidersEnabled(false);

        transform.SetParent(holdPoint, worldPositionStays: false);
        transform.localPosition = UnityEngine.Vector3.zero;
        transform.localRotation = UnityEngine.Quaternion.identity;

        // 反推 localScale 保持世界 scale 不变
        UnityEngine.Vector3 parentScale = holdPoint.lossyScale;
        transform.localScale = new UnityEngine.Vector3(
            _originalWorldScale.x / parentScale.x,
            _originalWorldScale.y / parentScale.y,
            _originalWorldScale.z / parentScale.z);
    }

    public void OnPlaced(UnityEngine.Vector3 position, UnityEngine.Vector3 surfaceNormal)
    {
        _isHeld = false;
        transform.SetParent(null);
        transform.position = position;
        transform.rotation = UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.up, surfaceNormal);
        transform.localScale = _originalWorldScale; // 还原原始 scale
        if (_rb != null)
        {
            _rb.linearVelocity = UnityEngine.Vector3.zero;
            _rb.angularVelocity = UnityEngine.Vector3.zero;
            _rb.isKinematic = false;
        }
        StartCoroutine(ReEnableColliders(0.1f));
    }

    public void OnDropped(UnityEngine.Vector3 throwDirection, bool isThrow = true)
    {
        _isHeld = false;
        transform.SetParent(null);
        transform.localScale = _originalWorldScale; // 还原原始 scale
        if (_rb != null)
        {
            _rb.isKinematic = false;
            if (isThrow)
            {
                UnityEngine.Vector3 force = throwDirection * throwForce
                                          + UnityEngine.Vector3.up * throwUpwardForce;
                _rb.AddForce(force, ForceMode.Impulse);
                _rb.AddTorque(UnityEngine.Random.insideUnitSphere * throwTorque, ForceMode.Impulse);
            }
        }
        StartCoroutine(ReEnableColliders(0.15f));
    }

    private void SetCollidersEnabled(bool state)
    {
        foreach (Collider col in _colliders)
            col.enabled = state;
    }

    private System.Collections.IEnumerator ReEnableColliders(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetCollidersEnabled(true);
    }
}
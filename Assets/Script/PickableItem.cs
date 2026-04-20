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
    }

    /// <summary>
    /// Place item on a surface at a specific position aligned to surface normal.
    /// 将物体放置到表面指定位置，朝向与法线对齐。
    /// </summary>
    public void OnPlaced(UnityEngine.Vector3 position, UnityEngine.Vector3 surfaceNormal)
    {
        _isHeld = false;
        transform.SetParent(null);

        // Position on surface / 放置到表面位置
        transform.position = position;

        // Align upward direction to surface normal / 将物体上方向对齐到表面法线
        transform.rotation = UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.up, surfaceNormal);

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
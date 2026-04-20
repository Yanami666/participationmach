using System;
using UnityEngine;

public class DirtDecal : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 2f;

    private Renderer _renderer;
    private Material _material;
    private bool _isFading = false;
    private float _alpha = 1f;

    public bool IsClean => _alpha <= 0f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
    }

    private void Update()
    {
        if (!_isFading) return;

        _alpha -= fadeSpeed * Time.deltaTime;
        _alpha = Mathf.Clamp01(_alpha);

        Color c = _material.color;
        c.a = _alpha;
        _material.color = c;

        if (_alpha <= 0f)
            gameObject.SetActive(false);
    }

    public void StartFading() => _isFading = true;
}
using UnityEngine;
using UnityEngine.UI;

public class UiAnim : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmplitude = 10f;
    [SerializeField] private float shakeFrequency = 20f;

    private RectTransform _rect;
    private Vector2 _originalPosition;
    private float _shakeTimer;
    private bool _isShaking;

    void Start()
    {
        _rect = GetComponent<RectTransform>();
        _originalPosition = _rect.anchoredPosition;
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[{gameObject.name}] Shake triggered");
            _isShaking = true;
            _shakeTimer = shakeDuration;
        }

        if (_isShaking)
        {
            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0f)
            {
                _rect.anchoredPosition = _originalPosition;
                _isShaking = false;
            }
            else
            {
                float decay = _shakeTimer / shakeDuration;
                float x = Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f;
                float y = Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f;
                _rect.anchoredPosition = _originalPosition + new Vector2(x, y) * shakeAmplitude * decay;
            }
        }
    }
}

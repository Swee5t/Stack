using UnityEngine;
using UnityEngine.UI;

public class CardShader : MonoBehaviour
{
    private CardVisual _cardVisual;
    private Image _image;
    private Material _material;

    private int _rotationID;

    private void Start()
    {
        _image = GetComponent<Image>();
        _material = new Material(_image.material);
        _image.material = _material;
        _cardVisual = GetComponentInParent<CardVisual>();
        _rotationID = Shader.PropertyToID("_Rotation");
    }

    private void Update()
    {
        if (!_cardVisual.initialized) return;

        var eulerAngles = _cardVisual.shakeTransform.rotation.eulerAngles + _cardVisual.tiltTransform.rotation.eulerAngles;

        var xAngle = eulerAngles.x;
        var yAngle = eulerAngles.y;

        xAngle = ClampAngle(xAngle, -90f, 90f);
        yAngle = ClampAngle(yAngle, -90f, 90f);

        _material.SetVector(_rotationID, new Vector2(Remap(xAngle, -20, 20, -.5f, .5f), Remap(yAngle, -20, 20, -.5f, .5f)));
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return Mathf.Clamp(angle, min, max);
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
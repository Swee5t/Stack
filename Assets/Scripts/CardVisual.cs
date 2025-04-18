using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    public bool initialized;

    public CardBase targetCard;
    public CardGroup targetCardGroup;

    public Transform cardTransform;
    public Transform tiltTransform;
    public Transform shakeTransform;

    public GameObject backgroundSprite;
    public GameObject foregroundSprite;
    public GameObject hologramSprite;

    [SerializeField] private float positionSpeed = 10f;
    [SerializeField] private float rotationSpeed = 20f;

    [SerializeField] private float rotationAmount = 20f;
    [SerializeField] private float autoTiltAmount = 10f;
    [SerializeField] private float manualTiltAmount = 20f;

    [SerializeField] private float springConstant = 800f;
    [SerializeField] private float springConstantOnDrag = 600f;
    [SerializeField] private float dampingFactor = 40f;
    [SerializeField] private float dampingFactorOnDrag = 25f;
    [SerializeField] private float maxSpringRotationAngle = 60f;

    [SerializeField] private float scaleOnHover = 1.05f;
    [SerializeField] private float scaleOnDrag = 1.15f;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.OutElastic;

    [SerializeField] private float shakeRotationAngle = 5f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private int shakeVibrato = 20;

    private Camera _camera;

    private Vector3 _positionVelocity;
    private Vector3 _rotationVelocity;
    
    private Vector3 _rotationDelta;
    
    private Tween _scaleTween;
    private Tween _shakeTween;
    
    private Coroutine _cardEffectRoutine;

    private void Awake()
    {
        shakeTransform = transform.Find("Shake");

        if (shakeTransform != null)
            tiltTransform = shakeTransform.Find("Tilt");

        if (tiltTransform != null)
        {
            backgroundSprite = tiltTransform.Find("Background").gameObject;
            foregroundSprite = tiltTransform.Find("Foreground").gameObject;
            hologramSprite = tiltTransform.Find("Hologram").gameObject;
        }

        if (_camera == null)
            _camera = Camera.main;
    }

    private void Update()
    {
        if (!initialized || ReferenceEquals(targetCard, null)) return;

        var deltaTime = Time.deltaTime;

        FollowPosition(deltaTime);
        FollowRotation(deltaTime);
        CardOffset(deltaTime);
        
        if (_cardEffectRoutine != null == targetCard.isInEffect) return;

        if (targetCard.isInEffect)
            _cardEffectRoutine = StartCoroutine(CardInEffect());
        else
        {
            StopCoroutine(_cardEffectRoutine);
            _cardEffectRoutine = null;
        }
    }

    private void OnDestroy()
    {
        _shakeTween?.Kill();
        _scaleTween?.Kill();
    }

    public void Initialize(CardBase card)
    {
        targetCard = card;
        targetCardGroup = targetCard.GetComponentInParent<CardGroup>();
        
        SetSprite(backgroundSprite, targetCard.backgroundSprite);
        SetSprite(foregroundSprite, targetCard.foregroundSprite);
        SetSprite(hologramSprite, targetCard.hologramSprite);
        
        cardTransform = card.transform;
        transform.position = cardTransform.position;
        transform.localScale = Vector3.one;
        
        initialized = true;
    }

    private void FollowPosition(float deltaTime)
    {
        var displacement = transform.position - cardTransform.position;

        var k = targetCard.isDragging ? springConstantOnDrag : springConstant;
        var d = targetCard.isDragging ? dampingFactorOnDrag : dampingFactor;

        var springForce = -k * displacement;
        var dampingForce = -d * _positionVelocity;
        var force = springForce + dampingForce;
        var acceleration = force / 1f;

        _positionVelocity += acceleration * deltaTime;
        transform.position += _positionVelocity * deltaTime;
    }

    private void FollowRotation(float deltaTime)
    {
        var movement = transform.position - cardTransform.position;
        var targetMovement = movement * rotationAmount;

        var displacement = _rotationDelta - targetMovement;

        var k = targetCard.isDragging ? springConstantOnDrag : springConstant;
        var d = targetCard.isDragging ? dampingFactorOnDrag : dampingFactor;

        var springForce = -k * displacement;
        var dampingForce = -d * _rotationVelocity;
        var force = springForce + dampingForce;
        var acceleration = force / 1f;

        _rotationVelocity += acceleration * deltaTime;
        _rotationDelta += _rotationVelocity * deltaTime;

        var overshoot = Mathf.Abs(_rotationDelta.x) - maxSpringRotationAngle;

        if (overshoot > 0f)
        {
            var pullBack = overshoot * 10f;
            _rotationVelocity.x -= Mathf.Sign(_rotationDelta.x) * pullBack * deltaTime;
        }

        var clampedZ = Mathf.Clamp(_rotationDelta.x, -maxSpringRotationAngle, maxSpringRotationAngle);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, clampedZ);
    }

    private void CardOffset(float deltaTime)
    {
        var time = Time.time + targetCard.ParentIndex() * 0.5f;
        var timeSine = Mathf.Sin(time);
        var timeCosine = Mathf.Cos(time);

        var center = (targetCardGroup.cards.Count - 1) / 2f;
        var normalizedOffset = center == 0 ? 0 : (targetCard.ParentIndex() - center) / center;

        var position = targetCard.isDragging ? Vector3.zero : Vector3.up * timeSine;

        tiltTransform.localPosition = Vector3.Lerp(tiltTransform.localPosition, position, positionSpeed * deltaTime);

        var offset = transform.position - _camera.ScreenToWorldPoint(Input.mousePosition);

        var rotationX = targetCard.isHovering ? offset.y * manualTiltAmount : timeSine * autoTiltAmount;
        var rotationY = targetCard.isHovering ? offset.x * manualTiltAmount : timeCosine * autoTiltAmount;
        var rotationZ = timeSine + normalizedOffset * -2f;

        var lerpX = Mathf.LerpAngle(tiltTransform.localEulerAngles.x, rotationX, rotationSpeed * deltaTime);
        var lerpY = Mathf.LerpAngle(tiltTransform.localEulerAngles.y, rotationY, rotationSpeed * deltaTime);
        var lerpZ = Mathf.LerpAngle(tiltTransform.localEulerAngles.z, rotationZ, rotationSpeed * deltaTime);

        tiltTransform.localEulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }
    
    private IEnumerator CardInEffect()
    {
        while (targetCard.isInEffect)
        {
            yield return new WaitForSeconds(1f);
            ShakeRotation(shakeDuration, shakeRotationAngle, shakeVibrato);
        }
    }

    public void OnEndDrag()
    {
        Scale(targetCard.isHovering ? scaleOnHover : 1f);
    }

    public void OnPointerEnter()
    {
        if (targetCardGroup.draggingCard != null)
            return;

        Scale(scaleOnHover);
        ShakeRotation(shakeDuration, shakeRotationAngle, shakeVibrato);
    }

    public void OnPointerExit()
    {
        if (targetCardGroup.draggingCard != targetCard)
            Scale(1f);
    }

    public void OnPointerUp()
    {
        Scale(targetCard.isHovering ? scaleOnHover : 1f);
    }

    public void OnPointerDown()
    {
        Scale(scaleOnDrag);
        ShakeRotation(shakeDuration, shakeRotationAngle, shakeVibrato);
    }

    private void Scale(float endValue)
    {
        _scaleTween?.Kill(true);
        _scaleTween = transform.DOScale(endValue, scaleDuration).SetEase(scaleEase);
    }

    private void ShakeRotation(float duration, float angle, int vibrato)
    {
        _shakeTween?.Kill(true);
        _shakeTween = shakeTransform.DOShakeRotation(duration, Vector3.one * angle, vibrato);
    }
    
    private static void SetSprite(GameObject obj, Sprite sprite)
    {
        var spriteRenderer = obj.GetComponent<Image>();
        if (sprite != null)
        {
            obj.SetActive(true);
            spriteRenderer.sprite = sprite;
        }
        else
        {
            obj.SetActive(false);
        }
    }
}
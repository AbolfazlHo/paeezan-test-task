using DG.Tweening;
using System;
using Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;

namespace GameCore
{
    /// <summary>Controls orbiting movement, scoring, collisions, shield use, and run-end rewards.</summary>
    public class BallController : MonoBehaviour
    {
        [SerializeField] private IntEvent OnScoreChanged;
        [SerializeField] private NullEvent OnGameEnded;
        [SerializeField] private NullEvent OnStartMoving;
        
        // برای رعایت ساختار کلی و احتمال استفاده در آینده اضافه شده است.
        [SerializeField] private IntEvent OnCurrencyEarned;
        
        [SerializeField] private ParticleSystem dieParticle;

        [SerializeField] private Transform center;
        [SerializeField] private float outsideRadius = 2f;
        [SerializeField] private float insideRadius = 1.3f;
        [SerializeField] private float moveTime = 0.2f;

        [Header("Rotation")]
        [SerializeField] private float speed = 360f;       
        [SerializeField] private float maxSpeed = 1200f;  
        [SerializeField] private float speedIncreaseRate = 50f; 

        [SerializeField] private int increasedScoreByPickingStars = 1;

        [Header("Death Animation")]
        [SerializeField] private float dieScaleTime = 0.5f;
        [SerializeField] private float dieRotationTime = 0.5f;
        [SerializeField] private float dieFadeTime = 0.5f;

        private int _score;
        private float angle;
        private float radius;
        private bool isInside;
        private bool canMove; 

        private SpriteRenderer _spriteRenderer;
        private Vector3 _firstLocalScale;
        private float _firstSpeed ;
        private Color _firstColor;
        private bool _hasActiveShield;

        /// <summary>Whether a shield is currently armed to absorb one obstacle collision.</summary>
        public bool HasActiveShield => _hasActiveShield;
        /// <summary>Raised when an armed shield absorbs an obstacle collision.</summary>
        public event Action ActiveShieldConsumed;

        private void Start()
        {
            _firstSpeed = speed;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _firstColor = _spriteRenderer.color;
            _firstLocalScale = transform.localScale;
            radius = outsideRadius* transform.parent.localScale.x;
        }

        private void OnEnable()
        {
            OnGameEnded.OnEventRaised += Die;
            OnGameEnded.OnEventRaised += EarnCurrency;
            OnStartMoving.OnEventRaised += StartMovement;
        }

        private void OnDisable()
        {
            OnGameEnded.OnEventRaised -= Die;
            OnGameEnded.OnEventRaised -= EarnCurrency;
            OnStartMoving.OnEventRaised -= StartMovement;
        }

        /// <summary>Enables movement and clears any shield left from a previous run.</summary>
        private void StartMovement()
        {
            _hasActiveShield = false;
            canMove = true;
        }

        /// <summary>Arms the one-hit shield if the ball is moving and has no active shield.</summary>
        public bool TryActivateShield()
        {
            if (!canMove || _hasActiveShield)
                return false;

            _hasActiveShield = true;
            if (_spriteRenderer != null)
                _spriteRenderer.color = new Color(0.55f, 0.9f, 1f, 1f);
            return true;
        }

        private void Update()
        {
            if (!canMove)
                return;

            if (speed < maxSpeed)
                speed = Mathf.Min(speed + speedIncreaseRate * Time.deltaTime, maxSpeed);

            angle += speed * Time.deltaTime;
            angle %= 360f;

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
            {
                AudioManger.AudioManager.Instance.PlaySFX(SoundType.Move);
                isInside = !isInside;
                float target = isInside ? insideRadius * transform.parent.localScale.x : outsideRadius* transform.parent.localScale.x;
                DOTween.To(() => radius, r => radius = r, target, moveTime)
                       .SetEase(Ease.OutBack);
            }

            float rad = angle * Mathf.Deg2Rad;
            transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
        }

        private static bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            if (Input.touchCount > 0)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    if (eventSystem.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                        return true;
                }
            }

            return eventSystem.IsPointerOverGameObject();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!canMove)
                return;

            if (other.CompareTag("Obsticle"))
            {
                if (_hasActiveShield)
                {
                    _hasActiveShield = false;
                    if (_spriteRenderer != null)
                        _spriteRenderer.color = _firstColor;
                    ActiveShieldConsumed?.Invoke();
                    return;
                }

                OnGameEnded.Raise();
            }
            else
            {
                _score += increasedScoreByPickingStars;
                OnScoreChanged.Raise(_score);
                AudioManger.AudioManager.Instance.PlaySFX(SoundType.Score);
            }
        }

        /// <summary>Stops the run, plays the death animation, and restores the ball for replay.</summary>
        private void Die()
        {
            canMove = false;
            _hasActiveShield = false;
            AudioManger.AudioManager.Instance.PlaySFX(SoundType.Explosion);
            speed = 0;

            if (dieParticle)
                dieParticle.Play();
            
            Sequence dieSequence = DOTween.Sequence();
            dieSequence.Append(transform.DOScale(Vector3.zero, dieScaleTime).SetEase(Ease.InBack));
            dieSequence.Join(transform.DORotate(new Vector3(0, 0, 720f), dieRotationTime, RotateMode.FastBeyond360));
            
            if (_spriteRenderer)
                dieSequence.Join(_spriteRenderer.DOFade(0f, dieFadeTime));

            dieSequence.OnComplete(() =>
            {
                float rad = 0f;
                transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
                
                gameObject.SetActive(false);
                DOTween.Kill(transform);
                
                if (_spriteRenderer)
                    _spriteRenderer.color = _firstColor;
                
                angle = 0f;
                radius = outsideRadius* transform.parent.localScale.x;
                transform.localScale = _firstLocalScale;
                isInside = false;
                canMove = false;
                speed = _firstSpeed;
                _score = 0;
            });
        }

        /// <summary>Converts the completed run's score into currency and saves the balance.</summary>
        private void EarnCurrency()
        {
            if (_score <= 0) return;
            var earnedCurrency = _score / 10;
            var totalCurrency = PlayerPrefsSaveService.Main.LoadInt("Currency");
            totalCurrency += earnedCurrency;
            PlayerPrefsSaveService.Main.SaveInt("Currency", totalCurrency);
            OnCurrencyEarned.Raise(totalCurrency);
        }
    }
    
}

using System.Collections;
using UnityEngine;
using DG.Tweening;
using Inventory;
using Core;

namespace Gameplay
{
    public class HarvestableResource : MonoBehaviour, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int harvestAmount = 3;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite fullSprite;
        [SerializeField] private Sprite emptySprite;

        [Header("Respawn Settings")]
        [SerializeField] private float respawnTime = 30f;

        [Header("Audio")]
        [SerializeField] private SoundEffectSO harvestSound;
        [SerializeField] private SoundEffectSO cooldownSound;

        private bool isHarvested;
        private bool isAnimating;
        private Vector3 originalPosition;
        private Vector3 originalScale;

        private Tween _activeTween;
        private Sequence _activeSequence;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (fullSprite == null && spriteRenderer != null)
                fullSprite = spriteRenderer.sprite;

            originalPosition = spriteRenderer.transform.localPosition;
            originalScale = spriteRenderer.transform.localScale;
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void KillAllTweens()
        {
            _activeTween?.Kill();
            _activeSequence?.Kill();

            if (spriteRenderer != null)
            {
                spriteRenderer.DOKill();
                spriteRenderer.transform.DOKill();
            }
        }

        private void ResetVisuals()
        {
            KillAllTweens();

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localPosition = originalPosition;
                spriteRenderer.transform.localScale = originalScale;

                var color = spriteRenderer.color;
                if (color.a < 1f)
                {
                    color.a = 1f;
                    spriteRenderer.color = color;
                }
            }
        }

        public bool IsFull => !isHarvested;

        public void InstantRegenerate()
        {
            if (IsFull) return;

            StopAllCoroutines();

            isHarvested = false;

            if (spriteRenderer != null && fullSprite != null)
            {
                spriteRenderer.sprite = fullSprite;
                ResetVisuals();
                PlayRespawnAnimation();
            }
        }

        public void Interact()
        {
            if (isAnimating) return;

            if (isHarvested)
            {
                PlayCooldownShake();
                return;
            }

            if (InventoryService.Instance == null || itemData == null)
            {
                Debug.LogWarning("[HarvestableResource] InventoryService or ItemData is null!");
                return;
            }

            var success = InventoryService.Instance.AddItem(itemData.Guid, harvestAmount);

            if (success)
            {
                Harvest();
            }
            else
            {
                Debug.Log("[HarvestableResource] Inventory is full!");
            }
        }

        public string GetInteractPrompt()
        {
            if (isHarvested)
                return "Empty";

            return itemData != null ? $"Harvest {itemData.itemName}" : "Harvest";
        }

        private void Harvest()
        {
            isHarvested = true;

            if (harvestSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(harvestSound);

            if (spriteRenderer != null && emptySprite != null)
                spriteRenderer.sprite = emptySprite;

            PlayHarvestSquash();
            StartCoroutine(RespawnTimer());
        }

        private IEnumerator RespawnTimer()
        {
            yield return new WaitForSeconds(respawnTime);

            while (isAnimating) yield return null;

            if (spriteRenderer != null && fullSprite != null)
            {
                spriteRenderer.sprite = fullSprite;
                PlayRespawnAnimation();
            }
        }

        private void PlayCooldownShake()
        {
            ResetVisuals();
            isAnimating = true;

#if UNITY_EDITOR
            Debug.Log("[HarvestableResource] Resource is empty, playing shake feedback.");
#endif

            if (cooldownSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(cooldownSound);

            _activeTween = spriteRenderer.transform.DOShakePosition(0.25f, new Vector3(0.08f, 0, 0), 30, 0, false)
                .OnComplete(() =>
                {
                    spriteRenderer.transform.localPosition = originalPosition;
                    isAnimating = false;
                });
        }

        private void PlayHarvestSquash()
        {
            ResetVisuals();
            isAnimating = true;

            var offsetY = spriteRenderer.bounds.size.y * 0.15f;

            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(spriteRenderer.transform.DOLocalMoveY(originalPosition.y - offsetY, 0.12f).SetEase(Ease.InQuad));
            _activeSequence.Join(spriteRenderer.transform.DOScaleY(originalScale.y * 0.7f, 0.12f).SetEase(Ease.InQuad));
            _activeSequence.Append(spriteRenderer.transform.DOLocalMoveY(originalPosition.y, 0.18f).SetEase(Ease.OutBack));
            _activeSequence.Join(spriteRenderer.transform.DOScaleY(originalScale.y, 0.18f).SetEase(Ease.OutBack));
            _activeSequence.OnComplete(() => isAnimating = false);
        }

        private void PlayRespawnAnimation()
        {
            ResetVisuals();
            isAnimating = true;

            var color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(spriteRenderer.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));
            _activeSequence.AppendInterval(0.1f);
            _activeSequence.Append(spriteRenderer.transform.DOPunchScale(originalScale * 0.15f, 0.4f, 5, 0.5f));
            _activeSequence.OnComplete(() =>
            {
                isHarvested = false;
                isAnimating = false;
            });
        }
    }
}

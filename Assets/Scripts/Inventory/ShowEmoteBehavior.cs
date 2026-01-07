using UnityEngine;
using Inventory;
using DG.Tweening;

public class ShowEmoteBehavior : ItemBehavior
{
    [Header("Emote Settings")]
    [SerializeField] private Sprite emoteSprite;
    [SerializeField] private Vector3 emoteOffset = new Vector3(0, 1.2f, 0);
    [SerializeField] private float riseDuration = 1f;
    [SerializeField] private float riseDistance = 0.5f;

    public override void OnUse(GameObject user)
    {
        if (emoteSprite != null)
        {
            ShowEmote(user.transform);
        }

#if UNITY_EDITOR
        Debug.Log("[ShowEmoteBehavior] Showing happy emote!");
#endif
    }

    private void ShowEmote(Transform playerTransform)
    {
        var emoteObj = new GameObject("HappyEmote");
        emoteObj.transform.SetParent(playerTransform);
        emoteObj.transform.localPosition = emoteOffset;
        emoteObj.transform.localScale = new Vector3(2.4f, 2.4f, 1);

        var spriteRenderer = emoteObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = emoteSprite;
        spriteRenderer.sortingOrder = 100;

        var startPos = emoteObj.transform.localPosition;
        var endPos = startPos + Vector3.up * riseDistance;

        var sequence = DOTween.Sequence();
        sequence.Append(emoteObj.transform.DOLocalMove(endPos, riseDuration).SetEase(Ease.OutQuad));
        sequence.Join(spriteRenderer.DOFade(0f, riseDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() => Destroy(emoteObj));
    }
}

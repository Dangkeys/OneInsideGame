using UnityEngine;
using UnityEngine.EventSystems;

public class CircleTarget : MonoBehaviour, IPointerEnterHandler
{
    public event System.Action<bool> onHit;
    [SerializeField] private float duration = 2f;
    private float currentTime = 0f;
    private void FixedUpdate()
    {
        if (currentTime > duration)
        {
            IsHit(false);
            currentTime = 0f;
        }
        currentTime += Time.fixedDeltaTime;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(eventData.pointerEnter != null)
        {
            IsHit(true);
            currentTime = 0f;
        }
    }

    private void IsHit(bool hit)
    {
        onHit?.Invoke(hit);
    }
}

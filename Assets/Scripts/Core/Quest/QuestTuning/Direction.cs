using UnityEngine;
using UnityEngine.UI;

public class Direction : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;
    public event System.Action<bool> OnConnect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void SetSpawn(Vector3 position)
    {
        rectTransform.anchoredPosition = position;
        image.color = Color.white;
    }

    public float GetHeight()
    {
        return rectTransform.rect.height;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        image.color = Color.black;
        OnConnect?.Invoke(true);
    }
}

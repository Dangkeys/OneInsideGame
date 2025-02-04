using UnityEngine;
using UnityEngine.UI;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float carbonLevel = 0f;
    [SerializeField] private float duration = 20f;
    [SerializeField] private GameObject oxygenBarGameObject;
    [SerializeField] private RectTransform oxygenRect;
    private Scrollbar oxygenBar;

    private void Awake()
    {
        oxygenBar = oxygenBarGameObject.GetComponent<Scrollbar>();
        oxygenBarGameObject.SetActive(true);
        SetOxygenBar();
    }

    private void Update()
    {
        if (oxygenBar == null)
            return;
        if (carbonLevel < maxOxygen)
        {
            carbonLevel += GetCarbonIncreaseSpeed();
            carbonLevel = Mathf.Min(carbonLevel, 100f);
            SetOxygenBar();
        }
    }

    private void SetOxygenBar()
    {
        oxygenRect.sizeDelta = new Vector2(oxygenRect.sizeDelta.x, (carbonLevel > 0) ? 0.1f : 0);
        oxygenBar.size = carbonLevel / maxOxygen;
    }

    public void IncreaseOxygen(float amount)
    {
        carbonLevel -= amount;
        if(carbonLevel < 0)
        {
            carbonLevel = 0;
        }
        SetOxygenBar();
    }

    public float GetCarbonIncreaseSpeed()
    {
        return (maxOxygen / duration) * Time.deltaTime;
    }
}

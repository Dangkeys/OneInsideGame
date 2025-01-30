using UnityEngine;
using UnityEngine.UI;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float notOxygen = 0f;
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
        if (notOxygen < maxOxygen)
        {
            notOxygen += GetOxygenSpeed();
            notOxygen = Mathf.Min(notOxygen, 100f);
            SetOxygenBar();
        }
    }

    private void SetOxygenBar()
    {
        oxygenRect.sizeDelta = new Vector2(oxygenRect.sizeDelta.x, (notOxygen > 0) ? 0.1f : 0);
        oxygenBar.size = notOxygen / maxOxygen;
    }

    public void IncreaseOxygen(float amount)
    {
        notOxygen -= amount;
        if(notOxygen < 0)
        {
            notOxygen = 0;
        }
        SetOxygenBar();
    }

    public float GetOxygenSpeed()
    {
        return (maxOxygen / duration) * Time.deltaTime;
    }
}

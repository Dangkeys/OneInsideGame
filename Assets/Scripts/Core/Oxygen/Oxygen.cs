using UnityEngine;
using UnityEngine.UI;

public class Oxygen : MonoBehaviour
{
    [SerializeField] private float maxOxygen = 100f;
    [SerializeField] private float oxygen = 0f;
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
        if (oxygen < maxOxygen)
        {
            oxygen += (maxOxygen / duration) * Time.deltaTime;
            oxygen = Mathf.Min(oxygen, 100f);
            SetOxygenBar();
        }
    }

    private void SetOxygenBar()
    {
        oxygenRect.sizeDelta = new Vector2(oxygenRect.sizeDelta.x, (oxygen > 0) ? 20 : 0);
        oxygenBar.size = oxygen / maxOxygen;
    }
}

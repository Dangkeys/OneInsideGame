using TMPro;
using UnityEngine;

public class ChangeDisplayText : MonoBehaviour
{
    private TMP_Text text;
    [SerializeField] private GameObject nameButton;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
        text.text = nameButton.name;
    }
}

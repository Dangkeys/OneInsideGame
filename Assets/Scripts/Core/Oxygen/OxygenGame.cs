using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OxygenGame : MonoBehaviour
{
    [SerializeField] private InputActionReference inputActionReference;
    private Oxygen oxygen;
    private RectTransform level;
    private TMP_Text oxygenText;
    private float speed = 0f;
    private float minSpeed;
    [SerializeField] private float amountGain = 0.1f;
    private List<string> wordList = new List<string>();
    private List<string> words = new List<string>();
    [SerializeField] private List<TMP_Text> buttonText;
    [SerializeField] private float timeChangeButton = 2f;
    private float currentTime = 0f;
    private void Awake()
    {
        oxygen = GetComponentInParent<Oxygen>();
        level = GameObject.Find("OxygenLever").GetComponent<RectTransform>();
        oxygenText = GameObject.Find("OxygenSpeed").GetComponent<TMP_Text>();
        minSpeed = oxygen.GetCarbonIncreaseSpeed();
        foreach (var binding in inputActionReference.action.bindings)
        {
            string[] pathSegments = binding.path.Split('/');
            string buttonName = pathSegments[pathSegments.Length - 1];
            wordList.Add(buttonName);
        }
        words.Add("A");
        words.Add("A");
    }

    private void OnEnable()
    {
        inputActionReference.action.performed += HandleClick;
        level.transform.eulerAngles = new Vector3(0, 0, 0);
        speed = 0f;
        currentTime = 0f;
        RandomButton();
    }

    private void OnDisable()
    {
        inputActionReference.action.performed -= HandleClick;
    }

    private void Update()
    {
        oxygen.IncreaseOxygen(speed);
        SetOxygenText();
        if (currentTime > timeChangeButton)
        {
            RandomButton();
            speed = (speed > 0) ? speed - amountGain : 0;
            currentTime = 0f;
        }
        else
        {
            currentTime += Time.deltaTime;
        }
    }

    private void HandleClick(InputAction.CallbackContext context)
    {
        string controlName = context.control.displayName.ToLower().Trim();

        if (words[0] == controlName)
        {
            speed += amountGain;
            level.transform.eulerAngles += new Vector3(0, 0, 5);
        }
        else if (words[1] == controlName)
        {
            speed = (speed > 0) ? speed - amountGain : 0;
            level.transform.eulerAngles += new Vector3(0, 0, -5);
        }
    }

    private void RandomButton()
    {
        int length = wordList.Count;
        int order1 = Random.Range(0, length);
        words[0] = wordList[order1];
        int order2;
        do
        {
            order2 = Random.Range(0, length);
        } while (order1 == order2);

        words[1] = wordList[order2];
        SetButtonText();
    }

    private void SetButtonText()
    {
        for (int i = 0; i < words.Count; i++)
        {
            buttonText[i].text = words[i];
        }
        
    }

    private void SetOxygenText()
    {
        if(speed < minSpeed / 2)
        {
            oxygenText.text = "Speed : Low";
        }
        else if(speed < minSpeed)
        {
            oxygenText.text = "Speed : Medium";
        }
        else
        {
            oxygenText.text = "Speed : High";
        }
    }
}

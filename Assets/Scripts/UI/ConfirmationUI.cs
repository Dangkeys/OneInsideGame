using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationUI : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI messageText;
    private OneInsideGameManager gameManager;

    private Action onConfirm;
    private Action onCancel;

    void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmClick);
        cancelButton.onClick.AddListener(OnCancelClick);

        gameManager = OneInsideGameManager.Instance;
        if (gameManager != null)
        {
            gameManager.UIManager.OnConfirmationRequired += ShowConfirmation;
        }
        gameObject.SetActive(false);
    }

    private void ShowConfirmation(string message, Action confirmAction, Action cancelAction)
    {
        messageText.text = message;
        onConfirm = confirmAction;
        onCancel = cancelAction;
        gameObject.SetActive(true);
    }

    private void OnConfirmClick()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
        ClearActions();
    }

    private void OnCancelClick()
    {
        gameObject.SetActive(false);
        onCancel?.Invoke();
        ClearActions();
    }

    private void ClearActions()
    {
        onConfirm = null;
        onCancel = null;
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.UIManager.OnConfirmationRequired -= ShowConfirmation;
        }

        confirmButton.onClick.RemoveListener(OnConfirmClick);
        cancelButton.onClick.RemoveListener(OnCancelClick);
    }
}
using System.Linq;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;


public class AudioDeviceSettings : MonoBehaviour
{
    public TMP_Dropdown InputDeviceDropdown;
    public TMP_Dropdown OutputDeviceDropdown;
    public Button BackButton;

    private void Start()
    {
        VivoxService.Instance.AvailableInputDevicesChanged += RefreshInputDeviceList;
        VivoxService.Instance.AvailableOutputDevicesChanged += RefreshOutputDeviceList;
        BackButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        InputDeviceDropdown.onValueChanged.AddListener((i) =>
        {
            InputDeviceValueChanged(i);
        });

        OutputDeviceDropdown.onValueChanged.AddListener((i) =>
        {
            OutputDeviceValueChanged(i);
        });
    }


    void OnEnable()
    {
        RefreshInputDeviceList();
        RefreshOutputDeviceList();
    }


    void OnDestroy()
    {
        // Unbind all UI actions
        InputDeviceDropdown.onValueChanged.RemoveAllListeners();
        VivoxService.Instance.AvailableInputDevicesChanged -= RefreshInputDeviceList;
        VivoxService.Instance.AvailableOutputDevicesChanged -= RefreshOutputDeviceList;
    }

    private void RefreshInputDeviceList()
    {
        InputDeviceDropdown.Hide();
        InputDeviceDropdown.ClearOptions();
        InputDeviceDropdown.options.AddRange(VivoxService.Instance.AvailableInputDevices.Select(v => new TMP_Dropdown.OptionData() { text = v.DeviceName }));
        InputDeviceDropdown.SetValueWithoutNotify(InputDeviceDropdown.options.FindIndex(option => option.text == VivoxService.Instance.ActiveInputDevice.DeviceName));
        InputDeviceDropdown.RefreshShownValue();
    }

    private void RefreshOutputDeviceList()
    {
        OutputDeviceDropdown.Hide();
        OutputDeviceDropdown.ClearOptions();
        OutputDeviceDropdown.options.AddRange(VivoxService.Instance.AvailableOutputDevices.Select(v => new TMP_Dropdown.OptionData() { text = v.DeviceName }));
        OutputDeviceDropdown.SetValueWithoutNotify(OutputDeviceDropdown.options.FindIndex(option => option.text == VivoxService.Instance.ActiveOutputDevice.DeviceName));
        OutputDeviceDropdown.RefreshShownValue();
    }

    void InputDeviceValueChanged(int index)
    {
        VivoxService.Instance.SetActiveInputDeviceAsync(VivoxService.Instance.AvailableInputDevices.Where(device => device.DeviceName == InputDeviceDropdown.options[index].text).First());
    }

    void OutputDeviceValueChanged(int index)
    {
        VivoxService.Instance.SetActiveOutputDeviceAsync(VivoxService.Instance.AvailableOutputDevices.Where(device => device.DeviceName == OutputDeviceDropdown.options[index].text).First());
    }
}
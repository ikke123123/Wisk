using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBLEManager : MonoBehaviour
{
    [SerializeField] private Button startSearch = null;
    [SerializeField] private Button stopSearch = null;
    [SerializeField] private TMP_InputField deviceName = null;

    private void Start()
    {
        deviceName.text = BLEManager.Instance.targetDeviceName = PlayerPrefs.GetString("DeviceName", "");
        SetButtons(true);
    }

    public void NameChanged()
    {
        BLEManager.Instance.targetDeviceName = deviceName.text;
        PlayerPrefs.SetString("DeviceName", "");
    }

    public void StartSearch()
    {
        SetButtons(false);
    }

    public void StopSearch()
    {
        SetButtons(true);
    }

    private void SetButtons(bool startSearchActive)
    {
        startSearch.gameObject.SetActive(startSearchActive);
        stopSearch.gameObject.SetActive(!startSearchActive);
    }
}

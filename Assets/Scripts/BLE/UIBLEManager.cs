using System.Collections;
using System.Collections.Generic;
using ThomasLib.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIBLEManager : MonoBehaviourBLECallbacks
{
    [SerializeField] private TMP_InputField deviceName = null;
    [SerializeField] private int sceneTarget = 1;

    private void Start()
    {
        deviceName.text = BLEManager.Instance.targetDeviceName = PlayerPrefs.GetString("DeviceName", "");
    }

    protected override void OnConnected()
    {
        SceneManager.LoadSceneAsync(sceneTarget);
    }

    public void NameChanged()
    {
        BLEManager.Instance.targetDeviceName = deviceName.text;
        PlayerPrefs.SetString("DeviceName", deviceName.text);
    }

    public void StartSearch()
    {
        BLEManager.Instance.StartDiscovery();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyInteraction : MonoBehaviour
{
    [SerializeField] private KeyAction[] keyActions = null;
    [Header("Debug")]
    [SerializeField] private bool active = false;

    public bool Active
    {
        get => active;
        set
        {
            if (value) OnEnable();
            else OnDisable();
        }
    }

    private Coroutine mainCoroutine = null;

    private void OnEnable()
    {
        if (mainCoroutine != null)
        {
            StopCoroutine(mainCoroutine);
            mainCoroutine = null;
        }
        foreach (KeyAction keyAction in keyActions)
        {
            keyAction.activeLastFrame = true;
        }
        mainCoroutine = StartCoroutine(MainCoroutine());
        active = true;
    }

    private void OnDisable()
    {
        if (mainCoroutine != null) StopCoroutine(mainCoroutine);
        mainCoroutine = null;
        active = false;
    }

    private IEnumerator MainCoroutine()
    {
        while (true)
        {
            foreach (KeyAction keyAction in keyActions)
            {
                if (keyAction.activeLastFrame == false && Input.GetAxis(keyAction.axis) > 0)
                {
                    keyAction.onKeyActivated.Invoke();
                }
                keyAction.activeLastFrame = Input.GetAxis(keyAction.axis) > 0;
            }
            yield return 0;
        }
    }

    [Serializable]
    private class KeyAction
    {
        public string axis = "";
        public UnityEvent onKeyActivated = null;
        [HideInInspector] public bool activeLastFrame = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour {

    private bool clicked = false;
    public UnityEvent clickEvent;
    public Color hoverColor;
    private EffectCamera cam;

    private Vector3 originalScale, targetScale;
    private bool hovering = false;
    private Vector3 hiddenScale = new Vector3(1.1f, 0f, 1f);

    public Image colorImage;

    void Awake()
    {
        originalScale = transform.localScale;
        cam = Camera.main.GetComponent<EffectCamera>();
    }

    public void OnMouseEnter()
    {
        //CursorManager.Instance.pointing = true;
        hovering = true;
        //AudioManager.Instance.PlayEffectAt(14, transform.position, 0.1f);
        colorImage.color = hoverColor;

        AudioManager.Instance.PlayEffectAt(5, transform.position, 0.5f);
    }

    public void OnMouseExit()
    {
        //CursorManager.Instance.pointing = false;
        hovering = false;
        //AudioManager.Instance.PlayEffectAt(14, transform.position, 0.04f);
        colorImage.color = Color.black;
    }

    public void OnMouseDown()
    {
    }

    public void OnMouseUp()
    {
        //      Manager.Instance.Calculate ();
        clicked = true;
        clickEvent.Invoke();

        AudioManager.Instance.PlayEffectAt(6, transform.position, 1f);

        ChangeVisibility(false);

        //AudioManager.Instance.PlayEffectAt(1, transform.position, 0.5f);
        //EffectManager.Instance.AddEffect(1, transform.position + Vector3.up * transform.lossyScale.y * 0.75f);

        cam.BaseEffect();
    }

    public void ChangeVisibility(bool visible)
    {
        targetScale = visible ? originalScale : hiddenScale;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboNode : MonoBehaviour {

    public Transform filling;
    public bool hasBonus = false;
    public GameObject button;

    Vector3 fullSize, buttonSize;
    bool currentState;

	// Use this for initialization
	void Awake () {
        buttonSize = button.transform.localScale;
        button.transform.localScale = Vector3.zero;

        fullSize = filling.localScale;
        filling.localScale = Vector3.zero;

        if (!hasBonus)
            button.SetActive(false);
	}
	
	public void Toggle(bool state, float delay = 0f)
    {
        currentState = state;

        if (state)
        {
            Tweener.Instance.ScaleTo(filling, fullSize, 0.3f, delay, TweenEasings.BounceEaseOut);

            if(hasBonus)
                Tweener.Instance.ScaleTo(button.transform, buttonSize, 0.3f, delay + 0.2f, TweenEasings.BounceEaseOut);

            AudioManager.Instance.PlayEffectAt(13, transform.position, 1f);
        }
        else
        {
            Tweener.Instance.ScaleTo(filling, Vector3.zero, 0.3f, delay, TweenEasings.QuadraticEaseOut);

            if(hasBonus)
                Tweener.Instance.ScaleTo(button.transform, Vector3.zero, 0.3f, delay + 0.2f, TweenEasings.QuadraticEaseOut);

            AudioManager.Instance.PlayEffectAt(8, transform.position, 0.5f);
        }
    }
}

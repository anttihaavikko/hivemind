using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverWindow : MonoBehaviour {

    public float slideVolume = 0.3f, bounceVolume = 0.6f;
    public Vector3 offPosition;
    public Vector3 soundPosition;
    Vector3 onPosition;

	// Use this for initialization
	void Awake () {
        onPosition = transform.localPosition;
        transform.localPosition = offPosition;
	}

    public void Show()
    {
        Tweener.Instance.MoveLocalTo(transform, onPosition, 1f, 0f, TweenEasings.BounceEaseOut);
        AudioManager.Instance.PlayEffectAt(11, soundPosition, slideVolume);
        StartCoroutine(DoWindowBounceSound(12, 0.3f, soundPosition, bounceVolume));
    }

    public void Hide()
    {
        Tweener.Instance.MoveLocalTo(transform, offPosition, 0.4f, 0f, TweenEasings.QuadraticEaseOut);
        AudioManager.Instance.PlayEffectAt(11, soundPosition, slideVolume);
    }

    IEnumerator DoWindowBounceSound(int soundIndex, float delayTime, Vector3 pos, float volume)
    {
        yield return new WaitForSeconds(delayTime);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume);
        yield return new WaitForSeconds(0.15f);
        AudioManager.Instance.PlayEffectAt(soundIndex, pos, volume - 0.1f);
    }
}

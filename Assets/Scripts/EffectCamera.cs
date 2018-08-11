using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class EffectCamera : MonoBehaviour
{

    private PostProcessingBehaviour filters;
    private float chromaAmount = 0f;
    private float chromaSpeed = 0.1f;

    private float shakeAmount = 0f, shakeTime = 0f;

    private Vector3 originalPos;

	private void Awake()
	{
        // set the desired aspect ratio (the values in this example are
        // hard-coded for 16:9, but you could make them into public
        // variables instead so you can set them at design time)
        float targetaspect = 16.0f / 9.0f;

        // determine the game window's current aspect ratio
        float windowaspect = (float)Screen.width / (float)Screen.height;

        // current viewport height should be scaled by this amount
        float scaleheight = windowaspect / targetaspect;

        // obtain camera component so we can modify its viewport
        Camera cam = GetComponent<Camera>();

        // if scaled height is less than current height, add letterbox
        if (scaleheight < 1.0f)
        {
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            cam.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = cam.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }
	}

	void Start()
    {
        filters = GetComponent<PostProcessingBehaviour>();
        originalPos = transform.position;
    }

    void Update()
    {

        // chromatic aberration update
        if (filters)
        {
            chromaAmount = Mathf.MoveTowards(chromaAmount, 0, Time.deltaTime * chromaSpeed);
            ChromaticAberrationModel.Settings g = filters.profile.chromaticAberration.settings;
            g.intensity = chromaAmount;
            filters.profile.chromaticAberration.settings = g;
        }

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            transform.position = originalPos + new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0);
        }
        else
        {
            transform.position = originalPos;
        }
    }

    public void Chromate(float amount, float speed)
    {
        chromaAmount = amount;
        chromaSpeed = speed;
    }

    public void Shake(float amount, float time)
    {
        shakeAmount = amount;
        shakeTime = time;
    }

    public void BaseEffect(float mod = 1f)
    {
        Shake(0.04f * mod, 0.075f * mod);
        Chromate(0.25f * mod, 0.1f * mod);
    }
}

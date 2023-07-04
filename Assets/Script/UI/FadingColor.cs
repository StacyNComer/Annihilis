using UnityEngine;

public abstract class FadingColor : MonoBehaviour
{
    [SerializeField]
    private float maxOpacity = 1;
    [SerializeField]
    private float fadeTime;
    [SerializeField]
    private float fadeDelay;
    [SerializeField]
    private bool fadeOnStart;
    [SerializeField]
    private bool destroyOnFade;

    private float fadeTracker;
    private float fadeDelayTracker;

    // Start is called before the first frame update
    protected void Start()
    {
        if(fadeOnStart)
        {
            PlayFade();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(fadeDelayTracker > 0)
        {
            fadeDelayTracker -= Time.deltaTime;

        } else if (fadeTracker > 0)
        {
            fadeTracker -= Time.deltaTime;

            var alpha = Mathf.Lerp(0, maxOpacity, fadeTracker/fadeTime);

            SetOpacity(alpha);

            if(destroyOnFade && fadeTracker <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Plays the fading animation for this gameObject's image component.
    /// </summary>
    public void PlayFade()
    {
        SetOpacity(maxOpacity);

        fadeDelayTracker = fadeDelay;
        fadeTracker = fadeTime;
    }

    /// <summary>
    /// Sets the fade delay. If the fade delay is currently being animated, this function will increase (or decrease!) the current time remaining in response to the new fade delay.
    /// </summary>
    public void SetFadeDelay(float newFadeDelay)
    {
        fadeDelay = newFadeDelay;

        if(fadeDelayTracker > 0)
        {
            fadeDelayTracker += newFadeDelay - fadeDelay;
        }
    }

    /// <summary>
    /// Instantly ends the images fade delay, if it is currently in the delay phase of the fade animation.
    /// </summary>
    public void SkipFadeDelay()
    {
        fadeDelayTracker = 0;
    }

    /// <summary>
    /// This is called as the script interpolates the alpha. Override this to control what changing the the alpha should actually affect.
    /// </summary>
    /// <param name="alpha"></param>
    protected abstract void SetOpacity(float alpha);
}

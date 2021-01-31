using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Tools.Tween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ease = Tools.Tween.Ease;

public class DelayedFadeOut : MonoBehaviour
{
    public VictoryCondition eventController;

    public Image fade;

    public float fadeDelay = 10;

    private bool fadeActivated = false;
    private bool fadeLaunched = false;
    
    // Start is called before the first frame update
    void Start()
    {
        eventController.RegisterNewEventListener( LaunchDelayedFadeOut );
    }

    private void Update()
    {
        if (fadeActivated && !fadeLaunched)
        {
            fadeLaunched = true;
            ColorTween.Create(
                this.GetHashCode().ToString(),
                new Color(0, 0, 0, 0),
                new Color(0, 0, 0, 255),
                fadeDelay * 100,
                Ease.Linear,
                (t) => { fade.color = t.Value; });
            StartCoroutine(GoToMainMenuAfterTime(fadeDelay));
        }
    }

    private void LaunchDelayedFadeOut()
    {
        fadeActivated = true;
    }
    
    private IEnumerator GoToMainMenuAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ColorTween.Delete(this.GetHashCode().ToString());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}

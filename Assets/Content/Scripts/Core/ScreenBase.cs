using UnityEngine;
using System.Collections;

public abstract class ScreenBase : MonoBehaviour
{
    public abstract void Initialize();
    public abstract IEnumerator AnimateShow();
    public abstract IEnumerator AnimateHide();
    
    protected ScreenManager ScreenManager => ScreenManager.Instance;

    public virtual IEnumerator AnimateFadeIn(CanvasGroup group, float duration)
    {
        group.alpha = 0;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        group.alpha = 1;
    }

    public virtual IEnumerator AnimateFadeOut(CanvasGroup group, float duration)
    {
        group.alpha = 1;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            group.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        group.alpha = 0;
    }
}
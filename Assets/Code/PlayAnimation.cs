using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class PlayAnimation : MonoBehaviour
{
	[SerializeField] float		animationSpeed = 1.0f;

// if animations are not working, make sure they're tagged as Legacy Animation in the Inspector / Debug view
    Animation animCtrl;
	
    void Start()
    {

        animCtrl = GetComponent<Animation>();
    }

	public void Play(string firstAnim, UnityEngine.Events.UnityAction onDone = null)
	{
		if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
		StartCoroutine(PlayCo(firstAnim, onDone));
	}

    protected IEnumerator PlayCo(string firstAnim, UnityEngine.Events.UnityAction onDone = null)
	{
		Debug.Log("Playing animation " + firstAnim);
		if (!animCtrl) 	yield break;
		if (!animCtrl.gameObject.activeInHierarchy) animCtrl.gameObject.SetActive(true);
		if (animCtrl.isPlaying)
		{
			if (animCtrl.clip)
			{
				animCtrl.clip.SampleAnimation(animCtrl.gameObject, animCtrl.clip.length);
			}
			animCtrl.Stop();
		}
		AnimationClip clip = animCtrl.GetClip(firstAnim);
		if (clip != null)
		{
			AnimationState state = animCtrl.PlayQueued(firstAnim);
			state.speed = animationSpeed;
            while (state != null && state.time < clip.length)
                yield return new WaitForEndOfFrame();			// force the final frame, just in case it encountered a stutter
			if (clip)	clip.SampleAnimation(animCtrl.gameObject, clip.length);
		} else 
		{
			Debug.Log("Invalid animation "+ firstAnim);
		}
        if (onDone!=null) onDone();
	}
}

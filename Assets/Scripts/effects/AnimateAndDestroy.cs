using UnityEngine;

public class AnimateAndDestroy : MonoBehaviour
{
  private Animator anim;
  [SerializeField] private const string ANIMATION_CLIP = "Effect";
  private float animationDuration = 1f;
  void Awake()
  {
    anim = GetComponent<Animator>();
    GetAnimationClipTimes();
    anim.Play(ANIMATION_CLIP);
    Destroy(gameObject, animationDuration);
  }
  private void GetAnimationClipTimes()
  {
    AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
    foreach(AnimationClip clip in clips)
    {
      switch(clip.name)
      {
        case ANIMATION_CLIP:
          animationDuration = clip.length;
          break;
      }
    }
  }
}

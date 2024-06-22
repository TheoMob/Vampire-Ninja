using System.Collections;
using TarodevController;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyCreatureDummy : EnemyCreature
{
  protected const string DUMMY_HIT_TEST = "DummyHitTest";
  protected const string DUMMY_HIT_ANIMATION_1 = "DummyHit1";
  protected const string DUMMY_HIT_ANIMATION_2 = "DummyHit2";
  protected const string DUMMY_HIT_ANIMATION_3 = "DummyHit3";

  [Header("damage and hit variables")]
  [SerializeField] private float timeToCountAsHitCombo = 1f;
  private int hitComboIndex;

  protected override void Awake()
  {
    base.Awake();
    hitComboIndex = 0;
  }

  protected override void Update()
  {
    base.Update();
  }
  protected override void OnCreatureHit()
  {
    hitComboIndex += 1;

    // if (hitComboIndex % 3 == 0)
    //   anim.Play(DUMMY_HIT_ANIMATION_2);
    // else
    //   anim.Play(DUMMY_HIT_ANIMATION_1);
    anim.Play(DUMMY_HIT_TEST);

    StartCoroutine(hitComboManager(hitComboIndex));
  }

  protected override void HandleAnimation()
  {
    //base.HandleAnimation();
  }

  IEnumerator hitComboManager(int initialComboIndex)
  {
    yield return new WaitForSeconds(timeToCountAsHitCombo);
    if (initialComboIndex == hitComboIndex) // if no new attacks happened, then break the combo of hits taken
    {
      hitComboIndex = 0;
    }
  }
}

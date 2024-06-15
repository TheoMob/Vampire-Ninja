using System.Collections;
using System.Collections.Generic;
using IPlayerState;
using static IPlayerState.PlayerStateController;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    [SerializeField] private float timeLimitToHitBossAtTheStart = 5f;
    private PlayerStateController _playerStateController;
    private AudioManager _audioManager;
    private GameObject _player;
    private GameObject boss;
    private BossTrigger _bossTrigger;
    private DoorsBossTrigger doorsScript;
    private Dialog dialogScript;
    private GameObject traps;

    private void Start()
    {
        _playerStateController = GameObject.FindWithTag("Player").GetComponent<PlayerStateController>();
        _audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();

        boss = transform.GetChild(0).gameObject;
        _bossTrigger = boss.GetComponent<BossTrigger>();
        doorsScript = transform.GetChild(1).GetComponent<DoorsBossTrigger>();
        dialogScript = transform.GetChild(2).GetComponent<Dialog>();
        traps = transform.GetChild(3).gameObject;
        InitializeTraps();
    }

    #region entering in the arena

    public bool alreadyTriggered = false;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (alreadyTriggered)
            return;

        if (other.CompareTag("Player") == false)
            return;
        
        if (_playerStateController.GetCurrentState() == PlayerState.Defeated)
            return;
        
        alreadyTriggered = true;
        doorsScript.MoveDoors(true);
        dialogScript.CallDialog();
        StartCoroutine(testIfFightStarted());
    }

    IEnumerator testIfFightStarted()
    {
        yield return new WaitForSeconds(timeLimitToHitBossAtTheStart);
        if (!_bossTrigger.bossHit && amountOfHitsDealthOnBoss == 0)
        {
            _bossTrigger.StartBossFightByTime();
        }
    }
    #endregion

    #region traps

    private ArrowTrap[] leftSideKunaisBottom;
    private ArrowTrap[] rightSideKunaisBottom;
    private ArrowTrap[] leftSideKunaisMiddle;
    private ArrowTrap[] rightSideKunaisMiddle;
    private ArrowTrap[] leftSideKunaisTop;
    private ArrowTrap[] rightSideKunaisTop;
    [SerializeField] private GameObject leftWarningBottom;
    [SerializeField] private GameObject rightWarningBottom;
    [SerializeField] private GameObject leftWarningMiddle;
    [SerializeField] private GameObject rightWarningMiddle;
    [SerializeField] private GameObject leftWarningTop;
    [SerializeField] private GameObject rightWarningTop;

    private void InitializeTraps()
    {
        GameObject trapCluster = traps.transform.GetChild(0).gameObject;
        leftSideKunaisBottom = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            leftSideKunaisBottom[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        }    

        trapCluster = traps.transform.GetChild(1).gameObject;
        rightSideKunaisBottom = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            rightSideKunaisBottom[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        } 

        trapCluster = traps.transform.GetChild(2).gameObject;
        leftSideKunaisMiddle = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            leftSideKunaisMiddle[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        }    

        trapCluster = traps.transform.GetChild(3).gameObject;
        rightSideKunaisMiddle = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            rightSideKunaisMiddle[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        }

        trapCluster = traps.transform.GetChild(4).gameObject;
        leftSideKunaisTop = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            leftSideKunaisTop[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        }    

        trapCluster = traps.transform.GetChild(5).gameObject;
        rightSideKunaisTop = new ArrowTrap[trapCluster.transform.childCount];
        for(int i = 0; i < trapCluster.transform.childCount; i++)
        {
            rightSideKunaisTop[i] = trapCluster.transform.GetChild(i).GetComponent<ArrowTrap>();
        }    
        
        // leftWarningBottom = traps.transform.GetChild(6).gameObject;
        // rightWarningBottom = traps.transform.GetChild(7).gameObject;
        // leftWarningMiddle = traps.transform.GetChild(8).gameObject;
        // rightWarningMiddle = traps.transform.GetChild(9).gameObject;
        // leftWarningTop = traps.transform.GetChild(10).gameObject;
        // rightWarningTop = traps.transform.GetChild(11).gameObject;

        leftWarningMiddle.SetActive(false);
        rightWarningMiddle.SetActive(false);
        leftWarningBottom.SetActive(false);
        rightWarningBottom.SetActive(false);
        leftWarningTop.SetActive(false);
        rightWarningTop.SetActive(false);
    }

    private void StopTrapsCoroutines()
    {
        foreach(ArrowTrap trapScript in leftSideKunaisBottom)
            trapScript.StopAllCoroutines();
        foreach(ArrowTrap trapScript in rightSideKunaisBottom)
            trapScript.StopAllCoroutines();
        foreach(ArrowTrap trapScript in leftSideKunaisMiddle)
            trapScript.StopAllCoroutines();
        foreach(ArrowTrap trapScript in rightSideKunaisMiddle)
            trapScript.StopAllCoroutines();
        foreach(ArrowTrap trapScript in leftSideKunaisTop)
            trapScript.StopAllCoroutines();
        foreach(ArrowTrap trapScript in rightSideKunaisTop)
            trapScript.StopAllCoroutines();

        leftWarningBottom.SetActive(false);
        leftWarningMiddle.SetActive(false);
        leftWarningTop.SetActive(false);
        rightWarningBottom.SetActive(false);
        rightWarningMiddle.SetActive(false);
        rightWarningTop.SetActive(false);
    }

    private void ShootLeftKunais(float shootForce, int orientationIndex)
    {
        switch(orientationIndex)
        {
            case 0:
                for(int i = 0; i < leftSideKunaisBottom.Length; i++)
                {
                    leftSideKunaisBottom[i].ForceActivation(shootForce);
                }
            break;

            case 1:
                for(int i = 0; i < leftSideKunaisMiddle.Length; i++)
                {
                    leftSideKunaisMiddle[i].ForceActivation(shootForce);
                }
            break;

            case 2:
                for(int i = 0; i < leftSideKunaisTop.Length; i++)
                {
                    leftSideKunaisTop[i].ForceActivation(shootForce);
                }
            break;
        }
    }
    private void ShootRightKunais(float shootForce, int orientationIndex)
    {
        switch(orientationIndex)
        {
            case 0:
                for(int i = 0; i < rightSideKunaisBottom.Length; i++)
                {
                    rightSideKunaisBottom[i].ForceActivation(shootForce);
                }
            break;

            case 1:
                for(int i = 0; i < rightSideKunaisMiddle.Length; i++)
                {
                    rightSideKunaisMiddle[i].ForceActivation(shootForce);
                }
            break;

            case 2:
                for(int i = 0; i < rightSideKunaisTop.Length; i++)
                {
                    rightSideKunaisTop[i].ForceActivation(shootForce);
                }
            break;
        }
    }

    #endregion

    [Header("AttackTimings")]
    [SerializeField] private float betweenAttacksDelay;

    private float timeBetweenAttacksMultiplier = 1f;
    private float projectileSpeedMultiplier = 1f;
    private float projectileSpeed = 15;

    private int amountOfHitsDealthOnBoss = 0;
    public void StartFight()
    {
        if (_bossTrigger.bossHit)
        {
            amountOfHitsDealthOnBoss = amountOfHitsDealthOnBoss + 1;
        }

        switch(amountOfHitsDealthOnBoss)
        {
            case 1:
                StartCoroutine(AttackOrder1());
            break;
            case 2:
                StartCoroutine(AttackOrder2());
                UnityEngine.Debug.Log("MAIS UM");
            break;
            case 3:
                StartCoroutine(EndGame());
            break;
        }
    }

    IEnumerator EndGame()
    {
        doorsScript.MoveDoors(false);
        _playerStateController.cantMove = true;
        yield return new WaitForSeconds(1f);

        GameObject endCanvas = GameObject.FindWithTag("EndCanvas");
        endCanvas.GetComponent<Image>().enabled = true;
        endCanvas.transform.GetChild(0).GetComponent<TextMeshProUGUI>().enabled = true;

        yield return new WaitForSeconds(5f);
    }

    IEnumerator AttackOrder1()
    {
        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        leftWarningBottom.SetActive(true);
        leftWarningTop.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        leftWarningBottom.SetActive(false);
        leftWarningTop.SetActive(false);

        ShootLeftKunais(projectileSpeed, 0);
        ShootLeftKunais(projectileSpeed, 2);
        

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        rightWarningBottom.SetActive(true);
        rightWarningTop.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        rightWarningBottom.SetActive(false);
        rightWarningTop.SetActive(false);

        ShootRightKunais(projectileSpeed, 0);
        ShootRightKunais(projectileSpeed, 2);

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        leftWarningBottom.SetActive(true);
        leftWarningTop.SetActive(true);
        rightWarningBottom.SetActive(true);
        rightWarningTop.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        leftWarningBottom.SetActive(false);
        leftWarningTop.SetActive(false);
        rightWarningBottom.SetActive(false);
        rightWarningTop.SetActive(false);

        ShootLeftKunais(projectileSpeed, 0);
        ShootLeftKunais(projectileSpeed, 2);
        ShootRightKunais(projectileSpeed, 0);
        ShootRightKunais(projectileSpeed, 2);

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier * 1.2f);

        _bossTrigger.BossAppear();
    }

    IEnumerator AttackOrder2()
    {
        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        leftWarningBottom.SetActive(true);
        leftWarningMiddle.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        leftWarningBottom.SetActive(false);
        leftWarningMiddle.SetActive(false);
        ShootLeftKunais(projectileSpeed, 0);
        ShootLeftKunais(projectileSpeed, 1);

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        rightWarningBottom.SetActive(true);
        rightWarningMiddle.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        rightWarningBottom.SetActive(false);
        rightWarningMiddle.SetActive(false);
        ShootRightKunais(projectileSpeed, 0);
        ShootRightKunais(projectileSpeed, 1);

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier);

        leftWarningBottom.SetActive(true);
        leftWarningMiddle.SetActive(true);
        rightWarningBottom.SetActive(true);
        rightWarningMiddle.SetActive(true);
        _audioManager.Play("Warning", false, Vector2.zero);

        yield return new WaitForSeconds(0.5f);

        leftWarningBottom.SetActive(false);
        leftWarningMiddle.SetActive(false);
        rightWarningBottom.SetActive(false);
        rightWarningMiddle.SetActive(false);
        ShootLeftKunais(projectileSpeed, 0);
        ShootLeftKunais(projectileSpeed, 1);
        ShootRightKunais(projectileSpeed, 0);
        ShootRightKunais(projectileSpeed, 1);

        yield return new WaitForSeconds(betweenAttacksDelay * timeBetweenAttacksMultiplier * 1.2f);

        _bossTrigger.BossAppear();
    }

    private int deathCounter = 0;
    public void ResetBossFight()
    {
        alreadyTriggered = false;
        amountOfHitsDealthOnBoss = 0;
        StopAllCoroutines();
        StopTrapsCoroutines();
        doorsScript.ResetDoors();
        _bossTrigger.ResetBoss();
        dialogScript.dialogAlreadyPlayed = false;
       // deathCounter = deathCounter + 1;

       // dialogScript.skipDialog = true;
    }

}

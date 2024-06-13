// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class EncounterManager : MonoBehaviour
// {
//     public DialogScriptableObject dialogData;

//     private GameObject player;
//     private Animator playerAnim;
//     private PlayerManager playerManagerScript;
//     private Rigidbody2D playerRigid;
//     private string eventText;
//     public GameObject dialogBox;
//     private Text dialogBoxText;
//     private Text[] dialogOptionsText = new Text[5];
//     private string[] dialogOptions = new string[5];
//     private float delayTextTime = 0.08f;

//     private bool movePlayer;
//     public Vector2 moveTo;
//     private float speed = 5;
//     private float minDist = 1; // isso serve pra movimentar o player
//     [HideInInspector] public int dialogOption = 0;
//     private int dialogIndex = 0;


//     public MyEnum Encounter = new MyEnum();
//     public enum MyEnum
//     {
//         none,
//         Skeleted,
//         test
//     };

//     void Start()
//     {
//         player = GameObject.FindGameObjectWithTag("Player");
//         playerAnim = player.GetComponent<Animator>();
//         playerManagerScript = player.GetComponent<PlayerManager>();
//         playerRigid = player.GetComponent<Rigidbody2D>();
//         dialogBoxText = dialogBox.transform.GetChild(0).GetComponent<Text>();

//         GameObject dialogoptions = dialogBox.transform.GetChild(1).gameObject;
//         for(int i = 0; i < 4; i++)
//         {
//             dialogOptionsText[i] = dialogoptions.transform.GetChild(i).GetComponent<Text>(); // isso armazena o objeto no qual s�o escritas as op��es de resposta
//         }
       
//         dialogBox.SetActive(false);

//         movePlayer = true;

//     }


//     void Update()
//     {
//         if(movePlayer)
//         {
//             player.transform.position = new Vector2(Mathf.MoveTowards(player.transform.position.x, moveTo.x, (speed * Time.deltaTime)), player.transform.position.y);
//             playerAnim.SetBool("Running", true);

//             if (Vector2.Distance(player.transform.position, moveTo) < minDist)
//             {
//                 movePlayer = false;
//                 playerAnim.SetBool("Running", false);
//                 dialogs();
//             }
//         } 
//     }

//     private void dialogs ()
//     {
//         if (dialogIndex == 99)
//             return;

//         dialogBox.SetActive(true);

//         eventText = dialogData.DialogAmount[dialogIndex].DialogBox;
//         dialogOptions[0] = dialogData.DialogAmount[dialogIndex].Answer1;
//         dialogOptions[1] = dialogData.DialogAmount[dialogIndex].Answer2;
//         dialogOptions[2] = dialogData.DialogAmount[dialogIndex].Answer3;
//         dialogOptions[2] = dialogData.DialogAmount[dialogIndex].Answer4;

//         StartCoroutine("TypeWriterEffect", dialogIndex);

//     }

//     IEnumerator TypeWriterEffect( int dialogIndex)
//     {
//         for (int i = 0; i < eventText.Length; i++)
//         {
//             dialogBoxText.text = eventText.Substring(0, i + 1);
//             yield return new WaitForSeconds(delayTextTime);
//         }
//         for (int j = 0; j < dialogData.DialogAmount[dialogIndex].answersAmount; j++)
//         {
//             for (int i = 0; i < dialogOptions[j].Length; i++)
//             {
//                 dialogOptionsText[j].text = dialogOptions[j].Substring(0, i + 1);
//                 yield return new WaitForSeconds(delayTextTime);
//             }
//         }

//         StartCoroutine("DialogSelection");
//     }

//     IEnumerator DialogSelection()
//     {
//         while (dialogOption == 0)
//         {
//             yield return null;
//         }

//         encounterSelector();
//         EraseDialogBox();
//         dialogs();

//         dialogOption = 0;
//         yield return null;
//     }

//     private void EraseDialogBox()
//     {
//         dialogBoxText.text = "";
//         dialogOptionsText[0].text = "";
//         dialogOptionsText[1].text = "";
//         dialogOptionsText[2].text = "";
//         dialogOptionsText[3].text = "";
//     }

//     private void encounterSelector()
//     {
//         if (Encounter.ToString() == "Skeleted")
//             Skeleton();
//     }
//     private void Skeleton()
//     {
//         switch (dialogIndex)
//         {
//             case 0:
//                 if (dialogOption == 1)
//                 {
//                     Debug.Log("player perde 10 de vida");
//                     dialogIndex = 1;
//                 }
//                 if (dialogOption == 2)
//                     dialogIndex = 2;
//                 break;
//             case 1:
//                 Debug.Log("Termina o bglh");
//                 dialogIndex = 99;
//                 EraseDialogBox();
//                 break;
//             case 2:
//                 if (dialogOption == 1)
//                 {
//                     Debug.Log("Player perde 10 de vida");
//                     dialogIndex = 1;
//                 }
//                 if (dialogOption == 2)
//                     dialogIndex = 3;
//                 break;
//             case 3:
//                 Debug.Log("termina o bglh sem perder vida");
//                 dialogIndex = 99;
//                 break;
//         }

//     }
// }

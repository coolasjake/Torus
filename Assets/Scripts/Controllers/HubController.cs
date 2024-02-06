using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HubController : MonoBehaviour
{
    public static HubController singleton;

    public string missionScene = "Mission";
    public string missionDataFolder = "/Data/Missions";
    public static List<MissionData> possibleMissions = new List<MissionData>();
    public List<MissionData> missionOptions = new List<MissionData>();

    //public List<HubCharacter> characters = new List<HubCharacter>();

    [Header("Dialogue Panel")]
    public RectTransform dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text dialogueNPCName;
    public Image dialogueNPCImage;

    [Header("Mission Brief Panel")]
    public RectTransform missionBriefPanel;
    public TMP_Text missionBriefText;
    public Image planetImage;
    public List<Image> enemyImages;

    public int maxCharactersPerBox;
    public float charAnimationDur = 0.01f;

    private HubCharacter currentCharacter;
    private bool _showingMissionBrief = false;
    private bool _showingWeaponSelect = false;
    private string[] dialogueSections;
    private int _currentSection = 0;
    private bool[] _readyPlayers;
    private int _initiatingPlayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        singleton = this;
        SetupUI();
        SetupMissions();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetupUI()
    {
        dialoguePanel.gameObject.SetActive(false);
        missionBriefPanel.gameObject.SetActive(false);
    }

    private void SetupMissions()
    {
        possibleMissions.AddRange(Resources.LoadAll<MissionData>(missionDataFolder));
    }

    public void SelectMission(int index)
    {
        index = Mathf.Clamp(index, 0, possibleMissions.Count - 1);
        BattleController.missionData = possibleMissions[index];
        SceneManager.LoadScene(missionScene);
    }

    public void StartDialogue(HubCharacter character, int initiatingPlayer)
    {
        currentCharacter = character;
        _initiatingPlayer = initiatingPlayer;
        ParseDialogue(character.dialogue);
        _currentSection = 0;
        if (dialogueSections.Length > 0)
            _animationCoroutine = StartCoroutine(AnimateDialogue());
    }

    public void AcceptPressed(int playerIndex)
    {
        if (_showingMissionBrief)
        {
            ReadyPlayerForMission(playerIndex);
        }
        else if (playerIndex == _initiatingPlayer)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                FinishTextAnimation();
            }
            else if (_currentSection < dialogueSections.Length - 1)
            {
                _currentSection += 1;
                AnimateDialogue();
            }
            else if (_showingMissionBrief == false)
            {
                ShowMissionBrief();
            }
        }
    }

    public void BackPressed(int playerIndex)
    {
        if (_readyPlayers[playerIndex] == true)
            UnreadyPlayerForMission(playerIndex);
        else if (playerIndex == _initiatingPlayer)
        {
            CloseDialogue();
        }
    }

    private void ReadyPlayerForMission(int playerIndex)
    {
        _readyPlayers[playerIndex] = true;

        //Show ready indicator

        bool allReady = true;
        for (int p = 0; p < _readyPlayers.Length; ++p)
        {
            if (_readyPlayers[p] == false)
            {
                allReady = false;
                break;
            }
        }
        if (allReady)
            StartMission();
    }

    private void UnreadyPlayerForMission(int playerIndex)
    {
        _readyPlayers[playerIndex] = false;

        //Change indicator to not-ready
    }

    private void CloseDialogue()
    {
        _showingMissionBrief = false;
    }

    private void ShowMissionBrief()
    {
        _showingMissionBrief = true;

    }

    private void StartMission()
    {
        BattleController.missionData = currentCharacter.mission;
        SceneManager.LoadScene(missionScene);
    }

    private Coroutine _animationCoroutine = null;
    private IEnumerator AnimateDialogue()
    {
        string substring = "";
        WaitForSeconds wait = new WaitForSeconds(charAnimationDur);
        for (int c = 0; c < dialogueSections[_currentSection].Length; ++c)
        {
            substring = dialogueSections[_currentSection].Substring(0, c);
            dialogueText.text = substring;
            yield return wait;
        }

        FinishTextAnimation();
    }

    private void FinishTextAnimation()
    {
        dialogueText.text = dialogueSections[_currentSection];
        _animationCoroutine = null;
    }

    private void ParseDialogue(string dialogue)
    {
        List<string> sections = new List<string>();
        List<string> paragraphs = new List<string>();
        paragraphs.AddRange(dialogue.Split(new string[] { "\n\n\n" }, System.StringSplitOptions.None));
        foreach (string paragraph in paragraphs)
        {
            List<string> sentences = new List<string>();
            sentences.AddRange(paragraph.Split(new string[] { ". ", "." }, System.StringSplitOptions.None));
            for (int i = 0; i < sentences.Count; ++i)
            {
                if (sentences[i].Length <= maxCharactersPerBox)
                    continue;

                for (int c = maxCharactersPerBox; c >= 0; --c)
                {
                    if (sentences[i][c] == ' ')
                    {
                        sentences.Insert(i + 1, sentences[i].Substring(c));
                        sentences.Insert(i + 1, sentences[i].Substring(0, c));
                        sentences.RemoveAt(i);
                        break;
                    }
                }
            }

            sections.AddRange(sentences);
        }

        dialogueSections = sections.ToArray();
    }
}

public enum Faction
{
    None,

    Terran,
    Forescouts,
    Astrogenesis,
    Maisonner,

    Ahnlia,
    Ryshvi,
    Zodan,
}
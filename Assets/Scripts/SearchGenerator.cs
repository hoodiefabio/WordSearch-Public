using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class SearchGenerator : MonoBehaviour
{

    public bool useWordpool; // 'should we use the wordpool?'
    public TextAsset wordpool; // if true, wordpool will be utilized
    public AudioSource winSound; // audio that plays when search is done
    public AudioSource correctSound; //audio that plays when a word is found
    public AudioSource selectSound; // audio that plays when letter is selected
    [SerializeField] LineRenderer lineRenderer; // Line rendering for selecting
    [SerializeField] Material correctMaterial; // material for line when correct
    [SerializeField] ParticleSystem confetti; // particles for when you complete search
    public string[] words; // overwritten if wordpool = true
    public int maxWordCount; // max number of words used
    public int maxWordLetters; // max length of word used 
    public bool allowReverse; // if true, words can be selected in reverse order.
    public int gridX, gridY; // grid dimensions
    public float sensitivity; // sensitivity of tiles when clicked
    public float spacing; // spacing between tiles
    public GameObject tile, background, current;
    public Color defaultTint, mouseoverTint, identifiedTint;// colors of letterboxes
    public bool correct = false;
    public string selectedString = ""; // string wich is currently selected
    public List<GameObject> selected = new List<GameObject>(); // list of selected boxes
    public Timer timer;//timer of the game

    private List<GameObject> tiles = new List<GameObject>();
    private GameObject temporary, backgroundObject;
    private int identified = 0;// amount of words identified
    private string[,] matrix;
    private SortedDictionary<string, bool> word = new SortedDictionary<string, bool>();
    private Dictionary<string, bool> insertedWords = new Dictionary<string, bool>();
    private string[] letters = new string[26]
    {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
    private Ray ray;
    private RaycastHit hit;
    private int mark = 0;
    private int letterIndex = 0;
    private bool soundplayed = false;
    private LineRenderer currentLine;
    private Letters currentLineStart;
    private Letters currentLineEnd;

    private static SearchGenerator instance;
    public static SearchGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        List<string> findLength = new List<string>();
        int count = 0;

        if (useWordpool)
        {
            words = wordpool.text.Split(';');
        }
        else
        {
            maxWordCount = words.Length;
        }

        if (maxWordCount <= 0)
        {
            maxWordCount = 3;
        }

        Mathf.Clamp(maxWordLetters, 0, gridY < gridX ? gridX : gridY);
        Mix(words);

        while (findLength.Count != maxWordCount)
        {
            if (words[count].Length <= maxWordLetters)
            {
                findLength.Add(words[count]);
            }
            count++;
        }

        for (int i = 0; i < maxWordCount; i++)
        {
            if (!word.ContainsKey(findLength[i].ToUpper()) && !word.ContainsKey(findLength[i]))
            {
                word.Add(findLength[i], false);
            }
        }

        Mathf.Clamp01(sensitivity);
        matrix = new string[gridX, gridY];
        InstantiateBG();

        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                temporary = Instantiate(tile, new Vector3(i * 1 * tile.transform.localScale.x * spacing, 10, j * 1 * tile.transform.localScale.z * spacing), Quaternion.identity) as GameObject;
                temporary.name = "tile-" + i.ToString() + "-" + j.ToString();
                temporary.transform.eulerAngles = new Vector3(180, 0, 0);
                temporary.transform.SetParent(backgroundObject.transform);
                BoxCollider boxCollider = temporary.GetComponent<BoxCollider>();
                boxCollider.size = new Vector3(sensitivity, 1, sensitivity);
                temporary.GetComponent<Letters>().letter.text = "";
                temporary.GetComponent<Letters>().gridX = i;
                temporary.GetComponent<Letters>().gridY = j;
                tiles.Add(tile);
                matrix[i, j] = "";
            }
        }
        CenterBG();
        InsertWords();
        FillRemaining();
    }
    private void CenterBG()
    {
        backgroundObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3((Screen.width / 2) + 100, (Screen.height / 2) + 60, 200));
    }

    private void InstantiateBG()
    {
        if (gridX % 2 != 0 && gridY % 2 == 0)
        {
            backgroundObject = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing)
            * (gridX / 2), 1, (tile.transform.localScale.z * spacing)
            * (gridY / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;
        }
        else if (gridX % 2 == 0 && gridY % 2 != 0)
        {
            backgroundObject = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing) * (gridX / 2)
            - (tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (gridY / 2)), Quaternion.identity) as GameObject;
        }
        else
        {
            backgroundObject = Instantiate(background, new Vector3((tile.transform.localScale.x * spacing) * (gridX / 2) -
                (tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (gridY / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;
        }
        backgroundObject.transform.eulerAngles = new Vector3(180, 0, 0);
        backgroundObject.transform.localScale = new Vector3(((tile.transform.localScale.x * spacing) * gridX), 1, ((tile.transform.localScale.x * spacing) * gridY));
    }

    private bool _block = false;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Letters.over != null)
        {
            if (_block) return;
            _block = true;
            currentLineStart = Letters.over;
            currentLine = Instantiate(lineRenderer);
            currentLine.transform.SetParent(lineRenderer.transform.parent);
            currentLine.SetPosition(0, new Vector3(currentLineStart.lineTarget.position.x, 200, currentLineStart.lineTarget.position.z));

        }

        else if (Input.GetMouseButton(0) && currentLine != null)
        {
            if (Letters.over != null)
            {
                currentLineEnd = Letters.over;
                currentLine.SetPosition(1, new Vector3(currentLineEnd.lineTarget.position.x, 200, currentLineEnd.lineTarget.position.z));
            }

            if (IsValidLine())
            {
                currentLine.gameObject.SetActive(true);
            }
            else
            {
                currentLine.gameObject.SetActive(true);
                Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                currentLine.SetPosition(1, new Vector3(targetPos.x, 200, targetPos.z));
            }
        }

        else if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            _block = false;
            Verify(currentLineStart, currentLineEnd);
            currentLineStart = null;
            currentLine = null;
        }
        

        if (insertedWords.Count == identified)
        {
            //feedback search complete
            PlayWinSound();
            confetti.Play();
            timer.timerIsRunning = false;
        }
    }


    private void PlayWinSound()
    {
        if (!soundplayed)
        {
            winSound.PlayOneShot(winSound.clip);
            soundplayed = true;
        }
        
    }

    bool IsValidLine()
    {
        if (currentLineStart == null || currentLineEnd == null) return false;

        if (currentLineStart.gridX == currentLineEnd.gridX || currentLineStart.gridY == currentLineEnd.gridY)
        {
            return true;
        }
        else if (Mathf.Max(currentLineStart.gridX, currentLineEnd.gridX) - Mathf.Min(currentLineStart.gridX, currentLineEnd.gridX) ==
            Mathf.Max(currentLineStart.gridY, currentLineEnd.gridY) - Mathf.Min(currentLineStart.gridY, currentLineEnd.gridY))
        {
            return true;
        }
        return false;
    }

    private void Verify(Letters a, Letters b)
    {
        if (!correct)
        {
           if(a != b  && a.index == b.index && (a.startWord || a.endWord) && (b.startWord || b.endWord) && !a.identified && !b.identified)
            {
                correct = true;
                a.identified = true;
                b.identified = true;
                selectedString = insertedWords.ElementAt(a.index - 1).Key;
            }   
        }

        if (correct)
        {
            correctSound.PlayOneShot(correctSound.clip);
            insertedWords.Remove(selectedString);
            currentLine.material = correctMaterial; 

            if (word.ContainsKey(selectedString))
            {
                insertedWords.Add(selectedString, true);
            }
            else if (word.ContainsKey(Reverse(selectedString)))
            {
                insertedWords.Add(Reverse(selectedString), true);
            }
            identified++;
        }
        else if (!correct)
        {
            Destroy(currentLine.gameObject);
        }
        selected.Clear();
        selectedString = "";
        correct = false;
    }

    private void InsertWords()
    {
        System.Random rn = new System.Random();
        foreach (KeyValuePair<string, bool> p in word)
        {
            string s = p.Key.Trim();
            bool placed = false;
            while (placed == false)
            {
                int row = rn.Next(gridX);
                int column = rn.Next(gridY);
                int directionX = 0;
                int directionY = 0;
                while (directionX == 0 && directionY == 0)
                {
                    directionX = rn.Next(3) - 1;
                    directionY = rn.Next(3) - 1;
                }
                placed = InsertWord(s.ToLower(), row, column, directionX, directionY);
                mark++;
                if (mark > 30000)
                {
                    break;
                }
            }
        }
    }

    private bool InsertWord(string word, int row, int column, int directionX, int directionY)
    {
        if (directionX > 0)
        {
            if (row + word.Length >= gridX)
            {
                return false;
            }
        }
        if (directionX < 0)
        {
            if (row - word.Length < 0)
            {
                return false;
            }
        }
        if (directionY > 0)
        {
            if (column + word.Length >= gridY)
            {
                return false;
            }
        }
        if (directionY < 0)
        {
            if (column - word.Length < 0)
            {
                return false;
            }
        }

        if (((0 * directionY) + column) == gridY - 1)
        {
            return false;
        }

        for (int i = 0; i < word.Length; i++)
        {
            if (!string.IsNullOrEmpty(matrix[(i * directionX) + row, (i * directionY) + column]))
            {
                return false;
            }
        }
        

        insertedWords.Add(word, false);
        letterIndex++;
        int letterCount = 0;
        char[] w = word.ToCharArray();
        for (int i = 0; i < w.Length; i++)
        {
            letterCount++;
            matrix[(i * directionX) + row, (i * directionY) + column] = w[i].ToString();
            GameObject.Find("tile-" + ((i * directionX) + row).ToString() + "-" + ((i * directionY) + column).ToString()).GetComponent<Letters>().letter.text = w[i].ToString();
            GameObject.Find("tile-" + ((i * directionX) + row).ToString() + "-" + ((i * directionY) + column).ToString()).GetComponent<Letters>().index = letterIndex;
            if (letterCount == 1)
                GameObject.Find("tile-" + ((i * directionX) + row).ToString() + "-" + ((i * directionY) + column).ToString()).GetComponent<Letters>().startWord = true; 
            else if (letterCount == w.Length)
                GameObject.Find("tile-" + ((i * directionX) + row).ToString() + "-" + ((i * directionY) + column).ToString()).GetComponent<Letters>().endWord = true;
        }
        return true;
    }

    private void FillRemaining()
    {
        for (int i = 0; i < gridX; i++)
        {
            for (int j = 0; j < gridY; j++)
            {
                if (matrix[i, j] == "")
                {
                    matrix[i, j] = letters[UnityEngine.Random.Range(0, letters.Length)];
                    GameObject.Find("tile-" + i.ToString() + "-" + j.ToString()).GetComponent<Letters>().letter.text = matrix[i, j];
                }
            }
        }
    }

    private void Mix(string[] words)
    {
        for (int i = 0; i < words.Length; i++)
        {
            string temp = words[i];
            int randomIndex = Random.Range(i, words.Length);
            words[i] = words[randomIndex];
            words[randomIndex] = temp;
        }
    }


    private string Reverse(string word)
    {
        string reversed = "";
        char[] letters = word.ToCharArray();
        for (int i = letters.Length - 1; i >= 0; i--)
        {
            reversed += letters[i];
        }
        return reversed;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(30);
        GUI.skin.label.fontSize = 66;
        GUI.contentColor = Color.black;

        foreach (KeyValuePair<string, bool> p in insertedWords)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("   " + p.Key);
            if (p.Value)
            {
                GUILayout.Label("✔");
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}

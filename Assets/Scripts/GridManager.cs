using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Serializable]
    public struct CardData
    {
        public string Name;
        public string ImageURL;
        //public Sprite Image;
    }
    CardData[] allCardsData;
    
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform camera;
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private float checkDelay = 0.3f;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] public TMP_Text gameText, finishText, menuText;

    public Sprite[] cards;
    private int[] cardsType;
    private string[] cardsName;
    private int subjectIndex = 0;
    private Dictionary<Vector2, Tile> tiles;
    private int width, height;
    private int howManyAreFlipped = 0;  //0-2.
    [HideInInspector]
    public bool isChecking = false;
    private Tile current1, current2;
    private int points = 0;

    //
    int currentPage = 0; //to be between 0 to amount of type/8 +1, need to reset each flip
    int currentType = 0;
    public GameObject tileParent;
    int amount0, amount1, amount2, amount3;
    [SerializeField] private TMP_Text cardsInGame;
    [SerializeField] private TMP_Text cardsNeeded;
    [SerializeField] private TMP_Text cardsChosen;
    [SerializeField] private TMP_Text cardsThrown;
    [SerializeField] private TMP_Text cardsHalf;
    public float allTilesOffset = 0.5f;
    int amountToPick;
    public GameObject winPanel;
    public GameObject ShowCardPanel;
    public Image ShowCardBig;
    //
    public Image ShowCardBig2;
    public bool isComparing = false;
    //
    private CardLoader cardLoader;
    public TMP_Text titleText;

    private void Start()
    {
        //subjectIndex= PlayerPrefs.GetInt("chosenValue");
        //StartCoroutine(LoadCards());    // with chosen
        cardLoader = FindObjectOfType<CardLoader>();
        if (cardLoader.oneSubjectGame)
            menuText.gameObject.SetActive(false);

        List<Sprite> cardsList= cardLoader.ListAllSprites(cardLoader.GetCurrentSubject()).Item1;
        List<string> cardsNames = cardLoader.ListAllSprites(cardLoader.GetCurrentSubject()).Item2;
        int len = cardsList.Count;
        cards = new Sprite[len];
        cardsType = new int[cards.Length];
        cardsName = new string[cards.Length];
        for (int i = 0; i < len; i++)
        {
            cards[i] = cardsList[i];
            cardsType[i] = 0;   //no choice yet
            cardsName[i] = cardsNames[i];
        }
        amount0 = cards.Length;
        amount1 = 0;
        amount2 = 0;
        amount3 = 0;
        /*
        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height= dims.Item1;
        */
        GenerateGrid(currentType,currentPage);
        UpdateNumbers();
        ButtonChangeAmountToPick(5);
        amountToPick = cardLoader.GetCurrentAmountToPick();

        char[] stringArray5 = amountToPick.ToString().ToCharArray();
        Array.Reverse(stringArray5);
        string amount5Rev = new string(stringArray5);
        cardsNeeded.text = "כמות קלפים לבחירה: " + amount5Rev;
    }

    private void Update()
    {
        
    }

    public void ButtonChangeAmountToPick(int num)
    {
        cardLoader.ChangeCurrentAmountToPick(num);
        amountToPick = cardLoader.GetCurrentAmountToPick();
        char[] stringArray5 = amountToPick.ToString().ToCharArray();
        Array.Reverse(stringArray5);
        string amount5Rev = new string(stringArray5);
        cardsNeeded.text = "כמות קלפים לבחירה: " + amount5Rev;
    }

    public void ShowNextCards()
    {
        int amount = HowManyOfType(currentType);
        if (amount <= (currentPage + 1) * 8)
        {
            currentPage = 0;
        }
        else
        {
            currentPage++;
        }
        FindObjectOfType<AudioManager>().Play("Flip");
        GenerateGrid(currentType, currentPage);
    }

    public void SwitchCurrentShownType(int type)
    {
        currentType = type;
        currentPage = 0;
        GenerateGrid(currentType, currentPage);
        FindObjectOfType<AudioManager>().Play("Match");
        UpdateNumbers();
        ChangeTitle(type);
    }

    public void ChangeTitle(int type)
    {
        switch (type)
        {
            case 0:
                titleText.text = "קלפים שנותרו לטיפול";
                break;
            case 1:
                titleText.text = "קלפים שנבחרו";
                break;
            case 2:
                titleText.text = "מתלבט לגבי קלפים";
                break;
            case 3:
                titleText.text = "קלפים שהושלכו";
                break;
        }
    }

    public void ShowBig()
    {
        if (current1 != null)
        {
            ShowCardBig2.sprite = null;
            ShowCardBig.sprite = current1.GetCard();
            //ShowCardPanel.SetActive(true);
            FindObjectOfType<AudioManager>().Play("Match");
        }
        isComparing = true;
    }

    public void ShowBig2()
    {
        if (current2 != null)
        {
            ShowCardBig2.sprite = current2.GetCard();
            ShowCardPanel.SetActive(true);
            FindObjectOfType<AudioManager>().Play("Match");
        }
        isComparing = false;
    }

    public void ExitShowBig()
    {
        ShowCardPanel.SetActive(false);
        ShowCardBig.sprite = null;
        FindObjectOfType<AudioManager>().Play("Match");
    }

    public bool CheckIfCanClick()
    {
        return !ShowCardPanel.activeInHierarchy;
    }

    public int HowManyOfType(int i)
    {
        int count = 0;
        for (int j = 0; j < cardsType.Length; j++)
        {
            if (cardsType[j] == i)
            {
                count++;
            }
        }
        return count;
    }

    public List<string> ListAllPicked()
    {
        List<string> chosen = new List<string>();
        for (int j = 0; j < cardsType.Length; j++)
        {
            if (cardsType[j] == 1)
            {
                chosen.Add(cardsName[j]);
            }
        }
        return chosen;
    }

    public void UpdateNumbers()
    {
        char[] stringArray1 = amount1.ToString().ToCharArray();
        Array.Reverse(stringArray1);
        string amount1Rev = new string(stringArray1);
        cardsChosen.text = "נבחרו: " + amount1Rev;

        char[] stringArray2 = amount2.ToString().ToCharArray();
        Array.Reverse(stringArray2);
        string amount2Rev = new string(stringArray2);
        cardsHalf.text = "בהתלבטות: " + amount2Rev;

        char[] stringArray3 = amount3.ToString().ToCharArray();
        Array.Reverse(stringArray3);
        string amount3Rev = new string(stringArray3);
        cardsThrown.text = "הושלכו: " + amount3Rev;

        char[] stringArray4 = cards.Length.ToString().ToCharArray();
        Array.Reverse(stringArray4);
        string amount4Rev = new string(stringArray4);
        cardsInGame.text = "כמות קלפים במשחק: " + amount4Rev;
    }


    IEnumerator LoadCards()
    {
        string url = "https://drive.google.com/uc?export=download&id=14zfwJDWAqhPhJ9R_wdc7ER12vW-iy8H4";
        //this from playerprefs
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.chunkedTransfer = false;
        yield return request.Send();

        if (request.isNetworkError)
        {
            Debug.Log("error?");
        }
        else
        {
            if (request.isDone)
            {
                allCardsData = JsonHelper.GetArray<CardData>(request.downloadHandler.text);
                Debug.Log(allCardsData);
                cards = new Sprite[allCardsData.Length];
                StartCoroutine(GetCardsImages());
            }
            else
            {
                Debug.Log("?");
            }
        }
        /*
        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height = dims.Item1;
        GenerateGrid();
        */
        
    }

    IEnumerator GetCardsImages()
    {
        for(int i=0;i<allCardsData.Length;i++) {
            
            /*
            WWW w = new WWW(allCardsData[i].ImageURL);
            yield return w;
            */

            UnityWebRequest w = UnityWebRequestTexture.GetTexture(allCardsData[i].ImageURL);
            yield return w.SendWebRequest();
            Debug.Log("WHAT? " + allCardsData[i].Name);

            if (w.error != null)
            {
                Debug.Log("Couldnt load this card");
            }
            else
            {
                if (w.isDone)
                {
                    //Texture2D tx = w.texture;
                    Texture2D tx=((DownloadHandlerTexture)w.downloadHandler).texture;
                    //allCardsData[i].Image = Sprite.Create(tx, new Rect(0f, 0f, tx.width, tx.height), Vector2.zero);
                    CardLoader cardLoader = FindObjectOfType<CardLoader>();
                    //cards[i]= Sprite.Create(tx, new Rect(0f,0f, tx.width, tx.height), new Vector2(1 / 2f, 1 / 2f), 128);
                    cards[i]= Sprite.Create(tx, new Rect(0f,0f, tx.width, tx.height), new Vector2(1 / 2f, 1 / 2f), 128/(tx.width/128));
                    Debug.Log(allCardsData[i].Name);
                    cardsName[i] = allCardsData[i].Name;

                }
            }
            

        }
        /*
        Tuple<int, int> dims = FindBestSquareShape(cards.Length * 2);
        width = dims.Item2;
        height = dims.Item1;
        GenerateGrid();
        */
    }

    private Tuple<int, int> FindBestSquareShape(int n)
    {
        var factors = new Tuple<int, int>[0];
        for (int i = 1; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0)
            {
                Array.Resize(ref factors, factors.Length + 1);
                factors[factors.Length - 1] = Tuple.Create(i, n / i);
            }
        }

        int bestDiff = int.MaxValue;
        Tuple<int, int> bestShape = null;
        foreach (var shape in factors)
        {
            int diff = Math.Abs(shape.Item1 - shape.Item2);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestShape = shape;
            }
        }

        return bestShape;
    }

    private List<int> GenerateDatabase(int x)
    {
        int[] randomNumbers = Enumerable.Range(0, cards.Length).OrderBy(x => Guid.NewGuid()).Take(x).ToArray();
        var database = new List<int>();
        for (int i = 0; i < x; i++)
        {
            database.Add(randomNumbers[i]);
            database.Add(randomNumbers[i]);
        }
        return database;
    }

    private int ChooseRandomItem(List<int> database)
    {
        var random = new System.Random();
        int index = random.Next(database.Count);
        int randomItem = database[index];
        database.RemoveAt(index);
        return randomItem;
    }
    /*
    private void Update()
    {
        //Debug.Log(howManyAreFlipped + "," + isChecking);
        if (howManyAreFlipped == 2&&!isChecking)
        {
            isChecking = true;
            StartCoroutine(CheckMatch());
        }
    }
    */

    public IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(checkDelay);
        if (current1.GetCard() == current2.GetCard())
        {
            BoxCollider2D collider1 = current1.gameObject.GetComponent<BoxCollider2D>();
            BoxCollider2D collider2 = current2.gameObject.GetComponent<BoxCollider2D>();
            if (collider1 != null)
            {
                collider1.enabled = false;
            }
            if (collider2 != null)
            {
                collider2.enabled = false;
            }
            points++;
            if (points == cards.Length)
            {
                gameText.gameObject.SetActive(false);
                finishText.gameObject.SetActive(true);
                pointsText.gameObject.SetActive(false);
                FindObjectOfType<AudioManager>().Play("Win");
            }
            else
            {
                pointsText.text = points.ToString();
                FindObjectOfType<AudioManager>().Play("Match");
            }
            howManyAreFlipped = 0;
            current1 = null;
            current2 = null;
            isChecking = false;
        }
        else
        {
            current1.ReturnToBack();
            current2.ReturnToBack();
            FindObjectOfType<AudioManager>().Play("Wrong");
        }
        /*
        howManyAreFlipped = 0;
        current1 = null;
        current2 = null;
        isChecking = false;
        */
    }

    public void SwitchType(int index,int type)
    {
        int prevType = cardsType[index];
        if (prevType == 0)
            amount0--;
        if (prevType == 1)
            amount1--;
        if (prevType == 2)
            amount2--;
        if (prevType == 3)
            amount3--;
        cardsType[index] = type;
        if (type == 0)
            amount0++;
        if (type == 1)
            amount1++;
        if (type == 2)
            amount2++;
        if (type == 3)
            amount3++;
        UpdateNumbers();
        if(amount1>=amountToPick)
        {
            Debug.Log("EMAIL!");
            winPanel.SetActive(true);
            FindObjectOfType<AudioManager>().Play("Win");
        }
    }

    public void TransferType(int type)
    {
        if (current1 == null || type == currentType)
        {
            FindObjectOfType<AudioManager>().Play("Wrong");
            return;
        }
        int index = int.Parse(current1.name.Substring(5));
        SwitchType(index, type);
        current1.Shrink();
    }
    void GenerateGrid(int currentType, int currentPage)
    {
        for (int i = 0; i < tileParent.transform.childCount; i++)
        {
            GameObject Go = tileParent.transform.GetChild(i).gameObject;
            Destroy(Go);
        }
        tiles = new Dictionary<Vector2, Tile>();
        width = 4;
        height = 2;
        CardLoader cardLoader = FindObjectOfType<CardLoader>();
        float ratioF=1;
        List<int> chosenIndices= new List<int>();
        int cnt = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            if (cardsType[i] == currentType)
            {
                cnt++;
                if (cnt > (currentPage * 8) && cnt <= ((currentPage * 8) + 8))
                {
                    chosenIndices.Add(i);
                }
            }
        }
        int listLen = chosenIndices.Count;
        int listCount = 0;
        for (int i=0;i<width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(listCount>=listLen)
                {
                    break;
                }
                Sprite chosenCard = cards[chosenIndices[listCount]];
                ratioF = cardLoader.HandleKnownRatio(chosenCard.rect.width);
                var spawnedTile = Instantiate(tilePrefab, new Vector3(i * (1* ratioF + offset), j * (1* ratioF + offset)), Quaternion.identity);
                spawnedTile.transform.SetParent(tileParent.transform);
                spawnedTile.transform.GetChild(0).transform.localScale = new Vector3(ratioF, ratioF, 1);
                spawnedTile.GetComponent<BoxCollider2D>().size = new Vector2(ratioF, ratioF);
                spawnedTile.name = $"Tile {chosenIndices[listCount]}";
                spawnedTile.SetCard(chosenCard);
                tiles[new Vector2(i,j)]= spawnedTile;
                listCount++;
            }
        }
        camera.transform.position = new Vector3((float)width * (1*ratioF + offset) / 2 - 0.5f* ratioF, (float)height * (1* ratioF + offset) / 2 - 0.5f* ratioF, -10);
        tileParent.transform.position = new Vector3(tileParent.transform.position.x, tileParent.transform.position.y - allTilesOffset, tileParent.transform.position.z);
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public int HowManyAreFlipped()
    {
        return howManyAreFlipped;
    }

    public void ResetFlips()
    {
        howManyAreFlipped = 0;
    }

    public void AddFlipCounter()
    {
        howManyAreFlipped++;
        Debug.Log("X");
    }

    public void SetCurrent1(Tile tile)
    {
        current1 = tile;
    }

    public void SetCurrent2(Tile tile)
    {
        current2 = tile;
    }

    public Tile GetCurrent1()
    {
        return current1;
    }

    public Tile GetCurrent2()
    {
        return current2;
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}

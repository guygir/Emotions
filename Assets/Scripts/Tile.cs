using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Sprite card,cardBack;
    [SerializeField] private GameObject highlight;

    private GridManager gridManager;
    
    // Start is called before the first frame update
    void Start()
    {
        gridManager=FindObjectOfType<GridManager>();
        ShowFront();
    }

    private void OnMouseEnter()
    {
        //highlight.SetActive(true); 
    }

    private void OnMouseExit()
    {
       //highlight.SetActive(false);
    }

    public void RemoveHighlight()
    {
        highlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        /*
        if (gridManager.HowManyAreFlipped() == 2||this==gridManager.GetCurrent1()||this==gridManager.GetCurrent2()||gridManager.finishText.gameObject.active)
        {
            return;
        }
        gridManager.AddFlipCounter();
        if (gridManager.HowManyAreFlipped() == 1)
            gridManager.SetCurrent1(this);
        else
            gridManager.SetCurrent2(this);
        HandleRayOn();
        FindObjectOfType<AudioManager>().Play("Flip");
        */
        HandleRayOn();
    }

    private void HandleRayOn()
    {
        /*
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.transform != null)
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile)
            {
                Animator anim = tile.transform.GetComponent<Animator>();
                anim.Play("FlipOn");
            }
        }
        */
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.transform != null)
        {
            Tile tile = hit.transform.GetComponent<Tile>();
            if (tile)
            {
                HandleFlip(tile);
            }
        }
    }

    private void HandleFlip(Tile tile)
    {
        /*
        if (gridManager.isChecking||gridManager.HowManyAreFlipped() == 2 || this == gridManager.GetCurrent1() || this == gridManager.GetCurrent2() || gridManager.finishText.gameObject.active)
        {
            return;
        }
        gridManager.AddFlipCounter();
        if (gridManager.HowManyAreFlipped() == 1)
            gridManager.SetCurrent1(tile);
        else
        {
            gridManager.SetCurrent2(tile);
        }
        Animator anim = tile.GetComponent<Animator>();
        anim.Play("FlipOn");
        FindObjectOfType<AudioManager>().Play("Flip");
        */
        if (!gridManager.CheckIfCanClick())
        {
            return;
        }
        if (gridManager.isComparing)
        {
            gridManager.SetCurrent2(tile);
            gridManager.ShowBig2();
            return;
        }
        Tile prevTile = gridManager.GetCurrent1();
        if (prevTile != null)
        {
            prevTile.highlight.SetActive(false);
        }
        gridManager.SetCurrent1(tile);
        highlight.SetActive(true);
    }

    public void Shrink()
    {
        Animator anim = GetComponent<Animator>();
        anim.Play("Shrink");
        FindObjectOfType<AudioManager>().Play("Flip");
    }

    public void DestroyTile()
    {
        Destroy(this.gameObject);
    }


    public void ReturnToBack()
    {
        //GetComponent<Animator>().Play("FlipOff");
    }

    public void ShowFront()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = card;
    }

    public void ShowBack()
    {
        //transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = cardBack;
        //gridManager.ResetFlips();
        gridManager.SetCurrent1(null);
        //gridManager.SetCurrent2(null);
        //gridManager.isChecking = false;
        highlight.SetActive(false);
    }

    public Sprite GetCard()
    {
        return card;
    }

    public void SetCard(Sprite newCard)
    {
        card = newCard;
    }
}

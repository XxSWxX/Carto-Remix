using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public bool mapOpen = false;
    public bool menuOpen = false;
    public CanvasGroup mapCanvas;

    public GameObject[] tilePrefabs;

    public GameObject storageUI;

    public GameObject mapUI;

    public GameObject selector;

    public Vector2 selectorPos = new Vector2(0, 0);

    //private int totalPlaced = 1;

    private GameObject holdingTile = null; //reference to tile being held

    private GameObject[] allworldtiles;

    private RaycastHit2D[] hits;

    public int canvasGrid; //distance a tile can move on the map's canvas
    public int storageGrid; //distance between tiles in storage ui

    private int movestep;//moves at the same distance as the tile's size
    public int mapbounds;//sets the bounds for the map selector to be 7x7 which is more than enough

    //public TextMeshProUGUI xButton; //might use these later
    //public TextMeshProUGUI spaceButton;

    private GameObject tile;

    void Start()
    {
        movestep = canvasGrid;
        placeAllWorldTiles();
    }


    // all the button press controls
    void OnGUI()
    {
        // escape to bring up map/menu
        if (Event.current.Equals(Event.KeyboardEvent("escape")))
        {
            if (holdingTile != null)
            {
                Debug.Log("cant close menu while holding a tile");
            }
            else
            {
                menuOpen = !menuOpen;
                if (menuOpen)
                {
                    mapOpen = true; // whenever menu is open, puts player to map screen first
                    mapCanvas.alpha = 1;
                    Time.timeScale = 0f; //freeze timescale when in menu, easy way to prevent enemies and players from physically moving while in menu
                }
                else
                {
                    //also have this here becacuse secretly it has to run twice because uh
                    placeAllWorldTiles();

                    mapOpen = false;
                    mapCanvas.alpha = 0;
                    Time.timeScale = 1.0f;
                }
            }
        }

        //if on the map screen in pause menu, enables all the map controls
        if (mapOpen)
        {

            //open/close tile storage
            if (Event.current.Equals(Event.KeyboardEvent("x")))
            {
                // placeAllWorldTiles();//also here cause storing these needs to cause an update to the borders aswell ;-;
                if (holdingTile != null)
                {
                    Debug.Log("cant close while holding a tile");
                }
                else
                {
                    ToggleStorage(1); // the true/false tells the function to uh
                }
            }

            // up down left right and wasd controls
            if ((Event.current.Equals(Event.KeyboardEvent("w")) || Event.current.Equals(Event.KeyboardEvent("up"))) && !toggleStorage)
            {
                //Debug.Log("up");
                if (selectorPos.y < mapbounds)
                {
                    selectorPos += new Vector2(0, movestep);
                }

            }
            if ((Event.current.Equals(Event.KeyboardEvent("s")) || Event.current.Equals(Event.KeyboardEvent("down"))) && !toggleStorage)
            {
                //Debug.Log("dn");
                if (selectorPos.y > -mapbounds)
                {
                    selectorPos += new Vector2(0, -movestep);
                }

            }
            if (Event.current.Equals(Event.KeyboardEvent("a")) || Event.current.Equals(Event.KeyboardEvent("left")))
            {
                //Debug.Log("lf");
                if (selectorPos.x > -mapbounds)
                {
                    selectorPos += new Vector2(-movestep, 0);
                }

            }
            if (Event.current.Equals(Event.KeyboardEvent("d")) || Event.current.Equals(Event.KeyboardEvent("right")))
            {
                //Debug.Log("rt");
                if (selectorPos.x < mapbounds)
                {
                    selectorPos += new Vector2(movestep, 0);
                }

            }

            //rotate held tile left
            if (Event.current.Equals(Event.KeyboardEvent("q")) && holdingTile)
            {
                holdingTile.GetComponent<RectTransform>().eulerAngles += new Vector3(0, 0, 90f);
            }

            //rotate held tile right
            if (Event.current.Equals(Event.KeyboardEvent("e")) && holdingTile)
            {
                holdingTile.GetComponent<RectTransform>().eulerAngles += new Vector3(0, 0, -90f);
            }

            //pick up the tile selected by selector
            if (Event.current.Equals(Event.KeyboardEvent("space")))
            {
                if (holdingTile == null)
                //if a tile is not picked up then pick up the selected tile
                {

                    holdingTile = FindClosestTile();
                    //if (holdingTile.transform.parent == mapUI) 
                    //{
                    //    totalPlaced -= 1; //decrement total placed tiles when a player picks up a tile that's on the map. used to check when all tiles are placed
                    //    Debug.Log("took away a tile:"+totalPlaced);
                    //}
                    holdingTile.transform.SetParent(selector.transform);
                    selector.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    if (toggleStorage)
                    {
                        ToggleStorage(0);
                    }

                }
                else
                //place a tile where the selector is
                {

                    int layer = 6; //the tile trigger collider layer
                    int layerMask = 1 << layer; //why does it have to be written like this??????????????

                    bool canPlace = true;

                    // Cast a ray in tile's UP transform
                    hits = Physics2D.RaycastAll(selector.transform.position, holdingTile.transform.up, 100, layerMask);

                    if (hits.Length == 2) //if 2 hits are detected then that means player is trying to connect 2 pieces
                    {
                        if (hits[0].transform.name == hits[1].transform.name) //compare the border collider name of each side, if the name is the same then can place
                        {
                        }
                        else
                        {
                            canPlace = false;
                        }
                    }
                    // Cast a ray in tile's DOWN transform
                    hits = Physics2D.RaycastAll(selector.transform.position, -holdingTile.transform.up, 100, layerMask);

                    if (hits.Length == 2) //if 2 hits are detected then that means player is trying to connect 2 pieces
                    {
                        if (hits[0].transform.name == hits[1].transform.name) //compare the border collider name of each side, if the name is the same then can place
                        {
                        }
                        else
                        {
                            canPlace = false;
                        }
                    }
                    // Cast a ray in tile's RIGHT transform
                    hits = Physics2D.RaycastAll(selector.transform.position, holdingTile.transform.right, 100, layerMask);

                    if (hits.Length == 2) //if 2 hits are detected then that means player is trying to connect 2 pieces
                    {
                        if (hits[0].transform.name == hits[1].transform.name) //compare the border collider name of each side, if the name is the same then can place
                        {
                        }
                        else
                        {
                            canPlace = false;
                        }
                    }
                    // Cast a ray in tile's LEFT transform
                    hits = Physics2D.RaycastAll(selector.transform.position, -holdingTile.transform.right, 100, layerMask);

                    if (hits.Length == 2) //if 2 hits are detected then that means player is trying to connect 2 pieces
                    {
                        if (hits[0].transform.name == hits[1].transform.name) //compare the border collider name of each side, if the name is the same then can place
                        {
                        }
                        else
                        {
                            canPlace = false;
                        }
                    }

                    //Debug.Log(canPlace);

                    if (canPlace)
                    {
                        //totalPlaced += 1; //increment total placed by 1 when a tile is successfully placed, this is used to check when all tiles are placed.
                        //Debug.Log("added a tile:" + totalPlaced); //still doesnt know if they are in the right places though..
                        selector.transform.localScale = new Vector3(1, 1, 1);
                        holdingTile.transform.SetParent(mapUI.transform);
                        holdingTile.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                        holdingTile.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                        holdingTile.GetComponent<RectTransform>().anchoredPosition = selectorPos;

                        //set world tiles after placement
                        placeAllWorldTiles();
                        holdingTile = null; //let rest of script know im not holding a tile
                    }
                    else
                    {
                        Debug.Log("cannot place this here!");
                    }

                    if (toggleStorage)
                    {
                        ToggleStorage(0);
                    }
                }

            }

            if (Event.current.Equals(Event.KeyboardEvent("z")))
            {
                //Debug.Log(WorldTiles.totalPlacedTiles);

            }

            if (Event.current.Equals(Event.KeyboardEvent("return")))
            {
                //Debug.Log(WorldTiles.totalPlacedTiles);
                //WorldTiles.totalPlacedTiles = 0; //reset counter so it doesnt count over what it needs

            }

            //debug add tiles by number press
            if (Event.current.Equals(Event.KeyboardEvent("1")))
            {
                addTile(0);
            }
            if (Event.current.Equals(Event.KeyboardEvent("2")))
            {
                addTile(1);
            }
            if (Event.current.Equals(Event.KeyboardEvent("3")))
            {
                addTile(2);
            }
            if (Event.current.Equals(Event.KeyboardEvent("4")))
            {
                addTile(3);
            }
            if (Event.current.Equals(Event.KeyboardEvent("5")))
            {
                addTile(4);
            }
            if (Event.current.Equals(Event.KeyboardEvent("6")))
            {
                addTile(5);
            }
            if (Event.current.Equals(Event.KeyboardEvent("7")))
            {
                addTile(6);
            }
            if (Event.current.Equals(Event.KeyboardEvent("8")))
            {
                addTile(7);
            }
            if (Event.current.Equals(Event.KeyboardEvent("9")))
            {
                addTile(8);
            }

        }



        //moves map selector
        selector.GetComponent<RectTransform>().anchoredPosition = selectorPos;

    }

    // function that adds tiles to the storage, another script will call this function
    public void addTile(int num)
    {
        Debug.Log("add piece #" + num + " to storage array");

        tile = Instantiate(tilePrefabs[num]) as GameObject;
        tile.transform.parent = storageUI.transform;
        tile.transform.localScale = new Vector3(1,1,1); //resize for storage UI

        //if (num < 8)
        //{
        //    tile = Instantiate(tilePrefabs[num]) as GameObject;
        //    tile.transform.localScale = new Vector3(1, 1, 1); //resize for storage UI
        //    tile.transform.parent = storageUI.transform;
        //}
        //else
        //{
        //    tile = Instantiate(tilePrefabs[num]) as GameObject;
        //    tile.transform.localScale = new Vector3(1, 1, 1); //resize for storage UI
        //    tile.transform.parent = mapUI.transform;
        //    tile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0); //should be positioned where the center of the donut is tho
        //}

    }



    private bool toggleStorage = false;

    void ToggleStorage(int withSelector)
    {
        toggleStorage = !toggleStorage;


        if (withSelector == 0)
        {
            if (toggleStorage)
            {
                mapbounds = 400;
                selector.transform.SetParent(storageUI.transform, false);
                selectorPos = new Vector2(-400, 0);
                movestep = storageGrid;
            }
            else 
            {
                mapbounds = 100;
                selector.transform.SetParent(mapUI.transform, false);
                selectorPos = new Vector2(0, 0);
                movestep = canvasGrid;
            }
        }
        else
        {
            if (toggleStorage) //OPEN UI
            {
                if (holdingTile != null) { selector.transform.localScale = new Vector3(1,1,1); holdingTile.transform.SetParent(mapUI.transform);  holdingTile = null; } //if storage is closed while something is held, it should not go back to storage or else player will go to the backrooms D: .

                mapbounds = 400;
                //make this tween later
                mapUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
                //also move selector to the tile storage ui for selection
                selector.transform.SetParent(storageUI.transform, false);
                selectorPos = new Vector2(-400, 0);
                movestep = storageGrid;
            }
            else //CLOSE UI
            {
                mapbounds = 100;
                //make this tween later
                mapUI.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,-120,0);

                selector.transform.SetParent(mapUI.transform, false);
                selectorPos = new Vector2(0, 0);
                movestep = canvasGrid;
            }
        }
    }

    public void placeAllWorldTiles()
    {
        WorldTiles.connectedWrong = false;
        allworldtiles = GameObject.FindGameObjectsWithTag("MapTile");
        foreach (GameObject child in allworldtiles)
        {
            if (child.transform.parent.gameObject == mapUI) //only place if not in storage
            {
                child.GetComponent<WorldTiles>().placeWorldTile(false);
            }
            else
            {
                //else if tile is in storage, send this tile to the BACKROOMS
                child.GetComponent<WorldTiles>().placeWorldTile(true);
            }
        }
        WorldTiles.totalPlacedTiles = 0; //reset counter so it doesnt count over what it needs
    }

    public GameObject FindClosestTile()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("MapTile");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = selector.transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        if (distance < 10 && !closest.isStatic) //if cursor is close enough to pick up and the object can be picked up (isstatic)
        {
            return closest;
        }
        else
        {
            return null;
        }
    }


    public void placeWinnerTile()
    {
        Debug.Log("YAY!!! GENERATE WINNER TILE!");
        addTile(8);
    }

    }

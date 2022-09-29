using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BabaooTest
{

    public class Taquin : MonoBehaviour
    {
        /// <summary>
        /// Static reference to the score
        /// </summary>
        public static int bestScore = 0;

        /// <summary>
        /// Direct references to tile prefabs. I tend to avoid Resources.Load
        /// </summary>
        [SerializeField]
        private List<GameObject> TilePrefabs;

        //HorizontalTilesCount * VerticalTilesCount must be equal to TilePrefabs.Count
        [SerializeField]
        private int HorizontalTilesCount = 3;
        [SerializeField]
        private int VerticalTilesCount = 3;

        //Spacing between tiles
        [SerializeField]
        private float HorizontalPadding = 4f;
        [SerializeField]
        private float VerticalPadding = 4f;

        /// <summary>
        /// Number of time the puzzle is pre shuffled
        /// </summary>
        [SerializeField]
        private int ShuffleCount = 200;

        /// <summary>
        /// Reference to the grid holding the tiles
        /// </summary>
        [SerializeField]
        private RectTransform GridRect;

        /// <summary>
        /// Reference to the blank tile prefab (the movable part)
        /// </summary>
        [SerializeField]
        private GameObject blankTilePrefab;

        /// <summary>
        /// Reference to the Game timer
        /// </summary>
        [SerializeField]
        private Timer timer;

        /// <summary>
        /// Array holding the tiles
        /// </summary>
        private Tile[] Tiles;
        /// <summary>
        /// Array holding a copy of the tiles, to compare against the Tile array for victory status
        /// </summary>
        private Tile[] ReferenceTiles;

        /// <summary>
        /// Reference holding the win text, appearing when the user has won
        /// </summary>
        [SerializeField]
        private GameObject WinText;
        /// <summary>
        /// Internal ref to the previous position of a tile during the shuffle, to prevent back and forth
        /// </summary>
        private Vector2 previousPos;
        /// <summary>
        /// Internal reference to the grid
        /// </summary>
        private GridLayoutGroup grid;

        private bool hasBegun = false;

        /// <summary>
        /// Internal ref to control the tween speed in order to have correct positionning
        /// </summary>
        private float tweenSpeed = 0.1f;


        IEnumerator Start()
        {
            yield return null;
            CreateGrid();
            yield return null;

            StartCoroutine(ShuffleTiles());
        }

        /// <summary>
        /// Creates the grid and populate tiles
        /// </summary>
        /// <exception cref="System.Exception">If the column count * row count != number of tiles, throw an exception</exception>
        private void CreateGrid()
        {
            if (TilePrefabs.Count != HorizontalTilesCount * VerticalTilesCount)
            {
                throw new System.Exception($"Invalid number of tiles provided for a {HorizontalTilesCount}x{VerticalTilesCount} grid");
            }
            float width = GridRect.rect.width;
            float height = GridRect.rect.height;

            // Position elements on a grid
            grid = GridRect.GetComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(HorizontalPadding, VerticalPadding);
            grid.cellSize = new Vector2((width / (HorizontalTilesCount)) - HorizontalPadding, (height / (VerticalTilesCount)) - VerticalPadding);

            Tiles = new Tile[TilePrefabs.Count];
            ReferenceTiles = new Tile[TilePrefabs.Count];
            for (int i = 0; i < TilePrefabs.Count; i++)
            {
                Tiles[i] = Instantiate(TilePrefabs[i], GridRect.transform).GetComponent<Tile>();
                Tiles[i].x = i % HorizontalTilesCount;
                Tiles[i].y = i / HorizontalTilesCount;
                Tiles[i].OnValidDrop = Swap;
                ReferenceTiles[i] = Tiles[i];
            }
        }

        /// <summary>
        /// Shuffles the tiles
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShuffleTiles()
        {
            // Take a random index 
            int blankIndex = Random.Range(0, TilePrefabs.Count);
            int x = Tiles[blankIndex].x;
            int y = Tiles[blankIndex].y;

            //Mark random Index tile as blank
            SetBlankTile(blankIndex);
            //Wait a frame for the blank tile to be the correct size
            yield return null;

            //The grid is not longer necessary, as it prevents tweening between tiles
            grid.enabled = false;
            tweenSpeed = 0.01f;

            for (int i = 0; i < ShuffleCount; i++)
            {
                Vector2 pos = GetValidPosition(x, y);

                int dX = x + (int)pos.x;
                int dY = y + (int)pos.y;
                int indexToSwap = HorizontalTilesCount * dY + dX;

                SwapTiles(blankIndex, indexToSwap, true);
                yield return new WaitForSeconds(tweenSpeed);
                //New position
                blankIndex = indexToSwap;
                x = dX;
                y = dY;

            }
            tweenSpeed = 0.1f;
            timer.BeginTimer();
        }

        /// <summary>
        /// Swap two tiles, their positions, their coordinates and their order in the Tiles list
        /// </summary>
        /// <param name="i">Index of tile 1 in the Tiles list</param>
        /// <param name="j">Index of tile 2 in the Tiles list</param>
        /// <param name="isTweening"></param>
        private void SwapTiles(int i, int j, bool isTweening = false)
        {
            //Swap coordinates
            int tempX = Tiles[i].x;
            int tempY = Tiles[i].y;
            Tiles[i].x = Tiles[j].x;
            Tiles[i].y = Tiles[j].y;
            Tiles[j].x = tempX;
            Tiles[j].y = tempY;

            //Swap positions
            if (isTweening)
            {
                LeanTween.move(Tiles[i].gameObject, Tiles[j].transform.position, tweenSpeed).setEaseInOutCirc();
                LeanTween.move(Tiles[j].gameObject, Tiles[i].transform.position, tweenSpeed).setEaseInOutCirc();
            }
            else
            {
                Vector2 pos = Tiles[i].transform.position;
                Tiles[i].transform.position = Tiles[j].transform.position;
                Tiles[j].transform.position = pos;
            }

            //Swap in hierarchy for visualisation purposes
            Tiles[i].transform.SetSiblingIndex(j);
            Tiles[j].transform.SetSiblingIndex(i);

            //Swap inside list
            Tile tmp = Tiles[i];
            Tiles[i] = Tiles[j];
            Tiles[j] = tmp;

            if (hasBegun && HasEnded())
            {
                timer.Stop();
                bestScore = timer.Timeleft;

                WinText.SetActive(true);
            }
        }

        /// <summary>
        /// Called by the Tile object on drop, swap two tiles
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        public void Swap(Tile t1, Tile t2)
        {
            //TODO: C'est moche i know

            hasBegun = true;
            int i = System.Array.IndexOf(Tiles, t1);
            int j = System.Array.IndexOf(Tiles, t2);
            SwapTiles(i, j, true);
        }

        /// <summary>
        /// Win condition function, compares the Tiles array to the Reference Array
        /// </summary>
        /// <returns>true if the player won, false otherwise</returns>
        public bool HasEnded()
        {
            if (Enumerable.SequenceEqual(Tiles, ReferenceTiles))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get a valid position for the shuffle function to work
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector2 GetValidPosition(int x, int y)
        {
            Vector2 ValidPos;
            do
            {
                int dir = Random.Range(0, 4);
                switch (dir)
                {
                    case 0: ValidPos = Vector2.up; break;
                    case 1: ValidPos = Vector2.left; break;
                    case 2: ValidPos = Vector2.down; break;
                    default: ValidPos = Vector2.right; break;
                }
            } while (!IsInsideBounds(x + (int)ValidPos.x, y + (int)ValidPos.y) || ValidPos * -1 == previousPos);

            previousPos = ValidPos;
            return ValidPos;
        }

        /// <summary>
        /// Test to see if the position given is inside the bounds of the array (Used only for shuffle, didn't think to use it for tile switching, must sleep ugh)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsInsideBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= HorizontalTilesCount || y >= VerticalTilesCount) return false;
            return true;
        }

        /// <summary>
        /// Sets the blank tile and destroy the tile it was on.
        /// Blank tile is the movable part
        /// </summary>
        /// <param name="index"></param>
        public void SetBlankTile(int index)
        {
            Tile tile = Tiles[index];

            Tile blankTile = Instantiate(blankTilePrefab, GridRect.transform).GetComponent<Tile>();
            blankTile.x = tile.x;
            blankTile.y = tile.y;
            blankTile.IsBlank = true;
            blankTile.OnValidDrop = Swap;
            Tiles[index] = blankTile;
            ReferenceTiles[index] = blankTile;
            Destroy(tile.gameObject);
            blankTile.transform.SetSiblingIndex(index);
        }
    }
}

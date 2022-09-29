using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BabaooTest
{

    public class Taquin : MonoBehaviour
    {

        public static int bestScore = 0;

        //Direct references to tile prefabs. I tend to avoid Resources.Load
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

        [SerializeField]
        private int ShuffleCount = 999;
        [SerializeField]
        private RectTransform GridRect;

        [SerializeField]
        private GameObject blankTilePrefab;

        [SerializeField]
        private Timer timer;

        private Tile[] Tiles;
        private Tile[] ReferenceTiles;

        private Vector2 previousPos;
        private GridLayoutGroup grid;
        private bool hasBegun = false;
        [SerializeField]
        private GameObject WinText;

        private float tweenSpeed = 0.1f;

        IEnumerator Start()
        {
            yield return null;
            CreateGrid();
            yield return null;

            StartCoroutine(ShuffleTiles());
        }

        private void CreateGrid()
        {
            if (TilePrefabs.Count != HorizontalTilesCount * VerticalTilesCount)
            {
                throw new System.Exception($"Invalid number of tiles provided for a {HorizontalTilesCount}x{VerticalTilesCount} grid");
            }
            float width = GridRect.rect.width;
            float height = GridRect.rect.height;

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

        private IEnumerator ShuffleTiles()
        {
            // Take a random index 
            int blankIndex = Random.Range(0, TilePrefabs.Count);
            int x = Tiles[blankIndex].x;
            int y = Tiles[blankIndex].y;

            //Mark random Index tile as blank
            SetBlankTile(blankIndex);
            yield return null;
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

        public void Swap(Tile t1, Tile t2)
        {
            //TODO: C'est moche i know

            hasBegun = true;
            int i = System.Array.IndexOf(Tiles, t1);
            int j = System.Array.IndexOf(Tiles, t2);
            SwapTiles(i, j, true);
        }

        public bool HasEnded()
        {
            if (Enumerable.SequenceEqual(Tiles, ReferenceTiles))
            {
                return true;
            }
            return false;
        }

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

        private bool IsInsideBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= HorizontalTilesCount || y >= VerticalTilesCount) return false;
            return true;
        }

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

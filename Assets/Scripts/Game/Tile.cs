using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BabaooTest
{
    public class Tile : MonoBehaviour, IDragHandler, IDropHandler
    {

        /// <summary>
        /// Action called when a drop is valid
        /// </summary>
        public Action<Tile, Tile> OnValidDrop;

        /// <summary>
        /// X Coordinate
        /// </summary>
        public int x;
        /// <summary>
        /// Y Coordinate
        /// </summary>
        public int y;
        /// <summary>
        /// Is mover tile or not
        /// </summary>
        public bool IsBlank = false;

        public void OnDrag(PointerEventData eventData)
        {
            //Do nothing but enable drag
        }

        public void OnDrop(PointerEventData eventData)
        {
            Tile dragged = eventData.pointerDrag.GetComponent<Tile>();
            if ((dragged.IsBlank || this.IsBlank) && ValidDrop(dragged))
            {
                OnValidDrop?.Invoke(this, dragged);
            }

        }

        /// <summary>
        /// Check if the dragged tile can be moved to the drop tile
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool ValidDrop(Tile t)
        {
            //TODO: Refacto this mess
            if (t.x > x + 1 || t.y > y + 1 || t.x < x - 1 || t.y < y - 1 || (t.x == x - 1 && t.y == y - 1) || (t.x == x + 1 && t.y == y + 1)
            || (t.x == x + 1 && t.y == y - 1) || (t.x == x - 1 && t.y == y + 1))
            {
                return false;
            }
            return true;
        }
    }
}

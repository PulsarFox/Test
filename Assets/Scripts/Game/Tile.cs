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

        public Action<Tile, Tile> OnValidDrop;

        public int x;
        public int y;
        public bool IsBlank = false;

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnDrop(PointerEventData eventData)
        {
            Tile dragged = eventData.pointerDrag.GetComponent<Tile>();
            if ((dragged.IsBlank || this.IsBlank) && ValidDrop(dragged))
            {
                OnValidDrop?.Invoke(this, dragged);
            }

        }

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

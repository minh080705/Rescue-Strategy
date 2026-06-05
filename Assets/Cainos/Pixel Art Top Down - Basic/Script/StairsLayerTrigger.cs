using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class StairsLayerTrigger : MonoBehaviour
    {
        public Direction direction;
        [Space]
        public string layerUpper;
        public string sortingLayerUpper;
        [Space]
        public string layerLower;
        public string sortingLayerLower;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (direction == Direction.South && other.transform.position.y < transform.position.y)
                SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else if (direction == Direction.West && other.transform.position.x < transform.position.x)
                SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
            else if (direction == Direction.East && other.transform.position.x > transform.position.x)
                SetLayerAndSortingLayer(other.gameObject, layerUpper, sortingLayerUpper);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (direction == Direction.South && other.transform.position.y < transform.position.y)
                SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else if (direction == Direction.West && other.transform.position.x < transform.position.x)
                SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
            else if (direction == Direction.East && other.transform.position.x > transform.position.x)
                SetLayerAndSortingLayer(other.gameObject, layerLower, sortingLayerLower);
        }

        private void SetLayerAndSortingLayer(GameObject target, string layer, string sortingLayer)
        {
            string oldLayer = LayerMask.LayerToName(target.layer);
            string oldSortingLayer = target.GetComponent<SpriteRenderer>().sortingLayerName;

            target.layer = LayerMask.NameToLayer(layer);

            target.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }

            Debug.Log($"[StairsLayerTrigger] '{target.name}' chuyển layerr: {oldLayer} → {layer} | Sorting: {oldSortingLayer} → {sortingLayer}", target);
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }
    }
}
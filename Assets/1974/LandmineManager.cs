using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LandmineManager : MonoBehaviour
{
    public Tilemap landmineTilemap;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("LandmineManager detected collision with " + collision.name);
        if (collision.CompareTag("Tank"))
        {
            Debug.Log("1");
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            Vector3Int cellPosition = landmineTilemap.WorldToCell(hitPosition);
      
                Debug.Log("2");
                // Remove the landmine tile
                landmineTilemap.SetTile(cellPosition, null);
                // Trigger tank hit response
                TankHealth tankHealth = collision.GetComponent<TankHealth>();
                if (tankHealth != null)
                {
                    tankHealth.TakeHit();
                }
            
        }
    }
}

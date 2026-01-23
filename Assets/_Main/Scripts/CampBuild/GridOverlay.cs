using UnityEngine;
namespace CampBuild
{
    public class GridOverlay : MonoBehaviour
    {
        [SerializeField] private int gridSizeX = 10; 
        [SerializeField] private int gridSizeZ = 10; 
        [SerializeField] private float tileSize = 1f; 

        private void OnDrawGizmos()
        {

            Debug.Log("Drawing Gizmos Grid");
            Gizmos.color = Color.green; 

           
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    
                    Vector3 tilePosition = new Vector3(x * tileSize, 0f, z * tileSize);
                    Gizmos.DrawWireCube(tilePosition, new Vector3(tileSize, 0f, tileSize)); 
                }
            }

        }
    }
}
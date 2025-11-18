using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private void Awake()
    {
        transform.localScale = Vector3.one * 1.1f;
    }
    public void UpdateCursor(RaycastHit raycastHit, RaycastHitType raycastHitType , Vertex_Y selected,Vertex_Y target)
    {
        GetComponent<MeshFilter>().mesh.Clear();
        if (raycastHitType == RaycastHitType.ground)
        {
            Debug.Log("01-targetPosition6"+target.worldPosition);
            Vertex vertex =  target.vertex;
            Debug.Log("02-currentPosition6"+target.vertex.currentPosition);
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            transform.position = target.worldPosition + Vector3.down * Grid.cellHeight* 0.5f ;
            Debug.Log("03-selfPosition6"+transform.position);
        }
        else if (raycastHitType == RaycastHitType.top)
        {
            Vertex vertex =  selected.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            transform.position = selected.worldPosition + Vector3.up * Grid.cellHeight* 0.6f ;
        }
        else if (raycastHitType == RaycastHitType.bottom)
        {
            Vertex vertex =  selected.vertex;
            GetComponent<MeshFilter>().mesh = vertex.CreateMesh();
            GetComponent<MeshFilter>().mesh.triangles = GetComponent<MeshFilter>().mesh.triangles.Reverse().ToArray();
            transform.position = selected.worldPosition + Vector3.down * Grid.cellHeight* 0.6f ;
        }
        else if (raycastHitType == RaycastHitType.side)
        {
            Vertex vertex =  selected.vertex;
            GetComponent<MeshFilter>().mesh = raycastHit.transform.GetComponent<MeshCollider>().sharedMesh;
            transform.position = selected.worldPosition  ;
        }
    }
}

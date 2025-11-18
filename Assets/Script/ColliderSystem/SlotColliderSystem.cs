using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotColliderSystem : MonoBehaviour
{
    private string GetSlotColliderName(Vertex_Y vertex_Y)
    {
        return "SlotCollider_" + vertex_Y.name;
    }

    public void CreateCollider(Vertex_Y vertex_Y)
    {   //创建Slot碰撞体的父对象
        Debug.Log($"CreateCollider,position{vertex_Y?.worldPosition}");
        GameObject slotCollider = new GameObject(GetSlotColliderName(vertex_Y),typeof(SlotCollider));
        slotCollider.layer = LayerMask.NameToLayer("SlotCollider");
        slotCollider.GetComponent<SlotCollider>().vertex_Y = vertex_Y;
        slotCollider.transform.SetParent(transform);
        slotCollider.transform.localPosition = vertex_Y.worldPosition;
        
        //创建顶部碰撞面
        GameObject top = new GameObject("top_to_" + (vertex_Y.y + 1), typeof(MeshCollider), typeof(SlotCollider_Top));
        top.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreateMesh();
        top.layer = LayerMask.NameToLayer("SlotCollider");
        top.transform.SetParent(slotCollider.transform);
        top.transform.localPosition = Vector3.up * Grid.cellHeight/2;
        
        //创建底部碰撞面
        GameObject bottom = new GameObject("bottom_to_" + (vertex_Y.y - 1), typeof(MeshCollider), typeof(SlotCollider_Bottom));
        bottom.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreateMesh();
        bottom.GetComponent<MeshCollider>().sharedMesh.triangles = bottom.GetComponent<MeshCollider>().sharedMesh.triangles.Reverse().ToArray();
        bottom.layer = LayerMask.NameToLayer("SlotCollider");
        bottom.transform.SetParent(slotCollider.transform);
        bottom.transform.localPosition = Vector3.down * Grid.cellHeight/2;
        
        //创建侧部碰撞面，并计算相邻块
        if (vertex_Y.vertex is vertex_center)
        {
            List<Mesh> meshes = ((vertex_center)vertex_Y.vertex).CreateSideMesh();
            for(int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
               Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].d.vertex_Ys[vertex_Y.y]; 
               GameObject side = new GameObject( "side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
               side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
               side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
               side.layer = LayerMask.NameToLayer("SlotCollider");
               side.transform.SetParent(slotCollider.transform);
               side.transform.localPosition = Vector3.zero;
              
            }
        }

        else if (vertex_Y.vertex is vertex_Hex)
        {
            List<Mesh> meshes = ((vertex_Hex)vertex_Y.vertex).CreateSideMesh();
            for(int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
                Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].b.vertex_Ys[vertex_Y.y]; 
                GameObject side = new GameObject( "side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
              
            }
        }
        else
        {
            List<Mesh> meshes = ((vertex_Mid)vertex_Y.vertex).CreateSideMesh();
            for(int i = 0; i < 4 ; i++)
            {
                Vertex_Y neighbor;
                if (vertex_Y.vertex == vertex_Y.vertex.subQuads[i].b)
                {
                    neighbor = vertex_Y.vertex.subQuads[i].c.vertex_Ys[vertex_Y.y];
                }
                else
                {
                    neighbor = vertex_Y.vertex.subQuads[i].a.vertex_Ys[vertex_Y.y];
                }
                GameObject side = new GameObject( "side_to_" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }
    }
    public void DestroyCollider(Vertex_Y vertex_Y)
    {
        Destroy(transform.Find(GetSlotColliderName(vertex_Y)).gameObject);
        Resources.UnloadUnusedAssets();
    }
}

public class SlotCollider : MonoBehaviour
{
    public Vertex_Y vertex_Y; 
}

public class SlotCollider_Top :MonoBehaviour
{

}

public class SlotCollider_Bottom : MonoBehaviour
{

}

public class SlotCollider_Side : MonoBehaviour
{
    public Vertex_Y neighbor;
}




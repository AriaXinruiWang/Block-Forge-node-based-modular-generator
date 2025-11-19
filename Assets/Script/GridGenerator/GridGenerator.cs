using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int radius;
    [SerializeField] private float cellSize;
    public float cellHeight;
    public int height;
    public int smoothTime;
    public Transform addSphere;
    public Transform deleteSphere;

    private Grid grid;
    public List<Slot> slots;
    private WorldMaster worldMaster;
    private WaveFunctionCollapse waveFunctionCollapse;
    [Header("Debug")]
    public GameObject testCube;
    //0502new
    public GameObject testDeleteCube;
    [Header("Module")]
    [SerializeField]
    public ModuleLibrary moduleLibrary;
    public static ModuleLibrary Instance { get; private set; }//0502new
    [SerializeField]
    private Material moduleMaterial;

    private void Awake()
    {
        worldMaster = GetComponentInParent<WorldMaster>();
        waveFunctionCollapse = worldMaster.waveFunctionCollapse;
        moduleLibrary = Instantiate(moduleLibrary);
        moduleLibrary.ImportedModule();
        Instance = moduleLibrary;//0502new
        grid = new Grid(radius, cellSize, cellHeight, height, smoothTime);
    }

    // 添加GetGrid方法，返回grid实例
    public Grid GetGrid()
    {
        return grid;
    }

    private void Update()
    {
        //foreach (var vertex in grid.vertexYList)
        // {
        //     if(Vector3.Distance(testCube.transform.position,vertex.currentPosition)<2f && !vertex.isBoundary)
        //     {
        //         vertex.SetVertexStatus(true);
        //     }
        //     if(Vector3.Distance(testDeleteCube.transform.position, vertex.currentPosition) <2f)
        //     {
        //         vertex.SetVertexStatus(false);
        //     }
        // }
        
        foreach (SubQuad subQuad in grid.subquads)
        {
            foreach (SubQuad_Cube SubQuad_Cube in subQuad.subQuad_Cubes)
            {
                SubQuad_Cube.UpdateBitValue();
            }
        }
    }

    //0502new
     public void UpdateSlot(SubQuad_Cube subQuad_Cube)
    {
        string name = "Slot_"+ grid.subquads.IndexOf(subQuad_Cube.subQuad)+"_"+ subQuad_Cube.y;
        GameObject slot_GameObject;
        if (transform.Find(name))
        {
            slot_GameObject = transform.Find(name).gameObject;
        }
        else
        {
            slot_GameObject = null;
        }
        if(slot_GameObject == null)
        {
            if(subQuad_Cube.bitValue != "00000000"&&subQuad_Cube.bitValue != "11111111")
            {
                slot_GameObject = new GameObject(name,typeof(Slot));
                slot_GameObject.transform.SetParent(transform);
                slot_GameObject.transform.localPosition = subQuad_Cube.centerPos;
                Slot slot = slot_GameObject.GetComponent<Slot>();
                slot.Initialize(subQuad_Cube,moduleMaterial,moduleLibrary);
                slots.Add(slot);
                slot.UpdateModule(slot.possibleModules[0]);
                
                //0702
                waveFunctionCollapse.resetSlots.Add(slot);
                waveFunctionCollapse.cur_collapseSlots.Add(slot);
            }
        }
        else
        {
            Slot slot = slot_GameObject.GetComponent<Slot>();
            if(subQuad_Cube.bitValue == "00000000"||subQuad_Cube.bitValue == "11111111")
            {
                slots.Remove(slot);
                //0702
                if (waveFunctionCollapse.resetSlots.Contains(slot))
                {
                    waveFunctionCollapse.resetSlots.Remove(slot);
                }

                if (waveFunctionCollapse.cur_collapseSlots.Contains(slot))
                {
                    waveFunctionCollapse.cur_collapseSlots.Remove(slot);
                }
               Destroy(slot_GameObject);
               Resources.UnloadUnusedAssets();
                
            }
            else
            {
                slot.ResetSlot(moduleLibrary);
                slot.UpdateModule(slot.possibleModules[0]);
                //0702
                if (!waveFunctionCollapse.resetSlots.Contains(slot))
                {
                    waveFunctionCollapse.resetSlots.Add(slot);
                }

                if (!waveFunctionCollapse.cur_collapseSlots.Contains(slot))
                {
                    waveFunctionCollapse.cur_collapseSlots.Add(slot);
                }
            }
        }
    }
    
//0504new
public void ToggleSlot(Vertex_Y vertex_Y)
{
    vertex_Y.isActive = !vertex_Y.isActive;
    foreach(SubQuad_Cube subQuad_Cube in vertex_Y.subQuad_Cubes)
    {
        subQuad_Cube.UpdateBitValue();
        UpdateSlot(subQuad_Cube);
    }
}

    // private void OnDrawGizmos()
    // {
    //     if (grid != null)
    //     {
    //         // // 遍历所有六边形顶点
    //         // foreach (vertex_Hex vertex in grid.Hexes)
    //         // {
    //         //     // 绘制顶点
    //         //     Gizmos.DrawSphere(vertex.coord.worldPosition, 0.1f);
    //         // }

    //         Gizmos.color = Color.red;
    //         // foreach (Triangle triangle in grid.triangles)
    //         // {
    //         //     Gizmos.DrawLine(triangle.a.coord.worldPosition, triangle.b.coord.worldPosition);
    //         //     Gizmos.DrawLine(triangle.b.coord.worldPosition, triangle.c.coord.worldPosition);
    //         //     Gizmos.DrawLine(triangle.c.coord.worldPosition, triangle.a.coord.worldPosition);
    //         //     // Gizmos.DrawSphere((triangle.a.coord.worldPosition + triangle.b.coord.worldPosition + triangle.c.coord.worldPosition)/3, 0.05f);
    //         // }

    //         foreach (var item in grid.subquads)
    //         {
    //             Gizmos.DrawLine(item.a.currentPosition, item.b.currentPosition);
    //             Gizmos.DrawLine(item.b.currentPosition, item.c.currentPosition);
    //             Gizmos.DrawLine(item.c.currentPosition, item.d.currentPosition);
    //             Gizmos.DrawLine(item.a.currentPosition, item.d.currentPosition);

    //             //Handles.Label(item.ab.vertexMid.initPos, item.ab.edgeID.ToString());
    //             //Handles.Label(item.bc.vertexMid.initPos, item.bc.edgeID.ToString());
    //             //Handles.Label(item.cd.vertexMid.initPos, item.cd.edgeID.ToString());
    //             //Handles.Label(item.ad.vertexMid.initPos, item.ad.edgeID.ToString());
    //         }
            

    //         foreach (var item in grid.allVertices)
    //         {
    //             foreach (var vertexY in item.vertex_Ys)
    //             {
    //                 if (vertexY.isActive)
    //                 {
    //                     Gizmos.color = Color.green;
    //                 }
    //                 else
    //                 {
    //                     Gizmos.color = Color.white;
    //                 }
    //                 Gizmos.DrawSphere(vertexY.currentPosition, 0.1f);
    //             }
    //         }
            
    //         foreach (var item in grid.subQuadCubes)
    //         {
    //             Gizmos.color = Color.white;
    //             Gizmos.DrawLine(item.vertices[0].currentPosition, item.vertices[1].currentPosition);
    //             Gizmos.DrawLine(item.vertices[1].currentPosition, item.vertices[2].currentPosition);
    //             Gizmos.DrawLine(item.vertices[2].currentPosition, item.vertices[3].currentPosition);
    //             Gizmos.DrawLine(item.vertices[0].currentPosition, item.vertices[3].currentPosition);

    //             Gizmos.DrawLine(item.vertices[4].currentPosition, item.vertices[5].currentPosition);
    //             Gizmos.DrawLine(item.vertices[5].currentPosition, item.vertices[6].currentPosition);
    //             Gizmos.DrawLine(item.vertices[6].currentPosition, item.vertices[7].currentPosition);
    //             Gizmos.DrawLine(item.vertices[4].currentPosition, item.vertices[7].currentPosition);

    //             Gizmos.DrawLine(item.vertices[0].currentPosition, item.vertices[4].currentPosition);
    //             Gizmos.DrawLine(item.vertices[1].currentPosition, item.vertices[5].currentPosition);
    //             Gizmos.DrawLine(item.vertices[2].currentPosition, item.vertices[6].currentPosition);
    //             Gizmos.DrawLine(item.vertices[3].currentPosition, item.vertices[7].currentPosition);

    //             GUI.color = Color.red;
    //             Handles.Label(item.centerPos, item.bitValue);


    //             //Gizmos.color = Color.blue;
    //             //Gizmos.DrawSphere(item.vertices[0].currentPosition, 0.15f);
    //         }
            
    //         //foreach (var item in grid.subquads)
    //         //{
    //         //    Gizmos.color = Color.green;
    //         //    DrawMyDirection(item.arrowPos[0], item.arrowPos[1]);
    //         //    DrawMyDirection(item.arrowPos[1], item.arrowPos[2]);
    //         //    DrawMyDirection(item.arrowPos[2], item.arrowPos[3]);
    //         //    DrawMyDirection(item.arrowPos[3], item.arrowPos[0]);
    //         //}

    //     }
    // }

    public void DrawMyDirection(Vector3 posFrom,Vector3 posTo)
    {
        Gizmos.DrawLine(posFrom, posTo);

        Vector3 lineDirection = (posFrom - posTo).normalized;
        Vector3 arrowDirection = Quaternion.Euler(0,90,0) * lineDirection;

        Vector3 midPos = (posFrom + posTo) / 2;

        Vector3 arrowUp = midPos + (lineDirection * Vector3.Distance(posTo, posFrom) * 0.15f) + arrowDirection * 0.15f;
        Vector3 arrowDown = midPos + (lineDirection * Vector3.Distance(posTo, posFrom) * 0.15f) - arrowDirection * 0.15f;

        Gizmos.DrawLine(midPos, arrowUp);
        Gizmos.DrawLine(midPos, arrowDown);
    }
}
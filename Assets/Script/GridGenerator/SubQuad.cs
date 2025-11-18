using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubQuad 
{
    public readonly vertex_Hex a;
    public readonly vertex_Mid b;
    public readonly vertex_center c;
    public readonly vertex_Mid d;

    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();// 添加了这个字段
    public Dictionary<SubQuad,Vertex[]>neighborVertices = new Dictionary<SubQuad,Vertex[]>();
    public readonly Vector3[] arrowPos = new Vector3[4];
    public SubQuad[] neighbors = new SubQuad[4];
    public SubQuad(vertex_Hex a,vertex_Mid b,vertex_center c, vertex_Mid d)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;
        subQuad_Cubes = new List<SubQuad_Cube>(); // 添加了初始化列表
    }
    
    public Vector3 GetCenterPos()
    {
        Vector3 centerPos = Vector3.zero;
        centerPos += a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition;
        centerPos /= 4;
        return centerPos;
    }

    public Vector3 GetMid_ab()
    {
        return (a.currentPosition + b.currentPosition) / 2;
    }

    public Vector3 GetMid_bc()
    {
        return (b.currentPosition + c.currentPosition) / 2;
    }

    public Vector3 GetMid_cd()
    {
        return (c.currentPosition + d.currentPosition) / 2;
    }

    public Vector3 GetMid_ad()
    {
        return (d.currentPosition + a.currentPosition) / 2;
    }


    public void GetArrowPos()
    {
        Vector3 dierectionA =  a.currentPosition - GetCenterPos();
        arrowPos[0] = a.currentPosition - dierectionA * 0.4f;
        Vector3 dierectionB = b.currentPosition - GetCenterPos();
        arrowPos[1] = b.currentPosition - dierectionB * 0.4f;
        Vector3 dierectionC = c.currentPosition - GetCenterPos();
        arrowPos[2] = c.currentPosition - dierectionC * 0.4f;
        Vector3 dierectionD = d.currentPosition - GetCenterPos();
        arrowPos[3] = d.currentPosition - dierectionD * 0.4f;
    
    }
    public void CaculateSmooth()
    {
        Vector3 center = (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;

        Vector3 vectorNew_a = (a.currentPosition
            + Quaternion.AngleAxis(-90, Vector3.up) * (b.currentPosition - center) + center
            + Quaternion.AngleAxis(-180, Vector3.up) * (c.currentPosition - center) + center
               + Quaternion.AngleAxis(-270, Vector3.up) * (d.currentPosition - center) + center
            ) / 4;
        Vector3 vectorNew_b = Quaternion.AngleAxis(90, Vector3.up) * (vectorNew_a - center) + center;
        Vector3 vectorNew_c = Quaternion.AngleAxis(180, Vector3.up) * (vectorNew_a - center) + center;
        Vector3 vectorNew_d = Quaternion.AngleAxis(270, Vector3.up) * (vectorNew_a - center) + center;

        a.offset += (vectorNew_a - a.currentPosition) * 0.1f;
        b.offset += (vectorNew_b - b.currentPosition) * 0.1f;
        c.offset += (vectorNew_c - c.currentPosition) * 0.1f;
        d.offset += (vectorNew_d - d.currentPosition) * 0.1f;

        //Debug.Log("ƽ������: a:" + a.offset + "b:" + b.offset + "c:" + c.offset + "d:" + d.offset);

    } 
}

public class SubQuad_Cube
{
    public SubQuad subQuad;
    public int y;

    public Vertex[] vertices = new Vertex[8];
    public SubQuad_Cube[] neighbors = new SubQuad_Cube[6];
    public Dictionary<SubQuad_Cube,Vertex_Y[]>neighborVertices = new Dictionary<SubQuad_Cube,Vertex_Y[]>();
    public bool isActive;
    public Vector3 centerPos;
    public string bitValue = "00000000";
    public string pre_bitValue = "00000000";
    

    public Slot slot;
    public int index;
    public SubQuad_Cube(SubQuad subQuad,int y,List<SubQuad_Cube> subQuad_Cubes)
    {
        this.subQuad = subQuad;
        this.y = y;
        subQuad_Cubes.Add(this);
        index = subQuad_Cubes.IndexOf(this);
        centerPos = subQuad.GetCenterPos() + Vector3.up * Grid.cellHeight* (y + 0.5f);

        vertices[0] = subQuad.a.vertex_Ys[y + 1];
        vertices[1] = subQuad.b.vertex_Ys[y + 1];
        vertices[2] = subQuad.c.vertex_Ys[y + 1];
        vertices[3] = subQuad.d.vertex_Ys[y + 1];
        vertices[4] = subQuad.a.vertex_Ys[y];
        vertices[5] = subQuad.b.vertex_Ys[y];
        vertices[6] = subQuad.c.vertex_Ys[y];
        vertices[7] = subQuad.d.vertex_Ys[y];
        //0505new
        foreach(Vertex vertex in vertices) 
        {
            if(vertex is Vertex_Y vertexY)
            {
                vertexY.subQuad_Cubes.Add(this);
            }
        }

        UpdateBitValue();
        foreach (var vertex in vertices)
        {
            vertex.OnVertexStatusChange += UpdateBitValue;
            vertex.OnVertexStatusChange += UpdateSlot;
        }
        centerPos = GetCenterPos();
    }

    public Vector3 GetCenterPos()
    {
        Vector3 tempPos = Vector3.zero;
        foreach (var item in vertices)
        {
            tempPos += item.currentPosition;
        }
        return tempPos / 8;
    }

    public void UpdateBitValue()
    {
        pre_bitValue = bitValue;
        bitValue = string.Empty;
        foreach (var vertex in vertices)
        {
            if(vertex.isActive)
            {
                bitValue+="1";
            }
            else
            {
                bitValue += "0";
            }
        }
        if (bitValue == "00000000")
        {
            isActive = false;
        }
        else
        { 
            isActive = true;
                
        }
    }

    public void UpdateSlot()
    {
        if(slot==null)
        {
            if(bitValue!="00000000"&& bitValue!= "11111111")
            {
                slot = new GameObject("Slot", typeof(Slot)).GetComponent<Slot>();
                slot.name = bitValue;
                slot.transform.position = centerPos;
                //路径：Assets/Resources/Material/Slot.mat
                Material newMaterial = Resources.Load<Material>("Material/Slot");
                slot.Initialize(this, newMaterial, GridGenerator.Instance);//0502new
                slot.UpdateModule(slot.possibleModules[0]);
            }
        }
        else 
        {
           if(bitValue == "00000000" && bitValue == "11111111")
           {
                slot.name = bitValue;
                GameObject.Destroy(slot.gameObject);
                Resources.UnloadUnusedAssets();
           }
           else
            {
                slot.name = bitValue;
                slot.ResetSlot(GridGenerator.Instance);//0502new//0702
                slot.UpdateModule(slot.possibleModules[0]);
            }
        }
    }
    //0702new
    public void NeighborsCheck()
    {
        if (subQuad.neighbors[0] != null)
        {
            neighbors[0] = subQuad.neighbors[0].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[0], new Vertex_Y[]
            {
                subQuad.neighborVertices[subQuad.neighbors[0]][0].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[0]][1].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[0]][0].vertex_Ys[y + 1],
                subQuad.neighborVertices[subQuad.neighbors[0]][1].vertex_Ys[y + 1]
            });
        }

        if (subQuad.neighbors[1] != null)
        {
            neighbors[1] = subQuad.neighbors[1].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[1], new Vertex_Y[]
            {
                subQuad.neighborVertices[subQuad.neighbors[1]][0].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[1]][1].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[1]][0].vertex_Ys[y + 1],
                subQuad.neighborVertices[subQuad.neighbors[1]][1].vertex_Ys[y + 1]
            });
        }
        
        if (subQuad.neighbors[2]!= null)
        {
            neighbors[2] = subQuad.neighbors[2].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[2], new Vertex_Y[]
            {
                subQuad.neighborVertices[subQuad.neighbors[2]][0].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[2]][1].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[2]][0].vertex_Ys[y + 1],
                subQuad.neighborVertices[subQuad.neighbors[2]][1].vertex_Ys[y + 1]
            });
        }
        
        if (subQuad.neighbors[3]!= null)
        {
            neighbors[3] = subQuad.neighbors[3].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[3], new Vertex_Y[]
            {
                subQuad.neighborVertices[subQuad.neighbors[3]][0].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[3]][1].vertex_Ys[y],
                subQuad.neighborVertices[subQuad.neighbors[3]][0].vertex_Ys[y + 1],
                subQuad.neighborVertices[subQuad.neighbors[3]][1].vertex_Ys[y + 1]
            });
        }

        if (y < Grid.height - 1)
        {
            neighbors[4] = subQuad.subQuad_Cubes[y + 1];
            neighborVertices.Add(neighbors[4], new Vertex_Y[]
            {
                (Vertex_Y)vertices[4],//和教程21.06不同  vertex_Ys
                (Vertex_Y)vertices[5],
                (Vertex_Y)vertices[6],
                (Vertex_Y)vertices[7]
            });
        }
        
        if (y > 0 )
        {
            neighbors[5] = subQuad.subQuad_Cubes[y - 1];
            neighborVertices.Add(neighbors[5], new Vertex_Y[]
            {
                (Vertex_Y)vertices[0],
                (Vertex_Y)vertices[1],
                (Vertex_Y)vertices[2],
                (Vertex_Y)vertices[3]
            });
        }
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.PlayerLoop; // 添加Linq引用以支持ToArray

public class Vertex
{
    public Vector3 initPos;

    public Vector3 currentPosition;

    public Vector3 offset;

    public List<SubQuad> subQuads = new List<SubQuad>();

    public List<Vertex_Y> vertex_Ys = new List<Vertex_Y>();
    public bool isBoundary;
    public int index;

    public int id  = 0;

    public bool isActive = false;

    public void BoundaryCheck()
    {
        //判断是否为边缘Hex
        bool isBoundaryHex = this is vertex_Hex && ((vertex_Hex )this).coord.radius == Grid.radius;
        //判断是否为边缘Mid
        bool isBoundaryMid = this is vertex_Mid && ((vertex_Mid )this).edge.hexes.ToArray()[0].coord.radius == Grid.radius && ((vertex_Mid)this).edge.hexes.ToArray()[1].coord.radius == Grid.radius;
        isBoundary = isBoundaryHex || isBoundaryMid;
    }
    public void Smooth()
    {
        // Debug.Log("offset :" + offset);
        currentPosition = initPos + offset;
    }
    public System.Action OnVertexStatusChange;
    
    public void SetVertexStatus(bool value)
    {
        if(value!=isActive)
        {
            isActive = value;
            OnVertexStatusChange?.Invoke();
        }
    }

    public Mesh CreateMesh()
    {
        Debug.Log("Mesh Created");
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
       
       foreach(SubQuad subQuad in subQuads)
       {
        if(this is vertex_center)
        {
            meshVertices.Add(currentPosition);
            meshVertices.Add(subQuad.GetMid_cd());
            meshVertices.Add(subQuad.GetCenterPos());
            meshVertices.Add(subQuad.GetMid_bc());
        }
        else if (this is vertex_Mid)
        {
            if ( subQuad.b == this)
            { 
                meshVertices.Add(currentPosition);
                meshVertices.Add(subQuad.GetMid_bc());
                meshVertices.Add(subQuad.GetCenterPos());
                meshVertices.Add(subQuad.GetMid_ab());
            }
            else
            {
                meshVertices.Add(currentPosition);
                meshVertices.Add(subQuad.GetMid_ad());
                meshVertices.Add(subQuad.GetCenterPos());
                meshVertices.Add(subQuad.GetMid_cd());
            }
        }
        else 
        {
            meshVertices.Add(currentPosition);
            meshVertices.Add(subQuad.GetMid_ab());
            meshVertices.Add(subQuad.GetCenterPos());
            meshVertices.Add(subQuad.GetMid_ad());
        }
       }
       for (int i = 0; i < meshVertices.Count; i++)
       {
           meshVertices[i] -= currentPosition;
       }

       for (int i = 0; i < subQuads.Count; i++)
       {
           meshTriangles.Add(i*4);
           meshTriangles.Add(i*4+1);
           meshTriangles.Add(i*4+2);
           meshTriangles.Add(i*4);
           meshTriangles.Add(i*4+2);
           meshTriangles.Add(i*4+3);
       }
       Mesh mesh = new Mesh();
       mesh.vertices = meshVertices.ToArray();
       mesh.triangles = meshTriangles.ToArray();
       return mesh;
    }
}
public class Coord 
{
    public readonly int q;
    public readonly int r;
    public readonly int s;
    public readonly int radius;
    public readonly Vector3 worldPosition;
    public Coord(int q, int r, int s, int radius = 0)  
    {
        this.q = q;
        this.r = r;
        this.s = s;
        this.radius = radius > 0 ? radius : Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));
        worldPosition = WorldPosition();
    }
    public Vector3 WorldPosition()
    {
        return new Vector3(q * Mathf.Sqrt(3) / 2, 0, -(float)r - ((float)q / 2)) * 2 * Grid.cellSize;
    }

   static public Coord[] directions = new Coord[]
   {
    new Coord(0, 1, -1, 0),
    new Coord(-1, 1, 0, 0),
    new Coord(-1, 0, 1, 0),
    new Coord(0, -1, 1, 0),
    new Coord(1, -1, 0, 0),
    new Coord(1, 0, -1, 0),
   };

   static public Coord Direction(int direction)
   {
     return Coord.directions[direction];
   }
   
   public Coord Add(Coord coord)
   {
    return new Coord(q + coord.q, r + coord.r, s + coord.s);
   }

   public Coord Scale(int k)
   {
    return new Coord(q * k, r * k, s * k);
   }

   public Coord Neighbor(int direction)
   {
    return Add(Direction(direction));
   }

   public static List<Coord> Coord_Ring(int radius)
   {
        List<Coord> result = new List<Coord>();
        if (radius == 0)
        {
            result.Add(new Coord(0, 0, 0, 0));
        }

        else
        {   
            Coord coord = Coord.Direction(4).Scale(radius);
            for (int i = 0; i < directions.Length; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    result.Add(coord);
                    coord = coord.Neighbor(i);
                }
            }
        }
        return result;
    }

   public static List<Coord> Coord_Hex()
   {
        List<Coord> result = new List<Coord>();
        for (int i=0;i<=Grid.radius;i++)
        {
            result.AddRange(Coord_Ring(i));
        }
        return result;
   }
}

public class vertex_Hex : Vertex
{
    public readonly Coord coord;

    public vertex_Hex(Coord coord)
    {
        this.coord = coord;
        initPos = coord.worldPosition;
        currentPosition = initPos;
    }

    public static List<vertex_Hex> GrabRing(int radius, List<vertex_Hex> vertices)
    {
        if (radius <= 0)
        {
            return vertices.GetRange(0, 1);
        }
        return vertices.GetRange(radius * (radius - 1) * 3 + 1, radius * 6);
    }

    public List<Mesh> CreateSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();
        
        for (int i = 0; i < n; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetMid_ab() + Vector3.down * Grid.cellHeight / 2);
            foreach (SubQuad subQuad in subQuads)
            {
                if (subQuad.d == subQuads[i].b)
                {
                    meshVertices.Add(subQuad.GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
                    meshVertices.Add(subQuad.GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
                    break;
                }
            }

            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }
            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);
            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;
    }

    public void NeighborSubQuadCheck()
    {
        foreach (SubQuad subQuad_a in subQuads)
        {
            foreach (SubQuad subQuad_b in subQuads)
            {
                if (subQuad_a.b == subQuad_b.d)
                {
                    subQuad_a.neighbors[0] = subQuad_b;
                    if (!subQuad_a.neighborVertices.ContainsKey(subQuad_b))
                    {
                        subQuad_a.neighborVertices.Add(subQuad_b, new Vertex[] { subQuad_a.b, subQuad_a.a });
                        Debug.Log($"Added neighborVertices for subQuad_a: {subQuad_a}, neighbor: {subQuad_b}, direction: 0");
                    }
                    else
                    {
                        Debug.LogWarning($"Key {subQuad_b} already exists in neighborVertices for subQuad_a: {subQuad_a}, direction: 0. Skipping.");
                    }
                    break;
                }
            }
            
            foreach (SubQuad subQuad_b in subQuads)
            {
                if (subQuad_a.d == subQuad_b.b)
                {
                    subQuad_a.neighbors[3] = subQuad_b;
                    if (!subQuad_a.neighborVertices.ContainsKey(subQuad_b))
                    {
                        subQuad_a.neighborVertices.Add(subQuad_b, new Vertex[] { subQuad_a.b, subQuad_a.a });
                        Debug.Log($"Added neighborVertices for subQuad_a: {subQuad_a}, neighbor: {subQuad_b}, direction: 3");
                    }
                    else
                    {
                        Debug.LogWarning($"Key {subQuad_b} already exists in neighborVertices for subQuad_a: {subQuad_a}, direction: 3. Skipping.");
                    }
                    break;
                }
            }
        }
    }
}



public class vertex_Mid : Vertex
{
    public readonly Edge edge;
    public bool isSp = false;
    public vertex_Mid(Edge edge)
    {
        this.edge = edge;
        Vector3 pos = Vector3.zero;
        foreach (var vertex in edge.hexes)
        {
            pos += vertex.initPos;
        }
        initPos = pos / 2;

        currentPosition = initPos;
        // id = Grid.idCount++;
    }
    public List<Mesh> CreateSideMesh()
    {
        List<Mesh> meshes = new List<Mesh>();
        
        for (int i = 0; i < 4; i++)
        {
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
            meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
            if (subQuads[i].b == this)
            { 
                meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_bc() + Vector3.down * Grid.cellHeight / 2);
                foreach (SubQuad subQuad in subQuads)
                {
                    if (subQuad.c == subQuads[i].c && subQuad ! == subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
                        break; 
                    }
                }
            }
            else
            {
                meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.up * Grid.cellHeight / 2);
                meshVertices.Add(subQuads[i].GetMid_ad() + Vector3.down * Grid.cellHeight / 2);
                foreach (SubQuad subQuad in subQuads)
                {
                    if (subQuad.a == subQuads[i].a && subQuad ! == subQuads[i])
                    {
                        meshVertices.Add(subQuad.GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
                        meshVertices.Add(subQuad.GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
                        break; 
                    }
                }
               
            }
            for (int j = 0; j < meshVertices.Count; j++)
            {
                meshVertices[j] -= currentPosition;
            }
            meshTriangles.Add(0);
            meshTriangles.Add(2);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(3);
            meshTriangles.Add(1);
            meshTriangles.Add(2);
            meshTriangles.Add(4);
            meshTriangles.Add(5);
            meshTriangles.Add(2);
            meshTriangles.Add(5);
            meshTriangles.Add(3);
            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            meshes.Add(mesh);
        }
        return meshes;

    }
}

public class vertex_center : Vertex
{
    public List<Mesh> CreateSideMesh()
    {
        int n = this.subQuads.Count;
        List<Mesh> meshes = new List<Mesh>();
        
        for (int i = 0; i < n; i++)
        {
           List<Vector3> meshVertices = new List<Vector3>();
           List<int> meshTriangles = new List<int>();
           meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
           meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.up * Grid.cellHeight / 2);
           meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPos() + Vector3.up * Grid.cellHeight / 2);
           meshVertices.Add(subQuads[i].GetCenterPos() + Vector3.down * Grid.cellHeight / 2);
           meshVertices.Add(subQuads[i].GetMid_cd() + Vector3.down * Grid.cellHeight / 2);
           meshVertices.Add(subQuads[(i + n - 1) % n].GetCenterPos() + Vector3.down * Grid.cellHeight / 2);

           for (int j = 0; j < meshVertices.Count; j++)
           {
               meshVertices[j] -= currentPosition;
           }
           meshTriangles.Add(0);
           meshTriangles.Add(1);
           meshTriangles.Add(3);
           meshTriangles.Add(1);
           meshTriangles.Add(4);
           meshTriangles.Add(3);
           meshTriangles.Add(1);
           meshTriangles.Add(2);
           meshTriangles.Add(5);
           meshTriangles.Add(1);
           meshTriangles.Add(5);
           meshTriangles.Add(4);
           Mesh mesh = new Mesh();
           mesh.vertices = meshVertices.ToArray();
           mesh.triangles = meshTriangles.ToArray();
           meshes.Add(mesh);
        }
        return meshes;

    }
}
    
public class vertex_triangleCenter : vertex_center
    {
        public vertex_triangleCenter(Triangle triangle)
        {
            initPos = (triangle.a.initPos + triangle.b.initPos + triangle.c.initPos) / 3;
            currentPosition = initPos;
        }
    }
    
public class vertex_quadCenter : vertex_center
    {
        public vertex_quadCenter(Quad quad)
        {
            initPos = (quad.a.initPos + quad.b.initPos + quad.c.initPos + quad.d.initPos) / 4;
            currentPosition = initPos;
        }
    }
    
public class Vertex_Y: Vertex
{
    public Vertex vertex;
    public readonly int y;
    public readonly string name;
    public float Height;
    public readonly bool isBoundary; 
    public Vector3 worldPosition;
    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();
    public Vertex_Y(Vertex vertex, float height)
    {
        this.vertex = vertex;
        this.Height = height;
        this.y = (int)height;  // 设置y值
        name = "Vertex_"+ vertex.index + "_" + y;
        isBoundary = vertex.isBoundary || this.y == Grid.height -1 || this.y == 0;
        worldPosition = vertex.currentPosition + new Vector3(0, Grid.cellHeight * height, 0);
        currentPosition = worldPosition;
    }
}
    


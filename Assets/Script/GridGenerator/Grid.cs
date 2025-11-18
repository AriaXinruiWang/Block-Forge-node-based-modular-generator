using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid 
{
    //---------- 网格配置参数 ----------
    public static int radius;
    public static float cellSize;
    public static float cellHeight;
    public static int height;
    public static int idCount = 0;
    public static int edgeIdCount = 0;
    public int smoothTime;
    
    public readonly List<vertex_Hex> Hexes = new List<vertex_Hex>();
    public readonly List<Triangle> triangles = new List<Triangle>();
    
    public static List<Edge>  edges = new List<Edge>();
    public readonly List<Quad> quads = new List<Quad>();
    public readonly List<SubQuad> subquads = new List<SubQuad>();
    
    public readonly List<Vertex> allVertices = new List<Vertex>();
    public readonly List<vertex_center> centerVertices = new List<vertex_center>();
    public readonly List<vertex_Mid> midVertices = new List<vertex_Mid> ();
    public readonly List<SubQuad_Cube> subQuadCubes = new List<SubQuad_Cube>();
    public readonly List<Vertex_Y> vertexYList = new List<Vertex_Y>();
    public readonly List<SubQuad_Cube> subquad_Cubes = new List<SubQuad_Cube>();
    public Grid(int radius,float cellSize,float cellHeight,int height,int smoothTime)
    {   
        Grid.radius = radius;
        Grid.cellSize = cellSize;
        Grid.cellHeight = cellHeight;
        Grid.height = height;
        this.smoothTime = smoothTime;
        // vertex_Hex.Hex(Hexes);
        // Triangle.Triangle_Hex(Hexes,triangles);
        Hex();   //六边形点阵
        triangle_Hex();  //三角形

        MergeTriangles();

        SubDividedQuadAndTriangle();

        SmoothVertex();
        
        Build3dGrid();
        
        BuildCube();
    }
    
    //初始化六边形网格矩阵
    public void Hex()
    {
        foreach (Coord coord in Coord.Coord_Hex())
        {
            Hexes.Add(new vertex_Hex(coord));
        }

        allVertices.AddRange(Hexes);
    }

    //初始化三角形
    public void triangle_Hex()
    {
        for (int i = 1; i <= Grid.radius; i++)
        {
           triangles.AddRange(Triangle.Triangle_Ring(i, Hexes));
        }
    }
    
    public static Edge GetEdge(vertex_Hex vertexA,vertex_Hex vertexB,Triangle triangle = null)
    {
        foreach (var edge in edges)
        {
            if(edge.CheckRepeatEdge(vertexA,vertexB))
            {
                if(triangle!=null)
                    //？查找到重复边意味着找到了相邻三角形
                    edge.FindAdjacentedTriangle(triangle);
                return edge;
            }
        }
        Edge newEdge = new Edge(vertexA, vertexB, triangle);
        edges.Add(newEdge);
        return newEdge;
    }

    public void MergeTriangles()
    {
        var random = new System.Random();
        var resultList = new List<Edge>();
    
        for (int i = 0; i < edges.Count; i++)
        {
            resultList.Add(edges[i]);
        }
    
        while(resultList.Count>0)
        {
            int index = UnityEngine.Random.Range(0, resultList.Count);
    
            if (resultList[index].CanMerge())
            {
                quads.Add(resultList[index].Merge(triangles));
                edges.Remove(resultList[index]);
            }
            resultList.Remove(resultList[index]);
        }

        foreach (var edge in edges)
        {
            allVertices.Add(edge.vertexMid);
        }
    }
    
    public void SubDividedQuadAndTriangle()
    {
    
        foreach (var triangle in triangles)
        {
            subquads.AddRange(triangle.dividedToSubQuad());
            // centerVertices.Add(triangle.triangelCenter);
            allVertices.Add(triangle.triangleCenter);
        }
    
        foreach (var quad in quads)
        {
            subquads.AddRange(quad.dividedToSubQuad());
            //centerVertices.Add(quad.quadCenter);
            allVertices.Add(quad.quadCenter);
        }
    }
    
    public void SubQuadSmooth()
    {
        foreach (var subquad in subquads)
        {
            subquad.CaculateSmooth();
        }
    }
    
    public void SmoothVertex()
    {
        for (int i = 0; i < smoothTime; i++)
        {
            SubQuadSmooth();
        }
    
        foreach (var vertex in allVertices)
        {
            vertex.Smooth();
        }
    
        foreach(var subquad in subquads)
        {
            subquad.GetArrowPos();
        }
    }

     public void Build3dGrid()
     {
         for (int i = 0; i < height; i++)
         {
             foreach (var vertex in allVertices)
             {
                 vertex.index = allVertices.IndexOf(vertex);
                 vertex.BoundaryCheck();
                 //0702new
                 if (vertex is vertex_Hex)
                 {
                     ((vertex_Hex)vertex).NeighborSubQuadCheck();
                 }
                 
                 Vertex_Y newVertexY = new Vertex_Y(vertex, i);
                 vertex.vertex_Ys.Add(newVertexY);
                 vertexYList.Add(newVertexY);
             }

             foreach (SubQuad subQuad in subquads)
             {
                 foreach (SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
                 {
                     subQuad_Cube.NeighborsCheck();
                 }
                 
             }
         }
        // allVertices.AddRange(vertexYList);
    }
    
    public void BuildCube()
    {
        for (int i = 0; i < height-1; i++)
        {
            foreach (var subQuad in subquads)
            {
                SubQuad_Cube newCube = new SubQuad_Cube(subQuad,i,subQuad_Cubes:subQuadCubes);//0702
                subQuadCubes.Add(newCube);
                newCube.UpdateBitValue();
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Triangle 
{
    public readonly vertex_Hex a;
    public readonly vertex_Hex b;
    public readonly vertex_Hex c;
    
    public readonly vertex_Hex[] vertices;
    public readonly Edge ab;
    public readonly Edge bc;
    public readonly Edge ac;
    
    public readonly Edge[] edges;

    public readonly vertex_triangleCenter triangleCenter;

    public bool isMerged = false;

    public Triangle(vertex_Hex a, vertex_Hex b, vertex_Hex c,List<Triangle> triangles)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        triangles.Add(this);
        ab = Grid.GetEdge(a, b, triangle:this);
        bc = Grid.GetEdge(b, c, this);
        ac = Grid.GetEdge(a, c, this);
        edges = new Edge[] { ab, bc, ac };
        vertices = new vertex_Hex[] { a, b, c }; 
        triangleCenter = new vertex_triangleCenter(this);
    }
    
    public vertex_Hex GetSingleVertex(Edge edge)
    {
        HashSet<vertex_Hex> exception = new HashSet<vertex_Hex>(vertices);
        exception.ExceptWith(edge.hexes);
        return exception.Single();
    }
    
    public static List<Triangle>  Triangle_Ring(int radius,List<vertex_Hex> vertices)
    {  
        // if (radius == 0) return;
        List<vertex_Hex> inner = vertex_Hex.GrabRing(radius-1,vertices);
        List<vertex_Hex> outer = vertex_Hex.GrabRing(radius,vertices);
        List<Triangle> triangles = new List<Triangle>();
        
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {   //创建两个顶点在外圈，一个顶点在内圈的三角形i*radius;
                vertex_Hex a = outer[(i* radius + j)%outer.Count];
                vertex_Hex b = outer[(i * radius + j + 1)%outer.Count];
                vertex_Hex c = inner[(i * (radius - 1) + j)%inner.Count];
                new Triangle(a,b,c,triangles);
                //创建一个顶点在内圈，一个顶点在外圈的三角形
                if(j > 0)
                {
                    vertex_Hex d = inner[(i * (radius - 1) + j - 1)%inner.Count];
                    new Triangle(a,c,d,triangles);
                }
            }
        }
        return triangles;
    }
    
    public List<SubQuad> dividedToSubQuad()
    {
        List<SubQuad> subquads = new List<SubQuad>();
        ab.vertexMid.isSp = true;
        bc.vertexMid.isSp = true;
        ac.vertexMid.isSp = true;
        subquads.Add(new SubQuad(a, ab.vertexMid, triangleCenter, ac.vertexMid));
        subquads.Add(new SubQuad(b, bc.vertexMid, triangleCenter, ab.vertexMid));
        subquads.Add(new SubQuad(c, ac.vertexMid, triangleCenter, bc.vertexMid));
        a.subQuads.Add(subquads[0]);
        b.subQuads.Add(subquads[1]);
        c.subQuads.Add(subquads[2]);
        triangleCenter.subQuads.Add(subquads[0]);
        triangleCenter.subQuads.Add(subquads[1]);
        triangleCenter.subQuads.Add(subquads[2]);
        ab.vertexMid.subQuads.Add(subquads[0]);
        ab.vertexMid.subQuads.Add(subquads[1]);
        bc.vertexMid.subQuads.Add(subquads[1]);
        bc.vertexMid.subQuads.Add(subquads[2]);
        ac.vertexMid.subQuads.Add(subquads[2]);
        ac.vertexMid.subQuads.Add(subquads[0]);
        
        subquads[0].neighbors[1] = subquads[1];
        subquads[0].neighborVertices.Add(subquads[1],new Vertex[]{ab.vertexMid,triangleCenter});
        subquads[0].neighbors[2] = subquads[2];
        subquads[0].neighborVertices.Add(subquads[2],new Vertex[]{ac.vertexMid,triangleCenter});
        subquads[1].neighbors[1] = subquads[2];
        subquads[1].neighborVertices.Add(subquads[2],new Vertex[]{bc.vertexMid,triangleCenter});
        subquads[1].neighbors[2] = subquads[0];
        subquads[1].neighborVertices.Add(subquads[0],new Vertex[]{ab.vertexMid,triangleCenter});
        subquads[2].neighbors[1] = subquads[0];
        subquads[2].neighborVertices.Add(subquads[0],new Vertex[]{ac.vertexMid,triangleCenter});
        subquads[2].neighbors[2] = subquads[1];
        subquads[2].neighborVertices.Add(subquads[1],new Vertex[]{bc.vertexMid,triangleCenter});
        
        return subquads;
    }
    // public static void Triangle_Hex(List<vertex_Hex> vertices,List<Triangle> triangles)
    // {
    //     for (int i = 1; i <= Grid.radius; i++)
    //     {
    //         Triangle_Ring(i,vertices,triangles);
    //     }
    // }
}

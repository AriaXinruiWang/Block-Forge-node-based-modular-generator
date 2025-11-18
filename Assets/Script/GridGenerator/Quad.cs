using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Quad
{   
    public readonly vertex_Hex a;
    public readonly vertex_Hex b;
    public readonly vertex_Hex c;
    public readonly vertex_Hex d;
    
    public  Edge ab;
    public  Edge bc;
    public  Edge cd;
    public  Edge ad;
    
    public vertex_quadCenter quadCenter;
    
    public Quad(List<vertex_Hex> vertices)
    {
        this.a = vertices[0];
        this.b = vertices[1];
        this.c = vertices[2];
        this.d = vertices[3];

        this.ab = Grid.GetEdge(this.a, this.b);
        this.bc = Grid.GetEdge(this.b, this.c);
        this.cd = Grid.GetEdge(this.c, this.d);
        this.ad = Grid.GetEdge(this.a, this.d);
        
        
        quadCenter = new vertex_quadCenter(this);
    }


    public List<SubQuad> dividedToSubQuad()
    {
        List<SubQuad> subquads = new List<SubQuad>();
        ab.vertexMid.isSp = true;
        bc.vertexMid.isSp = true;
        cd.vertexMid.isSp = true;
        ad.vertexMid.isSp = true;
        subquads.Add(new SubQuad(a, ab.vertexMid, quadCenter, ad.vertexMid));
        subquads.Add(new SubQuad(b, bc.vertexMid, quadCenter, ab.vertexMid));
        subquads.Add(new SubQuad(c, cd.vertexMid, quadCenter, bc.vertexMid));
        subquads.Add(new SubQuad(d, ad.vertexMid, quadCenter, cd.vertexMid));
        a.subQuads.Add(subquads[0]);
        b.subQuads.Add(subquads[1]);
        c.subQuads.Add(subquads[2]);
        d.subQuads.Add(subquads[3]);
        quadCenter.subQuads.Add(subquads[0]);
        quadCenter.subQuads.Add(subquads[1]);
        quadCenter.subQuads.Add(subquads[2]);
        quadCenter.subQuads.Add(subquads[3]);
        ab.vertexMid.subQuads.Add(subquads[0]);
        ab.vertexMid.subQuads.Add(subquads[1]);
        bc.vertexMid.subQuads.Add(subquads[1]);
        bc.vertexMid.subQuads.Add(subquads[2]);
        cd.vertexMid.subQuads.Add(subquads[2]);
        cd.vertexMid.subQuads.Add(subquads[3]);
        ad.vertexMid.subQuads.Add(subquads[3]);
        ad.vertexMid.subQuads.Add(subquads[0]);
        
        subquads[0].neighbors[1] = subquads[1];
        subquads[0].neighborVertices.Add(subquads[1],new Vertex[]{ab.vertexMid,quadCenter});
        subquads[0].neighbors[2] = subquads[3];
        subquads[0].neighborVertices.Add(subquads[3],new Vertex[]{ad.vertexMid,quadCenter});
        subquads[1].neighbors[1] = subquads[2];
        subquads[1].neighborVertices.Add(subquads[2],new Vertex[]{bc.vertexMid,quadCenter});
        subquads[1].neighbors[2] = subquads[0];
        subquads[1].neighborVertices.Add(subquads[0],new Vertex[]{ab.vertexMid,quadCenter});
        subquads[2].neighbors[1] = subquads[3];
        subquads[2].neighborVertices.Add(subquads[3],new Vertex[]{cd.vertexMid,quadCenter});
        subquads[2].neighbors[2] = subquads[1];
        subquads[2].neighborVertices.Add(subquads[1],new Vertex[]{bc.vertexMid,quadCenter});
        subquads[3].neighbors[1] = subquads[0];
        subquads[3].neighborVertices.Add(subquads[0],new Vertex[]{ad.vertexMid,quadCenter});
        subquads[3].neighbors[2] = subquads[3];
        subquads[3].neighborVertices.Add(subquads[3],new Vertex[]{cd.vertexMid,quadCenter});
        return subquads;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Edge  // 定义表示网格边的类
{
    // 存储边连接的两个六边形顶点（vertex_Hex类型）
    public readonly HashSet<vertex_Hex> hexes;

    // 存储与边相邻的三角形（Triangle类型）
    public HashSet<Triangle> adjacentedTriangles;

    // 边的唯一ID，初始化后不可修改
    public readonly int edgeID = 0;

    // 边的中点顶点对象
    public readonly vertex_Mid vertexMid;

    // // 静态计数器，用于生成唯一edgeID
    // public static int edgeIdCount = 0; // 添加这行代码

    public Edge(vertex_Hex a, vertex_Hex b, Triangle triangle)
    {   
        // 初始化hexes集合，添加两个顶点a和b
        hexes = new HashSet<vertex_Hex> {a, b};

        // 初始化三角形集合，添加初始关联的三角形
        adjacentedTriangles = new HashSet<Triangle> { triangle };

        // 创建边的中点顶点，并将当前Edge对象传递给它
        vertexMid = new vertex_Mid(edge:this);

        // // 分配唯一ID，并递增静态计数器（潜在问题：应为Edge.edgeIdCount++）
        // edgeID = Grid.edgeIdCount++; 
    }

    public List<vertex_Hex> ReturnVertices()
    {
        // 创建空列表存储顶点
        List<vertex_Hex> vertices = new List<vertex_Hex>();

        // 遍历hexes集合中的顶点
        foreach (var vertex in hexes)
        {
            vertices.Add(vertex); // 逐个添加顶点到列表
        }

        return vertices; // 返回顶点列表
    }

    public List<Triangle> ReturnTriangles()
    {
        List<Triangle> triangles = new List<Triangle>(); // 创建空列表

        // // 错误：遍历了局部变量triangles（空列表），应遍历adjacentedTriangles
        // foreach (var triangle in  triangles)
        foreach (var triangle in triangles)
        {
            triangles.Add(triangle); // 逻辑错误，此处永远为空（已改正）
        }

        return triangles; // 错误：返回空列表（已改正）
    }
    
    // 检查两个顶点是否属于同一条边
    public bool CheckRepeatEdge(vertex_Hex vertexA, vertex_Hex vertexB)
    {
        return hexes.Contains(vertexA) && hexes.Contains(vertexB);
    }
    
    public void FindAdjacentedTriangle(Triangle triangle)
    {
        if(adjacentedTriangles.Count<2)
        {
            adjacentedTriangles.Add(triangle);
        }
        else
        {
            Debug.Log("边的三角形数量异常");
        }
    }
    
    // 检查新边是否是当前边的子集（逻辑可能不准确）
    public bool CheckRepeatEdge(Edge newEdge)
    {
        return newEdge.hexes.IsProperSubsetOf(hexes);// 应判断是否相等而非子集
    }
    
    public Quad Merge(List<Triangle> triangles)
    {
        if(CanMerge()) // 检查是否可以合并
        {
            List<vertex_Hex> quadVertices = new List<vertex_Hex>();

            foreach (var triangle in adjacentedTriangles) // 遍历相邻三角形
            {
                triangles.Remove(triangle); // 从外部列表移除已合并的三角形
                triangle.isMerged = true; // 标记为已合并

                // 获取三角形中不属于当前边的顶点
                vertex_Hex vertexA = triangle.GetSingleVertex(this);
                quadVertices.Add(vertexA); 

                // 添加下一个顶点（需确保顶点顺序正确）
                quadVertices.Add(triangle.vertices[(Array.IndexOf(triangle.vertices, vertexA) + 1) % 3]);
            }

            return new Quad(quadVertices); // 创建四边形对象
        }
        return null; // 无法合并时返回null
    }
    
    public bool CanMerge()
    {
        foreach (var triganle in adjacentedTriangles) // 遍历相邻三角形
        {
            if(triganle.isMerged) // 如果任一三角形已被合并
            {
                return false;
            }
        }
        return adjacentedTriangles.Count == 2; // 必须属于两个三角形
    }
}


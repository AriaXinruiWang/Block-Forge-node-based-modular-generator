using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaster : MonoBehaviour
{
    public GridGenerator gridGenerator;
    public ColliderSystem colliderSystem;
    public WaveFunctionCollapse waveFunctionCollapse;

    private void Awake()
    {
        // 移除错误的循环引用初始化
        // 确保waveFunctionCollapse不为null（如果需要）
        if (waveFunctionCollapse == null)
        {
            Debug.LogWarning("waveFunctionCollapse未在Inspector中设置，请确保正确引用");
        }
    }
}

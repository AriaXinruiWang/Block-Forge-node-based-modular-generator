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
        gridGenerator = GetComponentInParent<WorldMaster>().gridGenerator;
        colliderSystem = GetComponentInParent<WorldMaster>().colliderSystem;
    }
}

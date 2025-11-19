using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum RaycastHitType
{
   ground,top,bottom,side,none
}
public class Clicker : MonoBehaviour
{
   private GridGenerator gridGenerator;
   private WaveFunctionCollapse waveFunctionCollapse;
   private ColliderSystem colliderSystem;
   private SlotColliderSystem slotColliderSystem;
   private PlayerInputActions inputActions;
   private RaycastHit raycastHit;
   private RaycastHitType raycastHitType;
   [SerializeField] private float raycastRange;
   [SerializeField] private LayerMask clickLayerMask;
   [SerializeField] private Cursor cursor;
   private Vertex_Y vertex_Y_Target;
   private Vertex_Y vertex_Y_Selected;
   private Vertex_Y vertex_Y_pre_Selected;
   private Vertex_Y vertex_Y_pre_Target;

   private void Awake()
   {
      gridGenerator = GetComponentInParent<WorldMaster>().gridGenerator;
      colliderSystem = GetComponentInParent<WorldMaster>().colliderSystem;
      slotColliderSystem = colliderSystem.GetSlotColliderSystem();
      waveFunctionCollapse = GetComponentInParent<WorldMaster>().waveFunctionCollapse;

      inputActions = new PlayerInputActions();
      inputActions.Build.Enable();
      inputActions.Build.Add.performed += Add;
      inputActions.Build.Delete.performed += Delete;
   }

   private void Update()
   {
      FindTarget();
      UpdateCursor();

      // 检测鼠标右键点击，用于选择对象 new
      if (Input.GetMouseButtonDown(1) && vertex_Y_Target != null)
      {
         vertex_Y_Selected = vertex_Y_Target;
         Debug.Log("已选择顶点: " + vertex_Y_Selected.worldPosition);
      }
   }

   //0505new
   public void FindTarget()
   {
      vertex_Y_pre_Selected = vertex_Y_Selected;
      vertex_Y_pre_Target = vertex_Y_Target;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out raycastHit, raycastRange, clickLayerMask))
      {
         if (raycastHit.transform.GetComponent<GroundCollider_Quad>())
         {
            vertex_Y_Selected = null;
            //与地面相交，计算目标块
            Vector3 aim = raycastHit.point;
            SubQuad subQuad = raycastHit.transform.GetComponent<GroundCollider_Quad>().subQuad;

            Vector3 a = subQuad.a.currentPosition;
            Vector3 b = subQuad.b.currentPosition;
            Vector3 c = subQuad.c.currentPosition;
            Vector3 d = subQuad.d.currentPosition;

            Vector3 ab = (a + b) / 2;
            Vector3 bc = (b + c) / 2;
            Vector3 cd = (c + d) / 2;
            Vector3 da = (d + a) / 2;

            float ab_cd = (aim.z - ab.z) * (aim.x - cd.x) - (aim.x - ab.x) * (aim.z - cd.z);
            float bc_da = (aim.z - bc.z) * (aim.x - da.x) - (aim.x - bc.x) * (aim.z - da.z);

            float a_ab_cd = (a.z - ab.z) * (a.x - cd.x) - (a.x - ab.x) * (a.z - cd.z);
            float a_bc_da = (a.z - bc.z) * (a.x - da.x) - (a.z - da.z) * (a.x - bc.x);
            // float b_bc_da = (b.z - bc.z)*(b.x-da.x)-(b.x-bc.x)*(b.z-da.z);
            // float c_cd_ab = (c.z - cd.z)*(c.x-ab.x)-(c.x-cd.x)*(c.z-ab.z);
            // float d_da_bc = (d.z - da.z)*(d.x-bc.x)-(d.x-da.x)*(d.z-bc.z);

            bool on_ad_side = ab_cd * a_ab_cd >= 0;
            bool on_ab_side = bc_da * a_bc_da >= 0;

            if (on_ad_side && on_ab_side)
            {
               vertex_Y_Target = subQuad.a.vertex_Ys[1];
            }
            else if (on_ad_side && !on_ab_side)
            {
               vertex_Y_Target = subQuad.d.vertex_Ys[1];
            }
            else if (!on_ad_side && on_ab_side)
            {
               vertex_Y_Target = subQuad.b.vertex_Ys[1];
            }
            else
            {
               vertex_Y_Target = subQuad.c.vertex_Ys[1];
            }

            if (vertex_Y_Target.vertex.isBoundary)
            {
               vertex_Y_Target = null;
               raycastHitType = RaycastHitType.none;
            }
            else
            {
               raycastHitType = RaycastHitType.ground;
            }
         }
         else
         {
            vertex_Y_Selected = raycastHit.transform.parent.GetComponent<SlotCollider>().vertex_Y;
            int y = vertex_Y_Selected.y;
            Debug.unityLogger.Log(raycastHit.transform.name);
            if(raycastHit.transform.GetComponent<SlotCollider_Top>())
            {
               if( y < Grid.height-2)
               {
                  vertex_Y_Target = vertex_Y_pre_Selected.vertex.vertex_Ys[y + 1];
                  raycastHitType = RaycastHitType.top;
               }
               else
               {
                  vertex_Y_Target = null;
                  raycastHitType = RaycastHitType.none;
               }
            }
            else if(raycastHit.transform.GetComponent<SlotCollider_Bottom>())
            {
               if (y > 1)
               {
                  vertex_Y_Target = vertex_Y_pre_Selected.vertex.vertex_Ys[y - 1];
                  raycastHitType = RaycastHitType.bottom;
               }
               else
               {
                  vertex_Y_Target = null;
                  raycastHitType = RaycastHitType.none;
               }
            }
            else
            {
               vertex_Y_Target = raycastHit.transform.GetComponent<SlotCollider_Side>().neighbor;
               if (vertex_Y_Target.vertex.isBoundary)
               {
                  vertex_Y_Target = null;
                  raycastHitType = RaycastHitType.none;
               }
               else
               {
                  raycastHitType = RaycastHitType.side;
               }
            }
         }
      }
      
      else
      {
         // 如果没有击中任何物体，则将target设为null
         vertex_Y_Target = null;
         vertex_Y_Selected = null;
         raycastHitType = RaycastHitType.none;
      }
   }

   private void UpdateCursor()
   {
      if (cursor == null)
      {
        Debug.LogWarning("Cursor 未赋值！");
        return;
      }
      if (vertex_Y_pre_Target != vertex_Y_Target || vertex_Y_pre_Selected != vertex_Y_Selected)
      {
         Debug.Log("target: " + vertex_Y_Target);
         Debug.Log("target.vertex: " + (vertex_Y_Target != null ? vertex_Y_Target.vertex.ToString() : "null"));
         Debug.Log("subQuads count: " + (vertex_Y_Target != null && vertex_Y_Target.vertex != null ? vertex_Y_Target.vertex.subQuads.Count : -1));
         
         // 新增 MeshFilter 判空
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            Debug.Log("mesh vertex count: " + meshFilter.mesh.vertexCount);
        }
        else
        {
            Debug.LogWarning("未找到 MeshFilter 或 mesh 为空！");
        }
         cursor.UpdateCursor(raycastHit, raycastHitType,vertex_Y_Selected, vertex_Y_Target);
      }
   }

   private void Add(InputAction.CallbackContext context)
   {
      if (vertex_Y_Target != null && !vertex_Y_Target.isActive)
      {
         gridGenerator.ToggleSlot(vertex_Y_Target);
         slotColliderSystem.CreateCollider(vertex_Y_Target);
         waveFunctionCollapse.WFC();
      }
   }

   private void Delete(InputAction.CallbackContext context)
   {
      Debug.Log("OnDelete");
      if (vertex_Y_Selected != null && !vertex_Y_Selected.isActive)
      {
         gridGenerator.ToggleSlot(vertex_Y_Selected);
         slotColliderSystem.DestroyCollider(vertex_Y_Selected);
         waveFunctionCollapse.WFC();
      }
   }

   private void OnDrawGizmos()
   {
      // 显示目标顶点
      if (vertex_Y_Target != null)
      {
         Gizmos.color = Color.red;
         Gizmos.DrawSphere(vertex_Y_Target.worldPosition, 0.1f);
      }

      // 显示选中的顶点
      if (vertex_Y_Selected != null)
      {
         Gizmos.color = Color.green;
         Gizmos.DrawSphere(vertex_Y_Selected.worldPosition, 0.15f);
      }
   }
}
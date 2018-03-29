using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {

    [SerializeField, Tooltip("Radius or max distance the 'player' can see")] private float viewRadius = 50f;
    [SerializeField, Range(0, 360), Tooltip("Wideness of the field of view")] private float viewAngle = 90f;
    [SerializeField, Tooltip("Affects the ray casted out when recalculating the fov. Raycast count = viewAngle * meshResolution")] private int meshResolution = 10;
    [SerializeField, Tooltip("Iterations of the edge resolving algorithm (higher = more precise but also more costly)")] private int edgeResolveIterations = 1;
    [SerializeField] private float edgeDstThreshold;

    [SerializeField, Range(0, 1), Tooltip("Delay between field of view updates")] private float delayBetweenFOVUpdates = 0.2f;

    [SerializeField, Tooltip("Objects that are effected when entering/exiting the fov. These MUST IMPLEMENT the IHideable interface")] private LayerMask targetMask;
    [SerializeField, Tooltip("Objects that block the field of view")] private LayerMask obstacleMask;

    [SerializeField, Tooltip("Mesh Filter component that holds the generated mesh when drawing the field of view")] private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    [SerializeField, Tooltip("Mesh Collider component that holds the generated mesh when drawing the field of view")] private MeshCollider viewMeshCollider;


    //variable is used in the DrawFieldOfView method (storing it here it way more efficient - GC.collect...)
    private List<Vector3> viewPoints = new List<Vector3>();


    private void Start() {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        viewMeshCollider.sharedMesh = viewMesh;
    }
    void OnEnable() {
        StartCoroutine("FindTargetsWithDelay", delayBetweenFOVUpdates);
    }

    private void LateUpdate() {
        DrawFieldOfView();
    }


    /// <summary>
    /// Draw the field of view.
    /// </summary>
    void DrawFieldOfView() {
        //int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        viewPoints.Clear();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= Mathf.RoundToInt(viewAngle * meshResolution); i++) {
            //float angle = transform.eulerAngles.y - viewAngle / 2 + (viewAngle / Mathf.RoundToInt(viewAngle * meshResolution)) * i;
            ViewCastInfo newViewCast = ViewCast(transform.eulerAngles.y - viewAngle / 2 + (viewAngle / Mathf.RoundToInt(viewAngle * meshResolution)) * i);

            if (i > 0) {
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDstThreshold)) {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        //Draw mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// Cast out a ray at a given angle and return a ViewCastInfo struct as a result.
    /// </summary>
    /// <param name="globalAngle"></param>
    /// <returns></returns>
    ViewCastInfo ViewCast(float globalAngle) {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask)) {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        } else {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }
    /// <summary>
    /// Finds the edge of a collider
    /// </summary>
    /// <param name="minViewCast"></param>
    /// <param name="maxViewCast"></param>
    /// <returns></returns>
	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    /// <summary>
    /// Run the find visible targets method every x seconds/ms
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator FindTargetsWithDelay(float delay) {
        while (true) {
            FindVisibleTargets();
            yield return new WaitForSeconds(delay);
        }
    }
    /// <summary>
    /// Finds all visible targets and adds them to the visibleTargets list.
    /// </summary>
    void FindVisibleTargets() {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++) {
            Transform target = targetsInViewRadius[i].transform;
            bool isInFOV = false;

            //check if hideable should be hidden or not
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask)) {
                    isInFOV = true;
                }
            }

            //apply effect to IHideable
            IHideable hideable = target.GetComponent<IHideable>();
            if (hideable != null) {
                if (isInFOV) {
                    target.GetComponent<IHideable>().OnFOVEnter();
                } else {
                    target.GetComponent<IHideable>().OnFOVLeave();
                }
            }
        }
    }

    /// <summary>
    /// Convert an angle to a direction vector.
    /// </summary>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    public Vector3 DirFromAngle(float angleInDegrees, bool IsAngleGlobal) {
        if (!IsAngleGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}


/// <summary>
/// Struct used to store information about a view raycast
/// </summary>
public struct ViewCastInfo {
    public bool hit;
    public Vector3 point;
    public float distance;
    public float angle;

    public ViewCastInfo(bool hit, Vector3 point, float distance, float angle) {
        this.hit = hit;
        this.point = point;
        this.distance = distance;
        this.angle = angle;
    }
}
/// <summary>
/// Stcuct that hold information about an edge
/// </summary>
public struct EdgeInfo {
    public Vector3 pointA;
    public Vector3 pointB;

    public EdgeInfo(Vector3 pointA, Vector3 pointB) {
        this.pointA = pointA;
        this.pointB = pointB;
    }
}
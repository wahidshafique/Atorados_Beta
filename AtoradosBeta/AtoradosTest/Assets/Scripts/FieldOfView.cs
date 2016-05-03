using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class FieldOfView : MonoBehaviour {

    public float viewRadius;
    [Range(0,360)] 
    public float viewAngle;
      
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector] public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution; // number of rays to create
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    public int edgeResolveIterations;
    public float edgeDstThreshold; 


    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh; 

        StartCoroutine("FindTargetsWithDelay", .2f); 
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            findVisibleTargets(); 
        }
    }

    void LateUpdate()
    {
        drawFieldOfView(); 
    }

    void findVisibleTargets()
    {
        visibleTargets.Clear();
      
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized; 
            if (Vector3.Angle(transform.forward, dirToTarget)< viewAngle / 2)
            {
                // target recognition
                float disToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast (transform.position, dirToTarget, disToTarget, obstacleMask))
                {
                    // do Something if has hit target
                    //Debug.Log("has hit target"); 
                    target.GetComponent<MeshRenderer>().material.color = Color.magenta; 
                    visibleTargets.Add(target); // not really needed 
                }               
            }
            else
            {
                target.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
        }
    }

    void drawFieldOfView()
    {
        int rayCount = Mathf.RoundToInt (viewAngle * meshResolution);
        float stepAngleSize = viewAngle / rayCount;      
        List<Vector3> viewPoints = new List<Vector3>();   // List to hold all the points that ViewCast hit so we can draw it

        ViewCastInfo oldViewCast = new ViewCastInfo(); 

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            // Debug.DrawLine(transform.position, transform.position + directionFromAngle(angle, true) * viewRadius, Color.red); 
            ViewCastInfo newViewCast = ViewCast(angle);
          

            if (i > 0 ) // Edge Detection 
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold; 
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge (oldViewCast, newViewCast); 
                        if (edge.pointA != Vector3.zero)
                        {
                            viewPoints.Add(edge.pointA); 
                        }
                        if (edge.pointB != Vector3.zero)
                        {
                            viewPoints.Add(edge.pointB);
                        }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;


        }

        // Creating a Mesh via script 
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; 

        // Set vertices to viewPointList
        for (int i = 0; i < vertexCount - 1; i++)
        {
            // Convert global points into local points
            vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]); 
            // Setting the triangles array x 3 - Each triangle starts at the origin vertex 0  
            if (i < vertexCount - 2)  // OutOfBounds array check
            {               
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

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero; 

        // cast a ray inbetween to find the min/max angle 
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point; 

            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point; 
            }
        }
        return new EdgeInfo(minPoint, maxPoint); 
    }

    ViewCastInfo ViewCast (float globalAngle)
    {
       
        Vector3 dir = directionFromAngle(globalAngle, true);
        RaycastHit hit; 

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            // Has hit obstacle
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            // Has not hit obstacle
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }
       
    public Vector3 directionFromAngle(float angleDegrees, bool angleIsGlobal)
    {
        // Figuring out what direction our angle is facing.
        // Converting degrees into Unity Degree relation - starting point for Unity is 0 deg (not 90 deg)
        if (!angleIsGlobal) 
        {
            angleDegrees += transform.eulerAngles.y; 
        }
        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleDegrees * Mathf.Deg2Rad));  
    }

    // Storing raycast info 
    public struct ViewCastInfo
    {       
        public bool hit;
        public Vector3 point;
        public float dst; 
        public float angle; 

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle; 
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA; // min
        public Vector3 pointB; // max

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB; 
        }
    }
}

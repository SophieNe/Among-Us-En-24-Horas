using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Game
{
	public class FieldOfView : MonoBehaviour
	{
		[HideInInspector] public SpriteRenderer floor;

		public float viewRadius;
		[Range(0, 360)]
		public float viewAngle;

		public LayerMask obstacleMask, targetMask;

		[HideInInspector] public List<Transform> visibleTargets = new List<Transform>();
		List<PlayerScript> visibleTargetScripts = new List<PlayerScript>(); 

		public float meshResolution;
		public int edgeResolveIterations;
		public float edgeDstThreshold;

		public float maskCutawayDst = .1f;

		public MeshFilter viewMeshFilter;
		Mesh viewMesh;

		bool isQuadGenerated = false; // SOLO SI ES IMPOSTOR O MUERTO EEE

		[HideInInspector] public UnityAction drawMeshAction;
		void Start()
		{
			floor = GameObject.FindGameObjectWithTag("Floor").GetComponent<SpriteRenderer>();
			viewMesh = new Mesh();
			viewMesh.name = "View Mesh";
			viewMeshFilter.mesh = viewMesh;
			StartCoroutine(VisibleTargetsDelay(0.05f));
		}
		IEnumerator VisibleTargetsDelay(float delay)
		{
			yield return new WaitForSeconds(10f);
			while (true)
			{
				HideOldTargets();
				FindVisibleTargets();
				DisplayVisibleTargetsName();
				yield return new WaitForSeconds(delay);
			}
		}
		void LateUpdate()
		{
			drawMeshAction?.Invoke();
		}
		void FindVisibleTargets()
		{
			visibleTargets.Clear();
			Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, viewRadius, targetMask);

			for (int i = 0; i < targetsInViewRadius.Length; i++)
			{
				Transform target = targetsInViewRadius[i].transform;
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				if (Vector3.Angle(transform.up, dirToTarget) < viewAngle / 2)
				{
					float dstToTarget = Vector3.Distance(transform.position, target.position);
					if (!Physics2D.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
					{
						visibleTargets.Add(target);
					}
				}
			}
		}
		void HideOldTargets()
		{
			foreach (PlayerScript player in visibleTargetScripts)
			{
				if (player == null)
				{
					continue;
				}
				player?.nameText?.gameObject?.SetActive(false);
			}
			visibleTargetScripts.Clear();
		}
		void DisplayVisibleTargetsName()
		{
			foreach (Transform target in visibleTargets)
			{
				PlayerScript script = target.GetComponent<PlayerScript>();
				if (script != null)
				{
					visibleTargetScripts.Add(script);
					script.nameText.gameObject.SetActive(true);
				}
			}
		}
		public void DrawFullQuad()
		{
			if (!isQuadGenerated)
			{
				Sprite sprite = floor.sprite;
				viewMesh.vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i);
				viewMesh.uv = sprite.uv;
				viewMesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);

				isQuadGenerated = true;
			}
		}

		public void DrawFieldOfView()
		{
			int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
			float stepAngleSize = viewAngle / stepCount;
			List<Vector3> viewPoints = new List<Vector3>();
			ViewCastInfo oldViewCast = new ViewCastInfo();
			for (int i = 0; i <= stepCount; i++)
			{
				float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
				ViewCastInfo newViewCast = ViewCast(angle);

				if (i > 0)
				{
					bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
					if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
					{
						EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
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

			int vertexCount = viewPoints.Count + 1;
			Vector3[] vertices = new Vector3[vertexCount];
			int[] triangles = new int[(vertexCount - 2) * 3];

			vertices[0] = Vector3.zero;
			for (int i = 0; i < vertexCount - 1; i++)
			{
				vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.right * maskCutawayDst;

				if (i < vertexCount - 2)
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


		ViewCastInfo ViewCast(float globalAngle)
		{
			Vector3 dir = DirFromAngle(globalAngle, true);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
			if (hit.collider != null)
			{
				return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
			}
			else
			{
				return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
			}
		}

		public Vector2 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
		{
			if (!angleIsGlobal)
			{
				angleInDegrees -= transform.eulerAngles.z;
			}
			return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
		}

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
			public Vector3 pointA;
			public Vector3 pointB;

			public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
			{
				pointA = _pointA;
				pointB = _pointB;
			}
		}
		public void SetDrawMeshListener(UnityAction action)
		{
			isQuadGenerated = false;
			drawMeshAction = action;
		}
	}
}
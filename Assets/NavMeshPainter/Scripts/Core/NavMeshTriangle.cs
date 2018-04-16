﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.NavMesh
{
    /// <summary>
    /// NavMesh三角面
    /// </summary>
    [System.Serializable]
    public class NavMeshTriangle
    {

        public Vector3 vertex0;
        public Vector3 vertex1;
        public Vector3 vertex2;
        public Vector2 uv0;
        public Vector2 uv1;
        public Vector2 uv2;

        /// <summary>
        /// 节点列表
        /// </summary>
        [SerializeField]
        private List<NavMeshTriangleNode> m_NodeLists;

        /// <summary>
        /// 三角形的AABB包围盒
        /// </summary>
        public Bounds bounds { get { return m_Bounds; } }

        [SerializeField]
        private Bounds m_Bounds;

        [SerializeField] private int m_MaxDepth;

        private NavMeshTriangleNode m_Root;

        public NavMeshTriangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            //NavMeshTriangleNode root = new NavMeshTriangleNode(vertex0, vertex1, vertex2, uv0, uv1, uv2);
            NavMeshTriangleNode root = new NavMeshTriangleNode(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
            m_NodeLists = new List<NavMeshTriangleNode>();
            m_NodeLists.Add(root);

            float maxX = Mathf.Max(vertex0.x, vertex1.x, vertex2.x);
            float maxY = Mathf.Max(vertex0.y, vertex1.y, vertex2.y);
            float maxZ = Mathf.Max(vertex0.z, vertex1.z, vertex2.z);

            float minX = Mathf.Min(vertex0.x, vertex1.x, vertex2.x);
            float minY = Mathf.Min(vertex0.y, vertex1.y, vertex2.y);
            float minZ = Mathf.Min(vertex0.z, vertex1.z, vertex2.z);

            Vector3 si = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
            if (si.x <= 0)
                si.x = 0.1f;
            if (si.y <= 0)
                si.y = 0.1f;
            if (si.z <= 0)
                si.z = 0.1f;
            Vector3 ct = new Vector3(minX, minY, minZ) + si / 2;

            this.m_Bounds = new Bounds(ct, si);

            this.vertex0 = vertex0;
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.uv0 = uv0;
            this.uv1 = uv1;
            this.uv2 = uv2;
        }

        public NavMeshTriangle(Vector2 uv0, Vector2 uv1, Vector2 uv2)
        {
            NavMeshTriangleNode root = new NavMeshTriangleNode(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
            m_NodeLists = new List<NavMeshTriangleNode>();
            m_NodeLists.Add(root);

            this.uv0 = uv0;
            this.uv1 = uv1;
            this.uv2 = uv2;
        }

        public int CheckMaxTriangleNodeCount()
        {
            return m_NodeLists.Count;
        }

        public void Init()
        {
            if (m_NodeLists.Count >= 1)
                m_NodeLists[0].Init(m_NodeLists);
            m_Root = m_NodeLists[0];
            m_NodeLists = null;
        }

        public void Clear()
        {
            m_Root = new NavMeshTriangleNode(new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
        }

        public void Save()
        {
            if (m_Root != null)
            {
                m_NodeLists = new List<NavMeshTriangleNode>();
                m_NodeLists.Add(m_Root);
                m_Root.Save(m_NodeLists);
            }
        }

        /// <summary>
        /// 节点细分
        /// </summary>
        /// <param name="maxDepth">最大深度</param>
        /// <param name="maxArea">最大三角面积</param>
        public void SetMaxDepth(int maxDepth, float maxArea, bool forceSet = false)
        {
            if (forceSet)
            {
                m_MaxDepth = maxDepth;
                return;
            }
            //根据最大三角面积及最大深度，确定当前三角面的细分深度
            //if (m_NodeLists != null && m_NodeLists.Count >= 1)
            {
                float tcount = Mathf.Pow(4, maxDepth);
                float marea = maxArea/tcount;
                float carea = GetArea();
                for (int i = 0; i <= maxDepth; i++)
                {
                    float dp = Mathf.Pow(4, i);
                    float area = carea / dp;
                    float m = Mathf.Round(area/marea);
                    if (m <= 1.0f)
                    {
                        maxDepth = i;
                        break;
                    }
                }
                m_MaxDepth = maxDepth;
            }
        }

        /// <summary>
        /// 获得当前三角面的面积
        /// </summary>
        /// <returns></returns>
        public float GetArea()
        {
            return Vector3.Cross(vertex1 - vertex0, vertex2 - vertex0).magnitude * 0.5f;
        }


        internal void GenerateMesh(List<NavMeshRenderTriangle> triangles)
        {
            if (m_Root != null)
                m_Root.GenerateMesh(vertex0, vertex1, vertex2, uv0, uv1, uv2, triangles);
        }

        public void SamplingFromTexture(Texture2D texture)
        {
            if (m_Root != null)
                m_Root.SamplingFromTexture(uv0, uv1, uv2, texture, 0, m_MaxDepth);
        }

        public void Interesect(IPaintingTool tool, bool erase)
        {
            if (m_Root != null)
                m_Root.Interesect(tool, erase, vertex0, vertex1, vertex2, 0, m_MaxDepth);
        }

        public void DrawTriangleGizmos()
        {
            if (m_Root != null)
                m_Root.DrawTriangleGizmos(vertex0, vertex1, vertex2);
        }
    }
}
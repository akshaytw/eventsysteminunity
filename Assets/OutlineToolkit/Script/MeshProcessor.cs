using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshProcessor {

    const float vertexEquivalenceMargin = 0.05f;
    const float normalEquivalenceMargin = 0.05f;

    public static Mesh processForOutlineMesh(Mesh mesh) {

        List<Vector2> uv = new List<Vector2>();
        List<Vector3> verts = new List<Vector3>();

        Vector3[] points = mesh.vertices;
        edge[] edges = processForEdges(mesh);

        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i += 3) {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];

            yes(edges, points, a, b, c, uv, verts);
            yes(edges, points, b, c, a, uv, verts);
            yes(edges, points, c, a, b,  uv, verts);
        }

        Mesh ret = new Mesh();
        ret.vertices = verts.ToArray();
        ret.uv = uv.ToArray();
        int[] triangles = new int[verts.Count];
        for (int i = 0; i < triangles.Length; i++) {
            triangles[i] = i;
        }
        ret.triangles = triangles;

        return ret;
    }

    static void yes(edge[] edges, Vector3[] points, int indexX, int indexY, int thirdIndex, List<Vector2> uv, List<Vector3> verts) {

        foreach (edge e in edges) {
            if ((e.a == indexX && e.b == indexY) || (e.b == indexX && e.a == indexY)) {

                Vector3 vX = points[indexX];
                Vector3 vY = points[indexY];
                Vector3 v3 = points[thirdIndex];

                Vector3 a = GetClosestPointOnLine(vX, vY, v3);

                verts.Add(vX); uv.Add(Vector2.zero);
                verts.Add(vY); uv.Add(Vector2.zero);
                verts.Add(v3); uv.Add(Vector2.one * Vector3.Distance(a, v3));

                return;
            } else if ((e.a == indexX || e.b == indexX) && !(e.a == thirdIndex && e.b == thirdIndex)) {

                Vector3 vX = points[indexX];
                Vector3 vY = points[indexY];
                Vector3 v3 = points[thirdIndex];

                Vector3 v4 = (e.a == indexX) ? points[e.b] : points[e.a];

                Vector3 aY = GetClosestPointOnLine(vX, v4, vY);
                Vector3 a3 = GetClosestPointOnLine(vX, v4, v3);

                verts.Add(vX); uv.Add(Vector2.zero);
                verts.Add(vY); uv.Add(Vector2.one * Vector3.Distance(aY, vY));
                verts.Add(v3); uv.Add(Vector2.one * Vector3.Distance(a3, v3));
            }
        }

    }

    public static Vector3 GetClosestPointOnLine(Vector3 A, Vector3 B, Vector3 P) {
        Vector3 AP = P - A;       //Vector from A to P   
        Vector3 AB = B - A;       //Vector from A to B  

        float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
        float ABAPproduct = Vector3.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
        float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

        return A + AB * distance;
    }

    public static edge[] processForEdges(Mesh mesh) {

        List<edge> edges = new List<edge>();
        List<edge> edgesWithAtLeast2SharedTriangles = new List<edge>();

        Vector3[] verts = mesh.vertices;
        Vector3[] norms = mesh.normals;

        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i+= 3) {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];

            tryReduce(ref a, verts, norms);
            tryReduce(ref b, verts, norms);
            tryReduce(ref c, verts, norms);

            trySwap(ref a, ref b);
            trySwap(ref a, ref c);
            trySwap(ref b, ref c);

            tryPair(new edge(a, b), edges, edgesWithAtLeast2SharedTriangles);
            tryPair(new edge(b, c), edges, edgesWithAtLeast2SharedTriangles);
            tryPair(new edge(a, c), edges, edgesWithAtLeast2SharedTriangles);
        }

        return edges.ToArray();
    }

    static void tryReduce(ref int index, Vector3[] verts, Vector3[] norms) {
        Vector3 iVert = verts[index];
        Vector3 iNorm = norms[index];
        for (int i = 0; i < index; i++) {
            float f = Mathf.Abs(iVert.x - verts[i].x);
            f += Mathf.Abs(iVert.y - verts[i].y);
            f += Mathf.Abs(iVert.z - verts[i].z);

            if (f <= vertexEquivalenceMargin) {

                float g = Mathf.Abs(iNorm.x - norms[i].x);
                g += Mathf.Abs(iNorm.y - norms[i].y);
                g += Mathf.Abs(iNorm.z - norms[i].z);

                if (g <= normalEquivalenceMargin) {
                    index = i;
                    return;
                }
            }
        }
    }

    static void trySwap(ref int d, ref int e) {
        if (d > e) {
            int f = e;
            e = d;
            d = f;
        }
    }

    static void tryPair(edge m, List<edge> intpairs, List<edge> intpairsWithAtLeast2SharedTriangles) {
        if (intpairsWithAtLeast2SharedTriangles.Contains(m))
            return;

        if (intpairs.Remove(m)) {
            intpairsWithAtLeast2SharedTriangles.Add(m);
            return;
        }

        intpairs.Add(m);
    }

    public struct edge {
        public int a;
        public int b;

        public edge(int _a, int _b) {
            a = _a;
            b = _b;
        }

        public override bool Equals(object obj) {
            if (!(obj is edge))
                return false;

            edge p = (edge)obj;

            return p.a == a && p.b == b;
        }

        public override int GetHashCode() {
            return a * 65536 + b;
        }
    }
}

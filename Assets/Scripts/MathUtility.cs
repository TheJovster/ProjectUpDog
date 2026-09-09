using UnityEngine;

public static class MathUtility
{
    public static float EaseIn(float f)
    {
        return f * f;
    }

    public static float EaseOut(float f)
    {
        return 1.0f - EaseIn(1.0f - f);
    }

    public static float SmoothStep(float f)
    {
        return f * f * (3.0f - 2.0f * f);
    }

    public static float EaseOutElastic(float f)
    {
        float p = 0.5f;
        return Mathf.Pow(2, -10 * f) * Mathf.Sin((f - p / 4) * (2 * Mathf.PI) / p) + 1;
    }

    public static float OutBounce(float f)
    {
        float div = 2.75f;
        float mult = 7.5625f;
        if (f < 1 / div)
        {
            return mult * f * f;
        }
        else if (f < 2 / div)
        {
            f -= 1.5f / div;
            return mult * f * f + 0.75f;
        }
        else if (f < 2.5 / div)
        {
            f -= 2.25f / div;
            return mult * f * f + 0.9375f;
        }
        else
        {
            f -= 2.625f / div;
            return mult * f * f + 0.984375f;
        }
    }

    public static float AngleBetween(Vector2 vA, Vector2 vB)
    {
        return (Mathf.Acos(Vector2.Dot(vA, vB) / (vA.magnitude * vB.magnitude))) * Mathf.Rad2Deg;
    }

    public static Vector3 GetClosestPointOnPlane(Plane plane, Vector3 v)
    {
        float fDistanceToPlane = Vector3.Dot(plane.normal, v) + plane.distance;
        return v - plane.normal * fDistanceToPlane;
    }

    public static bool GetSide(Plane plane, Vector3 v)
    {
        return Vector3.Dot(plane.normal, v) + plane.distance > 0.0f;
    }

    public static Vector3 GetClosestPointOnSegment(Vector3 vA, Vector3 vB, Vector3 vP)
    {
        // A
        Vector3 vAB = vB - vA;
        Vector3 vAP = vP - vA;
        if (Vector3.Dot(vAB, vAP) <= 0.0f)
        {
            return vA;
        }

        // B
        Vector3 vBA = vA - vB;
        Vector3 vBP = vP - vB;
        if (Vector3.Dot(vBA, vBP) <= 0.0f)
        {
            return vB;
        }

        // C
        return vA + vAB * (Vector3.Dot(vAB, vAP)) / vAB.sqrMagnitude;
    }

    public static bool PointInTriangle(Vector3 p, Vector3 p0, Vector3 p1, Vector3 p2, out Vector3 vBaryCoords)
    {
        // Compute vectors        
        Vector3 v0 = p2 - p0;
        Vector3 v1 = p1 - p0;
        Vector3 v2 = p - p0;

        // Compute dot products
        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
        float w = 1.0f - (u + v);
        vBaryCoords = new Vector3(w, v, u);

        // Check if point is in triangle
        return u >= 0.0f &&
               v >= 0.0f &&
               u + v <= 1.0f;
    }

    public static bool RayTriangleIntersection(Ray ray, Vector3 vA, Vector3 vB, Vector3 vC, out Vector3 vHit, out Vector3 vBaryCoords)
    {
        // raycast against triangle plane
        Plane plane = new Plane(vA, vB, vC);
        float fDistanceToPlane;
        if (!plane.Raycast(ray, out fDistanceToPlane))
        {
            vHit = Vector3.zero;
            vBaryCoords = Vector3.zero;
            return false;
        }

        // calculate the ray-point on the plane
        vHit = ray.origin + ray.direction * fDistanceToPlane;

        // did we intersect?
        return PointInTriangle(vHit, vA, vB, vC, out vBaryCoords);
    }
}

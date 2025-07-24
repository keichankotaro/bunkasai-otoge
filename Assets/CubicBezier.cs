using System;

public class CubicBezier
{
    public static float Evaluate(float T, float x1, float y1, float x2, float y2, float epsilon = 1e-6f)
    {
        float Bezier(float t, float p1, float p2)
        {
            float oneMinusT = 1f - t;
            return (float)(3 * Math.Pow(oneMinusT, 2) * t * p1 + 3 * oneMinusT * Math.Pow(t, 2) * p2 + Math.Pow(t, 3));
        }

        float BezierX(float t) => Bezier(t, x1, x2);
        float BezierY(float t) => Bezier(t, y1, y2);

        float t0 = 0f, t1 = 1f;
        while (t1 - t0 > epsilon)
        {
            float tm = (t0 + t1) / 2;
            float x = BezierX(tm);
            if (x < T)
                t0 = tm;
            else
                t1 = tm;
        }

        float tFinal = (t0 + t1) / 2;
        return BezierY(tFinal);
    }
}
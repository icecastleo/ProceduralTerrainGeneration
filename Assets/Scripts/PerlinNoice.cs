using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise {

    static int size = 256;

    int[] permutation = new int[size];
    int[] p = new int[size * 2];

    public PerlinNoise()
    {
        makePermutationTable();  
    }

    void makePermutationTable()
    {
        for (int i = 0; i < size; i++)
        {
            permutation[i] = i;
        }

        for (int i = 0; i < size; i++)
        {
            int j = Random.Range(0, size);

            // swap
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }

        System.Array.Copy(permutation, 0, p, 0, size);
        System.Array.Copy(permutation, 0, p, size, size);
    }

    public float noise(float x, float y)
    {
        int xi = (int)x & (size - 1);
        int yi = (int)y & (size - 1);
        int g1 = p[p[xi] + yi];
        int g2 = p[p[xi + 1] + yi];
        int g3 = p[p[xi] + yi + 1];
        int g4 = p[p[xi + 1] + yi + 1];

        float xf = x - (int)x;
        float yf = y - (int)y;

        float d1 = grad(g1, xf, yf);
        float d2 = grad(g2, xf - 1, yf);
        float d3 = grad(g3, xf, yf - 1);
        float d4 = grad(g4, xf - 1, yf - 1);

        float u = fade(xf);
        float v = fade(yf);

        float x1Inter = Mathf.Lerp(d1, d2, u);
        float x2Inter = Mathf.Lerp(d3, d4, u);
        return Mathf.Lerp(x1Inter, x2Inter, v);
    }

    private float fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float grad(int hash, float x, float y)
    {
        switch (hash & 3)
        {
            case 0: return x + y;
            case 1: return -x + y;
            case 2: return x - y;
            case 3: return -x - y;
            default: return 0;
        }
    }
}

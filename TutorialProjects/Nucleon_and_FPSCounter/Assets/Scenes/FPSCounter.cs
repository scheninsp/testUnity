using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public int frameRange = 60;

    public int FPS { get; private set; }

    public int AverageFPS { get; private set; }
    int[] fpsBuffer;
    int fpsBufferIndex;


    void InitializeBuffer()
    {
        if (frameRange <= 0) { frameRange = 1; }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    private void Awake()
    {
        InitializeBuffer();
    }

    void Update()
    {
        FPS = (int)(1f / Time.unscaledDeltaTime);
        if(fpsBuffer == null || fpsBuffer.Length != frameRange)
        {
            InitializeBuffer();
        }
        if (fpsBufferIndex < frameRange ) {
            fpsBuffer[fpsBufferIndex++] = FPS;
        }
        else
        {
            fpsBufferIndex = 0;
        }
        int sum = 0;
        for(int i=0; i<frameRange; i++)
        {
            sum += fpsBuffer[i];
        }
        AverageFPS = sum / frameRange;

    }
}

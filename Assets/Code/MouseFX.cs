using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFX : MonoBehaviour
{
    [SerializeField] int    clickBurst;
    [SerializeField] int    releaseBurst;

    ParticleSystem          partSys;

    // Start is called before the first frame update
    void Start()
    {
        partSys = GetComponent<ParticleSystem>();
        partSys.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        ParticleSystem.ShapeModule shape = partSys.shape;
        Vector3 mousePos = Input.mousePosition;
        Vector3 localPos = Camera.main.ScreenToWorldPoint(mousePos);
        localPos.x /= transform.localScale.x;
        localPos.y /= transform.localScale.y;
        localPos.z = shape.position.z;
        shape.position = localPos;
    }

    public void Trigger()
    {
        if (partSys.isStopped)
        {
            partSys.Play(true);
            DoBurst(clickBurst);
        }
    }

    public void Release()
    {
        if (partSys.isPlaying)
        {
            DoBurst(releaseBurst);
            partSys.Stop();
        }
    }

    void DoBurst(int burstCount)
    {
        ParticleSystem.MainModule main = partSys.main;
        float origSpeed = main.startSpeedMultiplier;
        main.startSpeedMultiplier = 3.0f * origSpeed;
        partSys.Emit(burstCount);
        main.startSpeedMultiplier = origSpeed;
    }
}

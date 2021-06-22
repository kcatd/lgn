using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FXController : SimpleSingleton<FXController>
{
    [Header("Prototypes")]
    [SerializeField] ParticleSystem             onDrag;
    [SerializeField] ParticleSystem             dragStart;
    [SerializeField] ParticleSystem[]           dragSuccess;
    [SerializeField] ParticleSystem             dragFailure;

    [SerializeField] float                      dragEmission = 100;

    [SerializeField] Camera                     worldCam;
    RectTransform                               rectTransform;

    List<ParticleSystem>            systems = new List<ParticleSystem>();
    ParticleSystem                  dragMouse;
    ParticleSystem[]                allMouseSystems;


    // Start is called before the first frame update
    void Start()
    {
        dragMouse = Instantiate<ParticleSystem>(onDrag, transform);
        allMouseSystems = dragMouse.GetComponentsInChildren<ParticleSystem>(true);
        if (worldCam == null)  worldCam = Camera.main;

        rectTransform = GetComponent<RectTransform>();
        EnableMouseDrag(false);
    }

    void EnableMouseDrag(bool b) 
    {
        foreach (var t in allMouseSystems)
        {
            ParticleSystem.EmissionModule emission = t.emission;
            emission.rateOverDistance = (b) ? dragEmission : 0;
        }
	}

    public void EndGame()
    {
        EnableMouseDrag(false);
	}

    // Update is called once per frame
    void Update()
    {
        foreach (var t in systems)    
        {
            if (!t.IsAlive())
            {
                systems.Remove(t);
                Destroy(t.gameObject);
                break;
            }
        }
        dragMouse.transform.localPosition = MousePos();
    }

    public Vector3 MousePos()
    {
        return worldCam.ScreenToWorldPoint(Input.mousePosition);
	}

    public void StartDragging(Vector3 pos)
    {
        EnableMouseDrag(true);
        ParticleSystem start = Instantiate<ParticleSystem>(dragStart, transform);
        start.transform.localPosition = pos;
        systems.Add(start);
    }

    public void StopDragging(Vector3 pos, int successRank)
    {
        EnableMouseDrag(false);
        if (successRank >= 0)
        {
            if (successRank >= dragSuccess.Length) successRank = dragSuccess.Length-1;
            ParticleSystem end = Instantiate<ParticleSystem>(dragSuccess[successRank], transform);
            end.transform.localPosition = pos;
            systems.Add(end);
        } else
        if (dragFailure)
        {
            ParticleSystem end = Instantiate<ParticleSystem>(dragFailure, transform);
            end.transform.localPosition = pos;
            systems.Add(end);

		}
	}

}

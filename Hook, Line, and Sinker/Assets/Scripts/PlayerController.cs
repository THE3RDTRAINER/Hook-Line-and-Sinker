﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
//[RequireComponent(typeof(HingeJoint))]
public class PlayerController : MonoBehaviour
{
    private LineRenderer lineRen;
    [SerializeField] private GameObject connectedObject;
    [SerializeField] private LayerMask LayersToCollideWith;
    [SerializeField] private Transform Camera;
    [SerializeField] private SpringJoint sj;
    [SerializeField] private GameObject tmpComponent;

    [Header("Controls")]
    [SerializeField] private KeyCode ShootRope;
    [Range(0, 1)]
    [SerializeField] private float amountToAddToRope = .5f;
    private List<GameObject> grabbableObjects = new List<GameObject>();
    //[SerializeField] private KeyCode LongenRope;
    //[SerializeField] private KeyCode ShortenRope;
    // Start is called before the first frame update

    [Header("Hookshot Options")]
    [SerializeField] private float hookshotDistance, hookSpeed;
    [SerializeField] private LayerMask InitialHit, HookableObjects;

    //Hookshot Variables
    private bool isHooked;
    private Vector3 oldPosition;
    [SerializeField] private Transform hookedObject;
    void Start()
    {
        lineRen = GetComponent<LineRenderer>();
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("GrabbableStatic"))
        {
            grabbableObjects.Add(i);
        }
        foreach (GameObject i in GameObject.FindGameObjectsWithTag("GrabbableDynamic"))
        {
            grabbableObjects.Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonUp("Fire1") || Input.GetKeyUp(ShootRope))
        {
            Debug.Log("FIRE");
            if (connectedObject == null)
            {
                Grapple(Camera.position, Camera.forward, 20f, LayersToCollideWith);
            }
            else
            {
                UnGrapple();
            }

        }
        lineRen.SetPosition(0, Camera.position);
        if (connectedObject != null)
        {
            lineRen.SetPosition(1, connectedObject.transform.position);
        }

        //Controls
        if (Input.GetButton("ShortenRope"))
        {
            retractRope();
        } else if (Input.GetButton("LongenRope"))
        {
            ExtendRope();
        }

        //Hookshot controls
        if (Input.GetButtonUp("Fire2"))
        {
            Debug.Log("Hookshot fired!");
            Hookshot(Camera.position, Camera.forward);
        }
        if(hookedObject != null && isHooked)
        {
            if(Vector3.Distance(transform.position,hookedObject.position) < 1.0f)
            {
                hookedObject = null;
                isHooked = false;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, hookedObject.position, .05f);
            }
        }

    }
    private void FixedUpdate()
    {
        if (Physics.Raycast(Camera.position, Camera.forward, 20f, LayersToCollideWith))
        {
            tmpComponent.SetActive(true);
        }
        else
        {
            tmpComponent.SetActive(false);
        }
    }

    void Grapple(Vector3 pos, Vector3 dir, float maxDistance, LayerMask layersToHit)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, maxDistance, layersToHit, QueryTriggerInteraction.Collide))
        {
            lineRen.enabled = true;
            connectedObject = hit.collider.gameObject;
            if (connectedObject.GetComponent<Rigidbody>())
            {
                AddJoint(connectedObject.GetComponent<Rigidbody>());
            }
        }
    }

    void UnGrapple()
    {
        connectedObject = null;
        Destroy(sj);
        sj = null;
        lineRen.enabled = false;
    }

    void ExtendRope()
    {
        sj.maxDistance += amountToAddToRope;
    }
    void retractRope()
    {
        sj.maxDistance -= amountToAddToRope;
    }

    void AddJoint(Rigidbody rb)
    {
        sj = gameObject.AddComponent(typeof(SpringJoint)) as SpringJoint;
        sj.maxDistance = Vector3.Distance(connectedObject.transform.position, Camera.transform.position);
        sj.connectedBody = rb;
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedAnchor = Vector3.zero;
        //sj.anchor = Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.position, Camera.forward);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (GameObject i in grabbableObjects)
        {
            Gizmos.DrawLine(Camera.position, i.transform.position);
        }
    }
    private void Hookshot(Vector3 pos, Vector3 dir)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos, dir, out hit, hookshotDistance, InitialHit, QueryTriggerInteraction.Collide)){
            //Start the hookshot
            if (hit.collider.gameObject.layer == 8)
            {
                isHooked = true;
                hookedObject = hit.collider.transform;
                oldPosition = transform.position;
            }
            //Plays the denied SFX
            else
            {
                Debug.Log("Hookshot didn't hit anything!");
            }
        }
    }
}
using System;
using UnityEngine;

public class PaintGun : MonoBehaviour
{
    private MagicLeapInputs mlInputs;
    private MagicLeapInputs.ControllerActions controllerActions;

    [SerializeField] private Transform gunBarrelOpening;
    [SerializeField] private Transform paintBlobParent;
    [SerializeField] private GameObject paintBlobPrefab;
    [SerializeField] private LayerMask layerMask;

    void Start()
    {
        if (this.gunBarrelOpening == null || this.paintBlobPrefab == null)
        {
            throw new NullReferenceException("PaintGun.cs: References not properly setup.");
        }

        this.mlInputs = new MagicLeapInputs();
        this.mlInputs.Enable();
        this.controllerActions = new MagicLeapInputs.ControllerActions(this.mlInputs);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.controllerActions.Trigger.triggered)
        {   

            if (Physics.Raycast(this.gunBarrelOpening.position, this.gunBarrelOpening.forward, out var hit, 10, this.layerMask.value))
            {
                var paintBlob = Instantiate(this.paintBlobPrefab, hit.point - this.gunBarrelOpening.forward * 0.003f, Quaternion.LookRotation(hit.normal), this.paintBlobParent);
                Debug.Log("Shots fired, hit something."); 
            }
            else
            {
                Debug.Log("Shots fired, you missed.");
            }
        }
    }
}

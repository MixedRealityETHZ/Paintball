using System;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

public class Meshing : MonoBehaviour
{
    [SerializeField] private MeshingSubsystemComponent meshingSubsystemComponent;
    private readonly MLPermissions.Callbacks mlPermissionsCallbacks = new();
    
    private void Awake()
    {
        if (this.meshingSubsystemComponent == null)
        {
            // get meshing subsystem
            this.meshingSubsystemComponent = FindObjectOfType<MeshingSubsystemComponent>(true);

            if (this.meshingSubsystemComponent == null)
            {
                throw new NullReferenceException("No MeshingSubsystemComponent was found in the open scene.");
            }
        }

        // subscribe to permission events
        this.mlPermissionsCallbacks.OnPermissionGranted += this.MlPermissionsCallbacks_OnPermissionGranted;
        this.mlPermissionsCallbacks.OnPermissionDenied += this.MlPermissionsCallbacks_OnPermissionDenied;
        this.mlPermissionsCallbacks.OnPermissionDeniedAndDontAskAgain += this.MlPermissionsCallbacks_OnPermissionDenied;
    }

    void Start()
    {
        // request permission at start
        MLPermissions.RequestPermission(MLPermission.SpatialMapping, this.mlPermissionsCallbacks);
    }

    // if permission denied, disable meshing subsystem
    private void MlPermissionsCallbacks_OnPermissionDenied(string permission)
    {
        this.meshingSubsystemComponent.enabled = false;
    }

    // if permission granted, enable meshing subsystem
    private void MlPermissionsCallbacks_OnPermissionGranted(string permission)
    {
        this.meshingSubsystemComponent.enabled = true;
    }

    private void OnDestroy()
    {
        // unsubscribe from permission events
        this.mlPermissionsCallbacks.OnPermissionGranted -= this.MlPermissionsCallbacks_OnPermissionGranted;
        this.mlPermissionsCallbacks.OnPermissionDenied -= this.MlPermissionsCallbacks_OnPermissionDenied;
        this.mlPermissionsCallbacks.OnPermissionDeniedAndDontAskAgain -= this.MlPermissionsCallbacks_OnPermissionDenied;
    }

}

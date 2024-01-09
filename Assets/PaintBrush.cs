using System;
using UnityEngine;

public class PaintBrush : MonoBehaviour
{
    public GameObject ColorIndicator;
    public ColorPickerTriangle ColorMenu;
    public float paintBlobRadius = 0.05f;
    public float paintBlobSpawnDistance = 0.02f;

    private LiquidPaintDropSystem liquidPaintDropSystem;
    private Vector3 lastPaintPosition;
    private BoxCollider boxCollider;

    void OnEnable()
    {
        this.liquidPaintDropSystem = LiquidPaintDropSystem.GetInstance();
        if (this.liquidPaintDropSystem == null)
        {
            this.enabled = false;
            throw new InvalidOperationException("There is no LiquidPaintDropSystem present in the scene.");
        }

        this.boxCollider = this.GetComponentInChildren<BoxCollider>();
        if (this.boxCollider == null)
        {
            this.enabled = false;
            throw new InvalidOperationException("There is no BoxCollider present in the paint brush hierarchy.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.ColorIndicator.GetComponent<MeshRenderer>().material.color != this.ColorMenu.TheColor)
        {
            this.ColorIndicator.GetComponent<MeshRenderer>().material.color = this.ColorMenu.TheColor;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        try
        {
            if (collision.contactCount == 0 || !PaintUtility.IsOnSurfaceLayer(collision.gameObject))
            {
                return;
            }

            var myPosition = this.transform.position;
            var sqDistToLastPaintPosition = (myPosition - this.lastPaintPosition).sqrMagnitude;

            if (sqDistToLastPaintPosition < paintBlobSpawnDistance * paintBlobSpawnDistance)
            {
                return;
            }

            this.lastPaintPosition = myPosition;

            if (collision.contactCount == 1)
            {
                var contact = collision.GetContact(0);
                this.AddPaint(contact.point, contact.normal);
            }

            var averageContactPoint = Vector3.zero;
            var averageContactNormal = Vector3.zero;

            for (var i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                averageContactPoint += contact.point;
                averageContactNormal += contact.normal;
            }

            var invContactCount = 1f / collision.contactCount;
            averageContactPoint *= invContactCount;
            averageContactNormal.Normalize();

            this.AddPaint(in averageContactPoint, in averageContactNormal);
        }
        catch (Exception e)
        {
            ErrorMessageUtility.ShowError(e.Message);
        }
    }

    private void AddPaint(in Vector3 position, in Vector3 normal)
    {
        PaintUtility.AddPaint(in position, in normal, in ColorMenu.TheColor, paintBlobRadius);
    }
}

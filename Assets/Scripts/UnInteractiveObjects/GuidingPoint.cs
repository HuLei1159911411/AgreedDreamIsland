using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GuidingPoint : MonoBehaviour
{
    public GuidingPointsGroup guidingPointsGroup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            guidingPointsGroup.ChangeToNextPoint();
            transform.gameObject.SetActive(false);
        }
    }
}

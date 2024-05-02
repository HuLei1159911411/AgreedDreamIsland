using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractController : MonoBehaviour
{
    public Collider interactCollider;
    public List<string> interactiveObjectsTags;
    [FormerlySerializedAs("queueInteractiveObjects")] public List<GameObject> listInteractiveObjects;

    private int _count;
    private GameObject _nowInteractiveObject;
    
    void Update()
    {
        if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]) && listInteractiveObjects.Count > 0)
        {
            // ^1 = listInteractiveObjects.Count - 1 是结尾表达式
            _nowInteractiveObject = listInteractiveObjects[^1];
            switch (_nowInteractiveObject.tag)
            {
                case "Equipment" :
                    if (_nowInteractiveObject.GetComponent<Equipment>().PickUpItem())
                    {
                        Debug.Log("捡起来");
                        listInteractiveObjects.RemoveAt(listInteractiveObjects.Count - 1);
                    }
                    else
                    {
                        Debug.Log("装备不了");
                    }
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        for (_count = 0; _count < interactiveObjectsTags.Count; _count++)
        {
            if (other.CompareTag(interactiveObjectsTags[_count]))
            {
                listInteractiveObjects.Add(other.gameObject);
                return;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        listInteractiveObjects.Remove(other.gameObject);
    }
}

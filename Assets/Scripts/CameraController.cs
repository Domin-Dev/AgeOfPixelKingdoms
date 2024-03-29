using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class CameraController : MonoBehaviour
{

    [SerializeField] SpriteRenderer target;

    public Vector3 Limit;


    bool isFrozen;
    private Transform provinceTarget;
    private Action end;

    Vector3 startPosition;
    Vector3 endPosition;

    SelectingProvinces selectingProvinces;


    void Start()
    {
        isFrozen = false;
        Application.targetFrameRate = 60;
        selectingProvinces = GetComponent<SelectingProvinces>();
        numbertouch = 0;

        if (target != null)
        {
            float screenRatio = (float)Screen.width / (float)Screen.height;
            float targetRatio = target.bounds.size.x / target.bounds.size.y;

            if (screenRatio >= targetRatio)
            {
                Camera.main.orthographicSize = target.bounds.size.y / 2;
            }
            else
            {
                float differenceInSize = targetRatio / screenRatio;
                Camera.main.orthographicSize = target.bounds.size.y / 2 * differenceInSize;
            }
        }
        lastValue = 0f;
    }


    float lastValue;
    float numbertouch;
    private void Update()
    {
        if (target == null && !isFrozen)
        {
           if(Input.mouseScrollDelta.y != 0) Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * 0.5f, 0.5f, 3);

            if (Input.GetMouseButtonDown(0) && !MouseIsOverUI())
            {
                startPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (selectingProvinces != null) selectingProvinces.SelectingProvince();
            }


            if (Input.GetMouseButton(0) && !MouseIsOverUI() && Input.touchCount <= 1 && numbertouch < 2)
            {
                endPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 position = transform.position + startPosition - endPosition;
                transform.position = new Vector3(Mathf.Clamp(position.x, -0.5f, Limit.x), Mathf.Clamp(position.y, -0.5f, Limit.y), -10);
            }


            if (Input.touchCount >= 2)
            {
                float Value = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (lastValue == 0)
                {
                    lastValue = Value;
                }
                else
                {
                    Debug.Log(Value + "  " + lastValue);
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + (lastValue - Value) * 0.004f, 0.5f, 3);
                    lastValue = Value;
                }
            }
            else if (lastValue != 0)
            {
                lastValue = 0;
            }

            numbertouch = Input.touchCount;
        }

        if(isFrozen)
        {
            Vector3 vector3 = Vector3.Lerp(transform.position, provinceTarget.position, 1f * Time.deltaTime);          
            transform.position = new Vector3(vector3.x, vector3.y,-10);
            if (Vector2.Distance(vector3, provinceTarget.position) < 0.3f)
            {
                StartCoroutine(EndMove());
            }
        }

    }

    private bool MouseIsOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    public void SetProvince(Transform province,Action action)
    {
        isFrozen = true;
        provinceTarget = province;
        end = action;
    }

    IEnumerator EndMove()
    {
       isFrozen = false;
       yield return new WaitForSeconds(1f);
       end();
    }
}

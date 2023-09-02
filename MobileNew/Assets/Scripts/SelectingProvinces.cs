using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static UnityEngine.UI.CanvasScaler;
using static UnityEditor.Progress;

public class SelectingProvinces : MonoBehaviour
{
    public Transform selectedProvince;
    public Transform selectedNeighbor;
    private bool moveMode = false;


    private Color selectedColor;

    private int selectedUnitIndex = -1;
    private int selectedProvinceNumber = 0;


    private int unitsNumber = 0;
    private int maxUnitsNumber = 100;


    private TextMeshProUGUI nameWindow;
    private Slider slider;
    private TextMeshProUGUI numberText;
    private Transform buttons;
    private TextMeshProUGUI price;
    private TextMeshProUGUI buttonText;
    private Button buttonRecruit;


    private void Start()
    {
        Transform selectionNumberUnits = UIManager.Instance.GetSelectionNumberUnitsWindowWindow();
        nameWindow = selectionNumberUnits.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
        buttons = selectionNumberUnits.GetChild(0);
        numberText = selectionNumberUnits.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        slider = selectionNumberUnits.GetChild(1).GetComponent<Slider>();
        price = selectionNumberUnits.GetChild(3).GetComponent<TextMeshProUGUI>();
        buttonRecruit = selectionNumberUnits.GetChild(4).GetComponent<Button>();
        buttonText = buttonRecruit.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        buttonRecruit.onClick.AddListener(() => { Recruit(); });
        slider.maxValue = maxUnitsNumber;
        slider.onValueChanged.AddListener((float value) => { SetUnitsNumber((int)(value)); });
        buttons.GetChild(0).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(-20); });
        buttons.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(-5); });
        buttons.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(-1); });

        buttons.GetChild(3).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(1); });
        buttons.GetChild(4).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(5); });
        buttons.GetChild(5).GetComponent<Button>().onClick.AddListener(() => { AddToUnitsNumber(20); });
        UpdateRecruitUI();
    }

    private void SetSelectionNumberUnits(bool isMove)
    {
        if(isMove)
        {
            buttonRecruit.onClick.RemoveAllListeners();
            buttonRecruit.onClick.AddListener(() => { Move(); });
            nameWindow.text = "movement of units";
            buttonText.text = "Move";
        }
        else
        {
            buttonRecruit.onClick.RemoveAllListeners();
            buttonRecruit.onClick.AddListener(() => { Recruit(); });
            nameWindow.text = "recruitment";
            buttonText.text = "Recruit";
        }
    }
    public void SelectingProvince()
    {
        Vector3 worldClickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        foreach (RaycastHit2D item in Physics2D.RaycastAll(worldClickPosition, Vector2.zero))
        {
            if (item.collider.tag == "Province")
            {
                SpriteRenderer spriteRenderer = item.collider.GetComponent<SpriteRenderer>();
                Texture2D spriteTexture = spriteRenderer.sprite.texture;
                Rect rect = spriteRenderer.sprite.rect;

                float x = (worldClickPosition.x - item.collider.transform.position.x) * spriteRenderer.sprite.pixelsPerUnit;
                float y = (worldClickPosition.y - item.collider.transform.position.y) * spriteRenderer.sprite.pixelsPerUnit;

                x += rect.width / 2;
                y += rect.height / 2;

                x += rect.x;
                y += rect.y;

                Color pixel = spriteTexture.GetPixel(Mathf.FloorToInt(x), Mathf.FloorToInt(y));

                if (pixel.a == 0) continue;
                else
                {
                    

                    if (selectedProvince != null)
                    {
                        if (selectedProvince.name == item.collider.gameObject.name)
                        {
                            if(moveMode)
                            {
                                ResetNeighbors();
                            }
                            else
                            {
                                HighlightNeighbors();
                            }
                            
                        }
                        else
                        {
                            if(moveMode && IsNeighbor(int.Parse(item.collider.gameObject.name)))
                            {
                                selectedNeighbor = item.collider.gameObject.transform;
                                UIManager.Instance.LoadUnitsMove(int.Parse(selectedProvince.name), int.Parse(item.collider.gameObject.name),false);
                            }
                            else
                            {
                                UIManager.Instance.CloseUIWindow("ProvinceStats");
                                ResetNeighbors();
                                selectedProvince = item.collider.gameObject.transform;
                            }
                        }
                    }else
                    {
                        selectedProvince = item.collider.gameObject.transform;
                    }



                    
                    spriteRenderer.sortingOrder = -1;
                    ChangeProvinceBorderColor(spriteRenderer, Color.white);
                    UIManager.Instance.OpenUIWindow("ProvinceStats", int.Parse(item.collider.name));
                    break;
                }
            }
        }

      
    }
    public void ClearSelectedProvince()
    {
        if (selectedProvince != null)
        {
            SpriteRenderer spriteRenderer = selectedProvince.GetComponent<SpriteRenderer>();
            ChangeProvinceBorderColor(spriteRenderer, Color.black);
            if (GameManager.Instance.provinces[int.Parse(selectedProvince.name)].isSea) spriteRenderer.sortingOrder = -11;
            else spriteRenderer.sortingOrder = -10;
            selectedProvince = null;
        }
    }
    public void Build(int index)
    {
        if (selectedProvince != null)
        {
            BuildingStats buildingStats = GameAssets.Instance.buildingsStats[index];
            ProvinceStats provinceStats = GameManager.Instance.provinces[int.Parse(selectedProvince.name)];
            if (provinceStats.buildingIndex == -1)
            {
                Transform transform = new GameObject(selectedProvince.name, typeof(SpriteRenderer)).transform;
                transform.position = selectedProvince.position + new Vector3(0, 0.08f, 0);
                transform.GetComponent<SpriteRenderer>().sprite = buildingStats.icon;
                transform.GetComponent<SpriteRenderer>().sortingOrder = 0;
                provinceStats.buildingIndex = index;
            }
        }
    }


    public void HighlightNeighbors()
    {
        if (selectedProvince != null)
        { 
            List<int> list = GameManager.Instance.provinces[int.Parse(selectedProvince.name)].neighbors;

            for (int i = 0; i < list.Count; i++)
            {
               SpriteRenderer spriteRenderer =  GameAssets.Instance.map.GetChild(list[i]).GetComponent<SpriteRenderer>();
               spriteRenderer.sortingOrder = -2;
               ChangeProvinceBorderColor(spriteRenderer, Color.yellow);
            }
            moveMode = true;
        }
    }
    public void ResetNeighbors()
    {
        if (selectedProvince != null)
        {
            List<int> list = GameManager.Instance.provinces[int.Parse(selectedProvince.name)].neighbors;

            for (int i = 0; i < list.Count; i++)
            {
                SpriteRenderer spriteRenderer = GameAssets.Instance.map.GetChild(list[i]).GetComponent<SpriteRenderer>();
                ChangeProvinceBorderColor(spriteRenderer, Color.black);
                spriteRenderer.sortingOrder = -10;      
            }
            moveMode = false;
        }
    }
    public bool IsNeighbor(int index)
    {
        if (selectedProvince != null)
        {
            List<int> list = GameManager.Instance.provinces[int.Parse(selectedProvince.name)].neighbors;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == index)
                {
                    return true;
                }
            }
            return false;
        }
        return false;
    }


    private void ChangeProvinceColor(SpriteRenderer spriteRenderer,Color provinceColor)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        if (spriteRenderer.HasPropertyBlock())
        {
            spriteRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("Color2", provinceColor);
        }
        else
        {         
            materialPropertyBlock.SetColor("Color1", Color.black);
            materialPropertyBlock.SetFloat("Float", -0.05f);
            materialPropertyBlock.SetColor("Color2", provinceColor);
            materialPropertyBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);
        }
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    private void ChangeProvinceBorderColor(SpriteRenderer spriteRenderer, Color borderColor)
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        if (spriteRenderer.HasPropertyBlock())
        {
            spriteRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("Color1", borderColor);
        }
        else
        {
            materialPropertyBlock.SetColor("Color1", borderColor);
            materialPropertyBlock.SetFloat("Float", -0.05f);
            materialPropertyBlock.SetTexture("_MainTex", spriteRenderer.sprite.texture);
        }
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
    public void SetUnitsNumber(int unit)
    {
        unitsNumber = Math.Clamp(unit, 0, maxUnitsNumber); 
        UpdateRecruitUI();
    }
    public void AddToUnitsNumber(int number)
    {
        unitsNumber = Math.Clamp(unitsNumber + number, 0, maxUnitsNumber);
        UpdateRecruitUI();
    }
    private void UpdateRecruitUI()
    {
        numberText.text = unitsNumber.ToString() + "/" + maxUnitsNumber.ToString();
        slider.value = unitsNumber;
        price.text = "Price:  <color=#FF0000>"+ unitsNumber * 10 +" <sprite index=21> </color>";
    }


    public void ResetUnits()
    {
        if (selectedUnitIndex >= 0)
        {
            if (selectedProvinceNumber == 0)
            {
                GameAssets.Instance.recruitUnitContentUI.GetChild(selectedUnitIndex).GetComponent<Image>().sprite = GameAssets.Instance.brownTexture;
                selectedUnitIndex = -1;
            }
            else
            {
                GetCounter(selectedUnitIndex, selectedProvinceNumber).GetComponent<Image>().sprite = GameAssets.Instance.blueTexture;
                selectedUnitIndex = -1;
                selectedProvinceNumber = 0;
            }
        }
    }

    private Transform GetUIContent(int provinceNumber)
    {
        switch (provinceNumber)
        {
            case 1: return GameAssets.Instance.moveUnitContentUI1;
            case 2: return GameAssets.Instance.moveUnitContentUI2;
        }
        return null;
    }

    private Transform GetCounter(int unitIndex, int provinceNumber)
    {
        Transform transform = GetUIContent(provinceNumber);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if(unitIndex.ToString() == child.name)
            {
                return child;
            }          
        }
        return null;
    }

    private ProvinceStats GetProvinceStats(int provinceNumber) 
    {
        if (provinceNumber > 0)
        {
            Transform transform;
            if (provinceNumber == 1) transform = selectedProvince;
            else transform = selectedNeighbor;

            return GameManager.Instance.provinces[int.Parse(transform.name)];
        }
        else
            return null;
    }
    public void SelectUnitToMove(int index, int provinceNumber)
    {
        if (selectedProvince != null)
        {

            Debug.Log(provinceNumber);
            if (selectedUnitIndex >= 0 && selectedProvinceNumber >= 0)
            {
                Transform selected = GetCounter(selectedUnitIndex, selectedProvinceNumber);
                if (selected != null) selected.GetComponent<Image>().sprite = GameAssets.Instance.blueTexture;
            }

            selectedUnitIndex = index;
            selectedProvinceNumber = provinceNumber;

            Transform transform = GetCounter(selectedUnitIndex, selectedProvinceNumber);
            if(transform != null)  transform.GetComponent<Image>().sprite = GameAssets.Instance.brownTexture;


            if (GetProvinceStats(provinceNumber).units.ContainsKey(index))
                maxUnitsNumber = GetProvinceStats(provinceNumber).units[index];
            slider.maxValue = maxUnitsNumber;
            unitsNumber = 0;
            UpdateRecruitUI();
            ProvinceStats provinceStats = GameManager.Instance.provinces[int.Parse(selectedProvince.name)];
            selectedUnitIndex = index;

            SetSelectionNumberUnits(true);
            UIManager.Instance.OpenUIWindow("SelectionNumberUnits", 0);

        }
    }
    public void SelectUnitToRecruit(int index)
    {
        if (selectedProvince != null)
        {
            if(selectedUnitIndex >= 0) GameAssets.Instance.recruitUnitContentUI.GetChild(selectedUnitIndex).GetComponent<Image>().sprite = GameAssets.Instance.brownTexture;
            selectedUnitIndex = index;
            GameAssets.Instance.recruitUnitContentUI.GetChild(selectedUnitIndex).GetComponent<Image>().sprite = GameAssets.Instance.blueTexture;

            unitsNumber = 0;
            UpdateRecruitUI();
            ProvinceStats provinceStats = GameManager.Instance.provinces[int.Parse(selectedProvince.name)];
            selectedUnitIndex = index;

            SetSelectionNumberUnits(false);
            UIManager.Instance.OpenUIWindow("SelectionNumberUnits", 0);
        }
    }
    public void Recruit()
    {
        if (selectedProvince != null && unitsNumber > 0)
        {
            ProvinceStats provinceStats = GameManager.Instance.provinces[int.Parse(selectedProvince.name)];
            provinceStats.unitsCounter += unitsNumber;

            if (provinceStats.units == null) provinceStats.units = new Dictionary<int, int>();

            if (provinceStats.units.ContainsKey(selectedUnitIndex))
            {
                provinceStats.units[selectedUnitIndex] += unitsNumber;
            }
            else
            {
                provinceStats.units.Add(selectedUnitIndex, unitsNumber);
            }


            if (selectedProvince.childCount == 0)
            {
                Instantiate(GameAssets.Instance.unitCounter, selectedProvince.transform.position - new Vector3(0, 0.05f, 0), Quaternion.identity, selectedProvince);
            }
            selectedProvince.GetChild(0).GetComponentInChildren<TextMeshPro>().text = provinceStats.unitsCounter.ToString();
           
            UIManager.Instance.LoadProvinceUnitCounters(int.Parse(selectedProvince.name), GameAssets.Instance.unitCounterContentUI.transform,false);
            GameAssets.Instance.recruitUnitContentUI.GetChild(selectedUnitIndex).GetComponent<Image>().sprite = GameAssets.Instance.brownTexture;
            UIManager.Instance.CloseUIWindow("SelectionNumberUnits");
        }
    }

    public void Move()
    {
        if(selectedNeighbor != null && selectedProvince != null && unitsNumber > 0)
        {
            ProvinceStats provinceStats1 = GameManager.Instance.provinces[int.Parse(selectedProvince.name)];
            ProvinceStats provinceStats2 = GameManager.Instance.provinces[int.Parse(selectedNeighbor.name)];

            if(selectedProvinceNumber == 1)
            {
                MoveTo(provinceStats1, provinceStats2);
            }
            else if(selectedProvinceNumber == 2)
            {
                MoveTo(provinceStats2, provinceStats1);
            }

            UpdateUnitNumber(selectedProvince);
            UpdateUnitNumber(selectedNeighbor);
            UIManager.Instance.CloseUIWindow("SelectionNumberUnits");
            UIManager.Instance.LoadUnitsMove(int.Parse(selectedProvince.name), int.Parse(selectedNeighbor.name),true);
        }
    }

    private void MoveTo(ProvinceStats from, ProvinceStats to) 
    {
        from.unitsCounter -= unitsNumber;
        from.units[selectedUnitIndex] -= unitsNumber;
        to.unitsCounter += unitsNumber;

        if (to.units != null)
        {
            if (to.units.ContainsKey(selectedUnitIndex))
                to.units[selectedUnitIndex] += unitsNumber;
            else
                to.units.Add(selectedUnitIndex, unitsNumber);
        }
        else
        {
            to.units = new Dictionary<int, int>();
            to.units.Add(selectedUnitIndex, unitsNumber);
        }
    } 

    private void UpdateUnitNumber(Transform province)
    {
        int number = GameManager.Instance.provinces[int.Parse(province.name)].unitsCounter;

        if (province.childCount == 0)
        {
            Instantiate(GameAssets.Instance.unitCounter, province.transform.position - new Vector3(0, 0.05f, 0), Quaternion.identity, province);
        }else if(number == 0) 
        {
            Destroy(province.GetChild(0).gameObject);
            return;
        }
        province.GetChild(0).GetComponentInChildren<TextMeshPro>().text = number.ToString();
    }
}
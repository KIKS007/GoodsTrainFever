using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Sirenix.OdinInspector;
using DG.Tweening;

public enum ConstraintType
{
    LimitedPerTrain_Constraint,
    LimitedPerWagon_Constraint,
    NotNextTo_Constraint,
    NotOnTrainCenter_Constraint,
    NotOnTrainExtremities_Constraint

}

public class MenuContainerInfos : MenuComponent
{
    [Header("Elements")]
    public Text type;
    public Text size;

    [Header("Constraints")]
    public RectTransform constraintsParent;
    public GameObject constraintPrefab;
    public Vector2 constraintPosition;
    public float rowSpacing;

    [Header("Constraints Descriptions")]
    public List<ConstraintsDescriptions> descriptions = new List<ConstraintsDescriptions>();

    private Container _selectedContainer;

    public override void Setup()
    {
        base.Setup();

        Container.OnContainerSelected += (c) => _selectedContainer = c;
        Container.OnContainerErrorSelected += (c) => _selectedContainer = c;

        TrainsMovementManager.Instance.OnTrainDeparture += (arg) =>
        {
            if (arg.containers.Contains(_selectedContainer))
            {
                _selectedContainer = null;
            }
        };
    }

    public override void OnShow()
    {
        if (_selectedContainer == null)
            return;

        base.OnShow();

        StartCoroutine(OnShowCoroutine());
    }

    IEnumerator OnShowCoroutine()
    {
        foreach (Transform t in constraintsParent.transform)
            Destroy(t.gameObject);

        type.text = _selectedContainer.containerType.ToString();
        size.text = _selectedContainer.isDoubleSize ? "40 pieds" : "20 pieds";

        Vector2 position = constraintPosition;

        for (int i = 0; i < _selectedContainer.constraints.Count; i++)
        {
            Constraint constraintScript = _selectedContainer.constraints[i].constraint;

            RectTransform constraint = (Instantiate(constraintPrefab, Vector3.zero, Quaternion.identity, constraintsParent.transform).GetComponent<RectTransform>());
            constraint.localPosition = Vector3.zero;
            constraint.localRotation = Quaternion.Euler(Vector3.zero);
            constraint.anchoredPosition = position;

            //Change Description
            foreach (var d in descriptions)
            {
                if (d.constraintType.ToString() == constraintScript.GetType().ToString())
                {
                    constraint.GetComponent<Text>().text = d.title;
                    constraint.GetChild(0).GetComponent<Text>().text = d.description;

                    //Replace Dollar
                    if (d.replaceDollar)
                    {
                        if (constraintScript.GetType().GetField(d.propertyName) == null)
                        {
                            Debug.LogError("Wrong Field Name!", this);
                            break;
                        }

                        string value = constraintScript.GetType().GetField(d.propertyName).GetValue(constraintScript).ToString();

                        constraint.GetComponent<Text>().text = constraint.GetComponent<Text>().text.Replace("$", value);
                        constraint.GetChild(0).GetComponent<Text>().text = constraint.GetChild(0).GetComponent<Text>().text.Replace("$", value);
                    }

                    break;
                }
            }

            yield return new WaitForEndOfFrame();

            if (_selectedContainer.constraints[i].isRespected)
            {
                constraint.GetChild(0).GetChild(0).gameObject.SetActive(true);
                constraint.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                constraint.GetChild(0).GetChild(0).gameObject.SetActive(false);
                constraint.GetChild(0).GetChild(1).gameObject.SetActive(true);
            }

            position.y -= constraint.GetComponent<RectTransform>().sizeDelta.y + constraint.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            position.y -= constraint.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            position.y -= rowSpacing;
        }

        constraintsParent.sizeDelta = new Vector2(constraintsParent.sizeDelta.x, -position.y);
    }

    [System.Serializable]
    public class ConstraintsDescriptions
    {
        public ConstraintType constraintType;
        public string title;
        [Multiline]
        public string description;
        public bool replaceDollar = false;
        [ShowIfAttribute("replaceDollar")]
        public string propertyName;
    }
}

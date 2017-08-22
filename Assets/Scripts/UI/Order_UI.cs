using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Order_UI : MonoBehaviour
{
	public bool isPrepared = false;
	public bool isSent = false;
	public OrderUI parentOrderUI;
	public int OrderPos;
	private bool hasBeenPrepared = false;
	[Header ("Order")]
	public Order_Level orderLevel = new Order_Level ();

	[Header ("Containers")]
	public List<Container_UI> containers = new List<Container_UI> ();

	private GameObject SelectedContainer;

	public void Setup ()
	{
		CheckContainers ();
		foreach (var c in containers) {
			c.myOrderUI = this;
		}
	}

	public void OrderSent ()
	{
		foreach (var c in containers) {
			if (!c.isSent)
				c.ContainerSent ();
		}
	}

	void OnEnable ()
	{
		if (SelectedContainer != null) {
			ContainerSelected (SelectedContainer.GetComponent<Container_UI> ().myContainer);
			SelectedContainer = null;
		}
	}

	void CheckContainers ()
	{
		bool hasCheck = true;

		do {
			hasCheck = true;

			foreach (var c in OrdersManager.Instance.containersFromNoOrder) {
				if (ContainerAdded (c)) {
					OrdersManager.Instance.containersFromNoOrder.Remove (c);
					hasCheck = false;
					break;
				}
			}
		} while (!hasCheck);
	}

	public bool ContainerSent (Container container)
	{
		foreach (var c in containers) {
			if (c.isSent)
				continue;

			if (c.container != container)
				continue;

			c.ContainerSent ();

			UpdateStates ();

			return true;
		}

		UpdateStates ();

		return false;
	}

	public bool ContainerAdded (Container container)
	{
		foreach (var c in containers) {
			if (c.isSent)
				continue;

			if (c.neededCount == 0)
				continue;

			if (c.containerLevel.containerColor != container.containerColor)
				continue;

			if (c.containerLevel.containerType != container.containerType)
				continue;

			if (c.containerLevel.isDoubleSize != container.isDoubleSize)
				continue;

			c.ContainerAdded (container);

			UpdateStates ();

			return true;
		}

		return false;
	}

	public void ContainerDeselected ()
	{
		SelectedContainer = null;
		parentOrderUI.UnSelected ();
		foreach (var c in containers) {
			if (!c.isSent)
				c.GetComponent<Image> ().DOFade (1f, 0.2f);
		}
	}

	public bool ContainerSelected (Container container)
	{
		bool tmpBool = false;
		parentOrderUI.Selected ();
		foreach (var c in containers) {
			if (c.isSent) {
				c.ContainerSelected (false);
				continue;
			}

			if (c.neededCount == 0) {
				c.ContainerSelected (false);
				continue;
			}

			if (c.containerLevel.containerColor != container.containerColor) {
				c.ContainerSelected (false);
				continue;
			}

			if (c.containerLevel.containerType != container.containerType) {
				c.ContainerSelected (false);
				continue;
			}

			if (c.containerLevel.isDoubleSize != container.isDoubleSize) {
				c.ContainerSelected (false);
				continue;
			}

			SelectedContainer = c.gameObject;
			c.myContainer = container;
			c.ContainerSelected (true);

			tmpBool = true;
		}

		return tmpBool;
	}

	public bool ContainerRemoved (Container container)
	{
		List<Container_UI> containersTemp = new List<Container_UI> (containers);
		containersTemp.Reverse ();

		foreach (var c in containersTemp) {
			if (c.isSent)
				continue;

			if (c.container != container)
				continue;
			
			c.ContainerRemoved ();
			
			UpdateStates ();
			
			return true;
		}

		return false;
	}

	private void OrderPrepared (bool status)
	{
		if (status) {
			//ORDER PREPARED

			//May be usefull if we want to put an order back to initial place if it's "unprepared" after being prepared
			//OrderPos = parentOrderUI.GetChildPosition (this.gameObject);
			hasBeenPrepared = true;
			if (parentOrderUI.GetChildPosition (this.gameObject.GetComponent<RectTransform> ()) != parentOrderUI.GetChildCount () - 1 && !parentOrderUI.CheckAllOrdersPrepared ()) {
				DOVirtual.DelayedCall (0.2f, () => {
					this.GetComponent<CanvasGroup> ().DOFade (0, 1.5f).OnComplete (() => {
						parentOrderUI.SetOrderAtPosition (this.gameObject.GetComponent<RectTransform> (), parentOrderUI.GetChildCount () - 1);
						parentOrderUI.ShowOrders ();
						parentOrderUI.HideOrders ();
					});
				});
			}



		} else {
			//ORDER NOT PREPARED

			//Put order on top if it was prepared and not anymore
			if (hasBeenPrepared) {
				parentOrderUI.SetOrderAtPosition (this.gameObject.GetComponent<RectTransform> (), 0);
				parentOrderUI.ShowOrders ();
				parentOrderUI.HideOrders ();
			}
		}
	}

	void UpdateStates ()
	{
		bool prepared = true;

		foreach (var c in containers)
			if (!c.isPrepared) {
				prepared = false;
				break;
			}

		isPrepared = prepared;

		OrderPrepared (isPrepared);

		bool sent = true;

		foreach (var c in containers)
			if (!c.isSent) {
				sent = false;
				break;
			}

		isSent = sent;
	}
}

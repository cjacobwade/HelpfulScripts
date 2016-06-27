using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup)), ExecuteInEditMode]
public class GridLayoutDynamicSize : MonoBehaviour
{
	GridLayoutGroup _gridLayoutGroup = null;
	GridLayoutGroup gridLayoutGroup
	{
		get
		{
			if (!_gridLayoutGroup)
				_gridLayoutGroup = GetComponent<GridLayoutGroup>();

			return _gridLayoutGroup;
		}
	}

	RectTransform _rectTransform = null;
	RectTransform rectTransform
	{
		get
		{
			if (!_rectTransform)
				_rectTransform = GetComponent<RectTransform>();

			return _rectTransform;
		}
	}

	void LateUpdate()
	{
		UpdateCellSize();
	}

	protected void OnRectTransformDimensionsChange()
	{
		UpdateCellSize();
	}

	void UpdateCellSize()
	{
		int cellCount = gridLayoutGroup.GetComponentsInImmediateChildren<Transform>().Length;

		int columnCount = cellCount == 2 ? 2 : Mathf.CeilToInt(((float)cellCount) / 2f);
		int rowCount = cellCount > 2 ? 2 : 1;

		float xPadding = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
		float yPadding = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

		float xSplits = (float)(columnCount - 1);
		float ySplits = (float)(rowCount - 1);

		Vector2 cellSize = Vector2.zero;
		cellSize.x = rectTransform.rect.width/(float)columnCount - (xPadding + gridLayoutGroup.spacing.x * xSplits)/(float)columnCount;
		cellSize.y = rectTransform.rect.height/(float)rowCount - (yPadding + gridLayoutGroup.spacing.y * ySplits)/(float)rowCount;
		gridLayoutGroup.cellSize = cellSize;
	}
}

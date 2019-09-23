using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListContentItem : MonoBehaviour {
    public delegate void OnInitializeItem(GameObject go, int dataIndex);

    public OnInitializeItem onInitializeItem;

    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }

    /// <summary>
    /// 安排类型——垂直或水平
    /// </summary>
    [Tooltip("选择类型")]
    public Arrangement arrangement = Arrangement.Horizontal;

    /// <summary>
    /// 每行最多子级
    /// 如果这种安排是水平的，这就否定了哥伦布的数目
    /// 如果这种安排是垂直的，这个位置是供参考的
    /// </summary>
    [Range(1, 50)]
    [Tooltip("每行最多子级")]
    public int maxPerLine = 1;

    /// <summary>
    /// 每个单元格的高度
    /// </summary>
    [Tooltip("每个单元格的高度")]
    public float cellWidth = 200f;

    /// <summary>
    /// 每个单元格的宽度
    /// </summary>
    [Tooltip("每个单元格的宽度")]
    public float cellHeight = 200f;

    /// <summary>
    /// 每个单元格的宽度空间
    /// </summary>
    [Range(0, 50)]
    [Tooltip("每个单元格的宽度空间")]
    public float cellWidthSpace = 0f;

    /// <summary>
    /// 每个单元格的高度空间
    /// </summary>
    [Range(0, 50)]
    [Tooltip("每个单元格的高度空间")]
    public float cellHeightSpace = 0f;


    [Range(0, 30)]
    [Tooltip("生成item的数量")]//不包含被动生成的Item
    public int viewCount = 5;

    [Tooltip("scrollRect")]
    public ScrollRect scrollRect;

    [Tooltip("Content")]
    public RectTransform content;

    [Tooltip("预设物")]
    public GameObject goItemPrefab;

    private int dataCount;

    private int curScrollPerLineIndex = -1;

    private List<LoopListItme> listItem;

    private Queue<LoopListItme> unUseItem;


    void Awake()
    {
        listItem = new List<LoopListItme>();
        unUseItem = new Queue<LoopListItme>();
    }


    /// <summary>
    /// 初始化操作
    /// </summary>
    /// <param name="dataCount"></param>
    public void Init(int dataCount)
    {
        if (scrollRect == null || content == null || goItemPrefab == null)
        {
            Debug.LogError("异常:请检测<" + gameObject.name + ">对象上UIWarpContent对应ScrollRect、Content、GoItemPrefab 是否存在值...." + scrollRect + " _" + content + "_" + goItemPrefab);
            return;
        }
        if (dataCount <= 0)
        {
            return;
        }
        setDataCount(dataCount);

        scrollRect.onValueChanged.RemoveAllListeners();
        scrollRect.onValueChanged.AddListener(onValueChanged);//______绑定滑动事件

        unUseItem.Clear();
        listItem.Clear();

        setUpdateRectItem(0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
	private void setDataCount(int count)
    {
        if (dataCount == count)
        {
            return;
        }
        dataCount = count;
        setUpdateContentSize();
    }

    private void onValueChanged(Vector2 vt2)
    {
        switch (arrangement)
        {//判断是纵向还是横向
            case Arrangement.Vertical:
                float y = vt2.y;
                if (y >= 1.0f || y <= 0.0f)
                {
                    return;
                }
                break;
            case Arrangement.Horizontal:
                float x = vt2.x;
                if (x <= 0.0f || x >= 1.0f)
                {
                    return;
                }
                break;
        }
        int _curScrollPerLineIndex = getCurScrollPerLineIndex();
        if (_curScrollPerLineIndex == curScrollPerLineIndex)
        {
            return;
        }
        setUpdateRectItem(_curScrollPerLineIndex);//滑动更新显示内容
    }

    /**
	 * @des:设置更新区域内item
	 * 功能:
	 * 1.隐藏区域之外对象
	 * 2.更新区域内数据
	 */
    private void setUpdateRectItem(int scrollPerLineIndex)
    {
        if (scrollPerLineIndex < 0)
        {
            return;
        }
        curScrollPerLineIndex = scrollPerLineIndex;
        int startDataIndex = curScrollPerLineIndex * maxPerLine;
        int endDataIndex = (curScrollPerLineIndex + viewCount) * maxPerLine;
        //移除
        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            LoopListItme item = listItem[i];
            int index = item.Index;
            if (index < startDataIndex || index >= endDataIndex)
            {
                item.Index = -1;
                listItem.Remove(item);
                unUseItem.Enqueue(item);
            }
        }
        //显示
        for (int dataIndex = startDataIndex; dataIndex < endDataIndex; dataIndex++)
        {
            if (dataIndex >= dataCount)
            {
                continue;
            }
            if (isExistDataByDataIndex(dataIndex))
            {
                continue;
            }
            createItem(dataIndex);
        }
    }



    /**
	 * @des:添加当前数据索引数据
	 */
    public void AddItem(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex > dataCount)
        {
            return;
        }
        //检测是否需添加gameObject
        bool isNeedAdd = false;
        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            LoopListItme item = listItem[i];
            if (item.Index >= (dataCount - 1))
            {
                isNeedAdd = true;
                break;
            }
        }
        setDataCount(dataCount + 1);

        if (isNeedAdd)
        {
            for (int i = 0; i < listItem.Count; i++)
            {
                LoopListItme item = listItem[i];
                int oldIndex = item.Index;
                if (oldIndex >= dataIndex)
                {
                    item.Index = oldIndex + 1;
                }
                item = null;
            }
            setUpdateRectItem(getCurScrollPerLineIndex());
        }
        else
        {
            //重新刷新数据
            for (int i = 0; i < listItem.Count; i++)
            {
                LoopListItme item = listItem[i];
                int oldIndex = item.Index;
                if (oldIndex >= dataIndex)
                {
                    item.Index = oldIndex;
                }
                item = null;
            }
        }
    }

    /**
	 * @des:删除当前数据索引下数据
	 */
    public void DelItem(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= dataCount)
        {
            return;
        }
        //删除item逻辑三种情况
        //1.只更新数据，不销毁gameObject,也不移除gameobject
        //2.更新数据，且移除gameObject,不销毁gameObject
        //3.更新数据，销毁gameObject

        bool isNeedDestroyGameObject = (listItem.Count >= dataCount);
        setDataCount(dataCount - 1);

        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            LoopListItme item = listItem[i];
            int oldIndex = item.Index;
            if (oldIndex == dataIndex)
            {
                listItem.Remove(item);
                if (isNeedDestroyGameObject)
                {
                    GameObject.Destroy(item.gameObject);
                }
                else
                {
                    item.Index = -1;
                    unUseItem.Enqueue(item);
                }
            }
            if (oldIndex > dataIndex)
            {
                item.Index = oldIndex - 1;
            }
        }
        setUpdateRectItem(getCurScrollPerLineIndex());
    }


    /**
	 * @des:获取当前index下对应Content下的本地坐标
	 * @param:index
	 * @内部使用
	*/
    public Vector3 getLocalPositionByIndex(int index)
    {
        float x = 0f;
        float y = 0f;
        float z = 0f;
        switch (arrangement)
        {
            case Arrangement.Horizontal: //水平方向
                x = (index / maxPerLine) * (cellWidth + cellWidthSpace);
                y = -(index % maxPerLine) * (cellHeight + cellHeightSpace);
                break;
            case Arrangement.Vertical://垂着方向
                x = (index % maxPerLine) * (cellWidth + cellWidthSpace);
                y = -(index / maxPerLine) * (cellHeight + cellHeightSpace);
                break;
        }
        return new Vector3(x, y, z);
    }

    /**
	 * @des:创建元素
	 * @param:dataIndex
	 */
    private void createItem(int dataIndex)
    {
        LoopListItme item;
        if (unUseItem.Count > 0)
        {
            item = unUseItem.Dequeue();
        }
        else
        {
            item = addChild(goItemPrefab, content).AddComponent<LoopListItme>();
        }
        item.WarpContent = this;
        item.Index = dataIndex;
        listItem.Add(item);
    }

    /**
	 * @des:当前数据是否存在List中
	 */
    private bool isExistDataByDataIndex(int dataIndex)
    {
        if (listItem == null || listItem.Count <= 0)
        {
            return false;
        }
        for (int i = 0; i < listItem.Count; i++)
        {
            if (listItem[i].Index == dataIndex)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// @des:根据Content偏移,计算当前开始显示所在数据列表中的行或列
    /// </summary>
    /// <returns></returns>
    private int getCurScrollPerLineIndex()
    {
        switch (arrangement)
        {
            case Arrangement.Horizontal: //水平方向
                return Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.x) / (cellWidth + cellWidthSpace));
            case Arrangement.Vertical://垂着方向
                return Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.y) / (cellHeight + cellHeightSpace));
        }
        return 0;
    }

    /**
	 * @des:更新Content SizeDelta
	 */
    private void setUpdateContentSize()
    {
        int lineCount = Mathf.CeilToInt((float)dataCount / maxPerLine);
        switch (arrangement)
        {
            case Arrangement.Horizontal:
                content.sizeDelta = new Vector2(cellWidth * lineCount + cellWidthSpace * (lineCount - 1), content.sizeDelta.y);
                break;
            case Arrangement.Vertical:
                content.sizeDelta = new Vector2(content.sizeDelta.x, cellHeight * lineCount + cellHeightSpace * (lineCount - 1));
                break;
        }


    }

    /**
	 * @des:实例化预设对象 、添加实例化对象到指定的子对象下
	 */
    private GameObject addChild(GameObject goPrefab, Transform parent)
    {
        if (goPrefab == null || parent == null)
        {
            Debug.LogError("异常。UIWarpContent.cs addChild(goPrefab = null  || parent = null)");
            return null;
        }
        GameObject goChild = GameObject.Instantiate(goPrefab) as GameObject;
        goChild.layer = parent.gameObject.layer;
        goChild.transform.SetParent(parent, false);

        return goChild;
    }

    void OnDestroy()
    {

        scrollRect = null;
        content = null;
        goItemPrefab = null;
        onInitializeItem = null;

        listItem.Clear();
        unUseItem.Clear();

        listItem = null;
        unUseItem = null;

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoopListMain : MonoBehaviour
{
    private List<Item> listItem;
    private LoopListContentItem warpContent;

    void Start()
    {
        //测试数据
        listItem = new List<Item>();
        for (int i = 0; i < 50; i++)
        {
            listItem.Add(new Item("测试:" + Random.Range(1, 1000)));
        }

        //scrollView 相关所需注意接口
        warpContent = gameObject.transform.GetComponentInChildren<LoopListContentItem>();
        warpContent.onInitializeItem = onInitializeItem;
        //注意：目标init方法必须在warpContent.onInitializeItem之后
        warpContent.Init(listItem.Count);
    }

    private void onInitializeItem(GameObject go, int dataIndex)
    {
        Text text = go.transform.Find("Text").GetComponent<Text>();
        text.text = "i:" + dataIndex + "_N:" + listItem[dataIndex].Name();
    }

    public void Add(int dataIndex)
    {
        listItem.Insert(dataIndex + 1, new Item("Insert" + Random.Range(1, 1000)));
        warpContent.AddItem(dataIndex + 1);
    }

    public void Sub(int dataIndex)
    {
        listItem.RemoveAt(dataIndex);
        warpContent.DelItem(dataIndex);
    }


}
//测试数据结构
public class Item
{
    private string name;
    public Item(string name)
    {
        this.name = name;
    }
    public string Name()
    {
        return name;
    }
    public void destroy()
    {
        name = null;
    }
}


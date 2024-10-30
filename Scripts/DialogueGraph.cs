﻿using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;


[Serializable, CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Dialogue Graph")]
public class DialogueGraph : NodeGraph
{
    public enum DataType { End, Dialogue, Option }
    public struct DialogueInfo { public Sprite sprite; public string name, context; }
    public List<string> names = new List<string>();

    //Information available for reading
    [HideInInspector] public DataType dataType; 
    [HideInInspector] public DialogueInfo dialogueInfo; 
    [HideInInspector] public List<string> optionInfo;

    Node node; int index; bool init;

    public DataType Next(int num = -1)
    {
        if (0 <= num) index = num;
        if (!init) Init(); //Find the start node
        else if (dataType != DataType.End) MoveOn(); 
        GetInfo(); return dataType;
    }

    void Init()
    {
        init = true;
        for (int i = 0; i < nodes.Count; i++)
        {
            bool isStartNode = true;
            foreach (NodePort port in nodes[i].Inputs) isStartNode &= !port.IsConnected;
            if (isStartNode) node = nodes[i];
        }
    }

    void GetInfo()
    {
        if (node is DialogueNode dialogueNode && index < dialogueNode.dialogueList.Count)
        {
            dialogueInfo.sprite = dialogueNode.dialogueList[index].sprite;
            dialogueInfo.name = names[dialogueNode.dialogueList[index].person];
            dialogueInfo.context = dialogueNode.dialogueList[index].context;
            dataType = DataType.Dialogue;
        }
        else if (node is OptionNode optionNode && index == 0)
        {
            optionInfo = optionNode.optionList;
            dataType = DataType.Option;
        }
        else dataType = DataType.End;
    }

    void MoveOn()
    {
        switch(node)
        {
            case DialogueNode dialogueNode:
                if (dialogueNode.dialogueList[index].type >= DialogueNode.PortType.Output) GetNodeAndIndex("dialogueList " + index);
                else if (++index >= dialogueNode.dialogueList.Count) node = null;
                break;
            case OptionNode optionNode:
                if (index >= optionNode.optionList.Count) node = null;
                else GetNodeAndIndex("optionList " + index);
                break;
            case EventNode eventNode:
                index = eventNode.Invoke(index);
                GetNodeAndIndex("eventList " + index);
                break;
            case CheckNode checkNode:
                GetNodeAndIndex(checkNode.GetNextStr());
                break;

        }

        if (node is EventNode || node is CheckNode) MoveOn();
    }

    void GetNodeAndIndex(string s)
    {
        NodePort port = node.GetOutputPort(s);
        if (port == null || port.Connection == null) node = null;
        else 
        { 
            port = port.Connection; node = port.node; 
            index = 0; int.TryParse(port.fieldName.Split(' ')[^1], out index); 
        }
    }
}
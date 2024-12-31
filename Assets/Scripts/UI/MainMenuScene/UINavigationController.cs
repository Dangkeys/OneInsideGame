#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
   [SerializeField, HideInInspector]
   private List<TKey> keyData = new List<TKey>();

   [SerializeField, HideInInspector]
   private List<TValue> valueData = new List<TValue>();

   void ISerializationCallbackReceiver.OnAfterDeserialize()
   {
       Clear();
       for (int i = 0; i < keyData.Count && i < valueData.Count; i++)
       {
           this[keyData[i]] = valueData[i];
       }
   }

   void ISerializationCallbackReceiver.OnBeforeSerialize()
   {
       keyData.Clear();
       valueData.Clear();

       foreach (var item in this)
       {
           keyData.Add(item.Key);
           valueData.Add(item.Value);
       }
   }
}

[Serializable]
public struct NavigationTarget
{
   public Transform? Parent;
   public GameObject? Target;
}

[Serializable]
public class UINavigationMap : UnitySerializedDictionary<Button, NavigationTarget> { }

public class UINavigationController : MonoBehaviour
{
   [SerializeField]
   private UINavigationMap navigationControls = new();

   private void Start()
   {
       InitializeNavigationControls();
   }

   private void InitializeNavigationControls()
   {
       foreach (var navigationControl in navigationControls)
       {
           navigationControl.Key.onClick.AddListener(() =>
           {
               ActivateUIElements(navigationControl.Value);
           });
       }
   }

   private void ActivateUIElements(NavigationTarget elements)
   {
       if (elements.Parent == null)
       {
           elements.Target?.SetActive(true);
           return;
       }

       foreach (Transform child in elements.Parent)
       {
           child.gameObject.SetActive(false);
       }
       
       elements.Target?.SetActive(true);
   }
}
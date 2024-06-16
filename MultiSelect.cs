﻿using EFT.UI.DragAndDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UIFixes
{
    public class MultiSelect
    {
        private static GameObject SelectedMarkTemplate;
        private static GameObject SelectedBackgroundTemplate;

        private static readonly Dictionary<GridItemView, ItemContextClass> SelectedItemViews = [];
        private static readonly List<ItemContextClass> SortedItemContexts = [];

        public static void Initialize()
        {
            // Grab the selection objects from ragfair as templates
            RagfairNewOfferItemView ragfairNewOfferItemView = ItemViewFactory.CreateFromPool<RagfairNewOfferItemView>("ragfair_layout");

            SelectedMarkTemplate = UnityEngine.Object.Instantiate(ragfairNewOfferItemView.R().SelectedMark, null, false);
            UnityEngine.Object.DontDestroyOnLoad(SelectedMarkTemplate);

            SelectedBackgroundTemplate = UnityEngine.Object.Instantiate(ragfairNewOfferItemView.R().SelectedBackground, null, false);
            UnityEngine.Object.DontDestroyOnLoad(SelectedBackgroundTemplate);

            ragfairNewOfferItemView.ReturnToPool();
        }

        public static void Toggle(GridItemView itemView)
        {
            if (!itemView.IsInteractable)
            {
                return;
            }

            if (SelectedItemViews.ContainsKey(itemView))
            {
                Deselect(itemView);
            }
            else
            {
                Select(itemView);
            }
        }

        public static void Clear()
        {
            // ToList() because we'll be modifying the collection
            foreach (GridItemView itemView in SelectedItemViews.Keys.ToList())
            {
                Deselect(itemView);
            }
        }

        public static void Select(GridItemView itemView)
        {
            if (itemView.IsInteractable && !SelectedItemViews.ContainsKey(itemView))
            {
                ItemContextClass itemContext = new ItemContextClass(itemView.ItemContext, itemView.ItemRotation);
                itemContext.GClass2813_0.OnDisposed += RugPull;
                itemContext.OnDisposed += RugPull;

                SelectedItemViews.Add(itemView, itemContext);
                SortedItemContexts.Add(itemContext);
                ShowSelection(itemView);
            }
        }

        public static void Deselect(GridItemView itemView)
        {
            if (SelectedItemViews.TryGetValue(itemView, out ItemContextClass itemContext))
            {
                itemContext.GClass2813_0.OnDisposed -= RugPull;
                itemContext.OnDisposed -= RugPull;
                itemContext.Dispose();
                SortedItemContexts.Remove(itemContext);
                SelectedItemViews.Remove(itemView);
                HideSelection(itemView);
            }
        }

        public static bool IsSelected(GridItemView itemView)
        {
            return SelectedItemViews.ContainsKey(itemView);
        }

        public static IEnumerable<ItemContextClass> ItemContexts
        {
            get { return SortedItemContexts; }
        }

        public static int Count
        {
            get { return SelectedItemViews.Count; }
        }

        public static bool Active
        {
            get { return SelectedItemViews.Count > 1; }
        }

        public static void ShowDragCount(DraggedItemView draggedItemView)
        {
            if (Count > 1)
            {
                GameObject textOverlay = new("MultiSelectText", [typeof(RectTransform), typeof(TextMeshProUGUI)]);
                textOverlay.transform.parent = draggedItemView.transform;
                textOverlay.transform.SetAsLastSibling();
                textOverlay.SetActive(true);

                RectTransform overlayRect = textOverlay.GetComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.anchoredPosition = new Vector2(0.5f, 0.5f);

                TextMeshProUGUI text = textOverlay.GetComponent<TextMeshProUGUI>();
                text.text = MultiSelect.Count.ToString();
                text.fontSize = 36;
                text.alignment = TextAlignmentOptions.Baseline;
            }
        }

        private static void RugPull()
        {
            throw new InvalidOperationException("ItemContext disposed before MultiSelect was done with it!");
        }

        private static void ShowSelection(GridItemView itemView)
        {
            GameObject selectedMark = itemView.transform.Find("SelectedMark")?.gameObject;
            if (selectedMark == null)
            {
                selectedMark = UnityEngine.Object.Instantiate(SelectedMarkTemplate, itemView.transform, false);
                selectedMark.name = "SelectedMark";
            }

            selectedMark.SetActive(true);

            GameObject selectedBackground = itemView.transform.Find("SelectedBackground")?.gameObject;
            if (selectedBackground == null)
            {
                selectedBackground = UnityEngine.Object.Instantiate(SelectedBackgroundTemplate, itemView.transform, false);
                selectedBackground.transform.SetAsFirstSibling();
                selectedBackground.name = "SelectedBackground";
            }

            selectedBackground.SetActive(true);
        }

        private static void HideSelection(GridItemView itemView)
        {
            GameObject selectedMark = itemView.transform.Find("SelectedMark")?.gameObject;
            GameObject selectedBackground = itemView.transform.Find("SelectedBackground")?.gameObject;

            selectedMark?.SetActive(false);
            selectedBackground?.SetActive(false);
        }
    }
}

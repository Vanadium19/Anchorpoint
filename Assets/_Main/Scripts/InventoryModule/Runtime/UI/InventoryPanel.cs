using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform sectionsContainer;
        [SerializeField] private ContainerSection sectionPrefab;

        [Header("Settings")]
        [SerializeField] private float sectionSpacing = 5f;

        public RectTransform SectionsContainer => sectionsContainer;
        public ContainerSection SectionPrefab => sectionPrefab;

        private readonly Dictionary<string, ContainerSection> _sectionsByItemId = new Dictionary<string, ContainerSection>();
        private List<ContainerSection> _sections = new List<ContainerSection>();

        private float _lastRefreshTime;
        private bool _pendingRefresh;
        private const float MinRefreshInterval = 0.016f;

        public void Initialize()
        {
            SetupSectionsContainer();
        }

        private void SetupSectionsContainer()
        {
            if (sectionsContainer == null) 
                return;

            var vlg = sectionsContainer.GetComponent<VerticalLayoutGroup>();

            if (vlg == null)
                vlg = sectionsContainer.gameObject.AddComponent<VerticalLayoutGroup>();

            var csf = sectionsContainer.GetComponent<ContentSizeFitter>();

            if (csf == null)
                csf = sectionsContainer.gameObject.AddComponent<ContentSizeFitter>();
        }

        public void EnsureSectionHasLayoutElement(ContainerSection section)
        {
            if (section == null) 
                return;

            var layoutElement = section.GetComponent<LayoutElement>();

            if (layoutElement == null)
                layoutElement = section.gameObject.AddComponent<LayoutElement>();

            layoutElement.minHeight = 40f;
            layoutElement.preferredHeight = -1;
            layoutElement.flexibleHeight = 0;
        }

        public void AddContainerSection(ContainerSection section)
        {
            if (section == null) 
                return;

            _sections.Add(section);

            if (section.ContainerItem != null && section.ContainerItem.ItemDataSo != null)
                _sectionsByItemId[section.ContainerItem.ItemDataSo.DisplayName] = section;

            if (section.ContainerItem != null)
                section.RefreshGridUISafe();
            else
                section.RefreshVisualsSafe();

            RefreshLayout();
        }

        public void AddSectionFirst(ContainerSection section)
        {
            if (section == null) 
                return;

            _sections.Insert(0, section);
            section.transform.SetAsFirstSibling();

            if (section.ContainerItem != null && section.ContainerItem.ItemDataSo != null)
                _sectionsByItemId[section.ContainerItem.ItemDataSo.DisplayName] = section;

            if (section.ContainerItem != null)
                section.RefreshGridUISafe();
            else
                section.RefreshVisualsSafe();

            RefreshLayout();
        }

        public void RemoveContainerSection(ContainerSection section)
        {
            if (section == null) 
                return;

            if (section.ContainerItem != null && section.ContainerItem.ItemDataSo != null)
                if (_sectionsByItemId.ContainsKey(section.ContainerItem.ItemDataSo.DisplayName))
                    _sectionsByItemId.Remove(section.ContainerItem.ItemDataSo.DisplayName);

            _sections.Remove(section);

            RefreshLayout();
        }

        public void RefreshAllSections()
        {
            float now = Time.unscaledTime;

            if (now - _lastRefreshTime < MinRefreshInterval)
            {
                if (!_pendingRefresh)
                {
                    _pendingRefresh = true;
                    Invoke(nameof(DelayedRefreshAllSections), MinRefreshInterval);
                }

                return;
            }

            _lastRefreshTime = now;
            DoRefreshAllSections();
        }

        private void DelayedRefreshAllSections()
        {
            _pendingRefresh = false;
            _lastRefreshTime = Time.unscaledTime;
            DoRefreshAllSections();
        }

        private void DoRefreshAllSections()
        {
            foreach (var section in _sections)
                section?.RefreshGridUI();
        }

        public void CloseAllSections()
        {
            foreach (var section in _sections.ToArray())
                section?.Close();

            _sections.Clear();
            _sectionsByItemId.Clear();
        }

        private void RefreshLayout()
        {
            if (sectionsContainer == null || !gameObject.activeInHierarchy) return;

            Canvas.ForceUpdateCanvases();

            var vlg = sectionsContainer.GetComponent<VerticalLayoutGroup>();

            if (vlg != null)
            {
                vlg.SetLayoutHorizontal();
                vlg.SetLayoutVertical();
            }

            var csf = sectionsContainer.GetComponent<ContentSizeFitter>();

            if (csf != null)
            {
                csf.SetLayoutHorizontal();
                csf.SetLayoutVertical();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(sectionsContainer);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Shows the models of installed modifications on the weapon prefab and hides the stock parts they replace.
    /// </summary>
    public class WeaponAttachmentsView : WeaponViewPart
    {
        [SerializeField] private WeaponAttachment[] attachments = Array.Empty<WeaponAttachment>();

        private Vector3 _aimPositionOffset;

        public override Vector3 AimPositionOffset => _aimPositionOffset;

        public override void OnModificationsChanged(IReadOnlyList<WeaponModificationConfig> modifications)
        {
            _aimPositionOffset = Vector3.zero;

            foreach (var attachment in attachments)
                SetObjectsActive(attachment.ShownObjects, false);

            foreach (var attachment in attachments)
                SetObjectsActive(attachment.HiddenObjects, true);

            foreach (var attachment in attachments)
            {
                if (!Contains(modifications, attachment.Modification))
                    continue;

                SetObjectsActive(attachment.ShownObjects, true);
                SetObjectsActive(attachment.HiddenObjects, false);
                _aimPositionOffset += attachment.AimPositionOffset;
            }
        }

        private static bool Contains(IReadOnlyList<WeaponModificationConfig> modifications,
            WeaponModificationConfig modification)
        {
            if (modification == null)
                return false;

            foreach (var installed in modifications)
            {
                if (installed == modification)
                    return true;
            }

            return false;
        }

        private static void SetObjectsActive(GameObject[] objects, bool isActive)
        {
            foreach (var target in objects)
            {
                if (target != null)
                    target.SetActive(isActive);
            }
        }
    }
}

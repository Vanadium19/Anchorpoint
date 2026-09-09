using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// Searchable type picker for any <c>[SerializeReference]</c> interface field — the same
    /// dropdown Unity uses for "Add Component". Used for both action steps and condition sets
    /// so every polymorphic field in the module picks its type the same way.
    /// </summary>
    public class ManagedReferenceTypeDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly Action<Type> _onSelected;

        /// <summary>Creates the dropdown listing every concrete type derived from <paramref name="baseType"/>.</summary>
        public ManagedReferenceTypeDropdown(AdvancedDropdownState state, Type baseType, Action<Type> onSelected) : base(state)
        {
            _baseType = baseType;
            _onSelected = onSelected;
            minimumSize = new Vector2(220f, 300f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_baseType.Name);
            root.AddChild(new Item("<None>", null));

            foreach (var type in GetConcreteTypes(_baseType))
                root.AddChild(new Item(type.Name, type));

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is Item typedItem)
                _onSelected(typedItem.Type);
        }

        /// <summary>Every non-abstract, non-generic type derived from <paramref name="baseType"/>, sorted by name.</summary>
        public static IEnumerable<Type> GetConcreteTypes(Type baseType) =>
            TypeCache.GetTypesDerivedFrom(baseType)
                .Where(type => !type.IsAbstract && !type.IsInterface && !type.IsGenericTypeDefinition)
                .OrderBy(type => type.Name);

        private class Item : AdvancedDropdownItem
        {
            public Item(string name, Type type) : base(name) => Type = type;

            public Type Type { get; }
        }
    }
}

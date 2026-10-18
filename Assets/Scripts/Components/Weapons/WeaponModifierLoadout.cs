using Components.Modifiers;
using UnityEngine;

namespace Components.Weapons
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponModifierLoadout : MonoBehaviour
    {
        [SerializeField] private ModifierDefinition[] _modifiers;

        private void Start()
        {
            Weapon weapon = GetComponent<Weapon>();

            foreach (ModifierDefinition definition in _modifiers)
            {
                if (definition == null)
                    continue;

                weapon.AddModifier(definition);
            }
        }
    }
}

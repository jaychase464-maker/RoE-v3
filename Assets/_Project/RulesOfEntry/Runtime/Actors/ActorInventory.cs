using System;
using System.Linq;
using UnityEngine;

namespace RulesOfEntry.Actors
{
    [Serializable]
    public readonly struct ActorSearchResult
    {
        public ActorSearchResult(bool weaponFound, string[] discoveredItems)
        {
            WeaponFound = weaponFound;
            DiscoveredItems = discoveredItems ?? Array.Empty<string>();
        }

        public bool WeaponFound { get; }
        public string[] DiscoveredItems { get; }
        public string Summary => WeaponFound
            ? DiscoveredItems.Length == 0
                ? "Weapon recovered."
                : "Weapon recovered; " + string.Join(", ", DiscoveredItems)
            : DiscoveredItems.Length == 0
                ? "No reportable items found."
                : string.Join(", ", DiscoveredItems);
    }

    [DisallowMultipleComponent]
    public sealed class ActorInventory : MonoBehaviour
    {
        [SerializeField] private bool hasWeapon;
        [SerializeField] private bool weaponSecured;
        [SerializeField] private bool searched;
        [SerializeField] private string[] reportableItems = Array.Empty<string>();

        public bool HasWeapon => hasWeapon && !weaponSecured;
        public bool WeaponSecured => weaponSecured;
        public bool Searched => searched;

        public void Configure(bool configuredHasWeapon, string[] configuredReportableItems)
        {
            hasWeapon = configuredHasWeapon;
            weaponSecured = false;
            searched = false;
            reportableItems = configuredReportableItems?
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .ToArray() ?? Array.Empty<string>();
        }

        public ActorSearchResult Search()
        {
            searched = true;
            bool weaponFound = HasWeapon;
            weaponSecured |= weaponFound;
            string[] discovered = reportableItems.ToArray();
            return new ActorSearchResult(weaponFound, discovered);
        }
    }
}

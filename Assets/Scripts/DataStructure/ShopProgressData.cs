using System;
using System.Collections.Generic;

namespace DataStructure
{
    /// <summary>Stores the player's owned themes, selected themes, and powerup quantities.</summary>
    [Serializable]
    public class ShopProgressData
    {
        /// <summary>Current serialized save-data format version.</summary>
        public const int CurrentSchemaVersion = 1;

        /// <summary>Version of this saved progress structure.</summary>
        public int schemaVersion = CurrentSchemaVersion;
        /// <summary>Identifiers of themes the player owns.</summary>
        public List<int> ownedThemeIds = new List<int>();
        /// <summary>Selected world theme id, or -1 when no theme is selected.</summary>
        public int selectedWorldThemeId = -1;
        /// <summary>Selected player theme id, or -1 when no theme is selected.</summary>
        public int selectedPlayerThemeId = -1;
        /// <summary>Saved remaining quantities for purchased powerups.</summary>
        public List<PowerupInventoryEntry> powerupCounts = new List<PowerupInventoryEntry>();

        /// <summary>Repairs missing collections, removes duplicate themes, and merges valid powerup counts.</summary>
        public void Normalize()
        {
            if (schemaVersion <= 0)
                schemaVersion = CurrentSchemaVersion;

            if (ownedThemeIds == null)
                ownedThemeIds = new List<int>();

            if (powerupCounts == null)
                powerupCounts = new List<PowerupInventoryEntry>();

            var uniqueThemeIds = new List<int>();
            foreach (var themeId in ownedThemeIds)
            {
                if (!uniqueThemeIds.Contains(themeId))
                    uniqueThemeIds.Add(themeId);
            }
            ownedThemeIds = uniqueThemeIds;

            var uniquePowerupCounts = new List<PowerupInventoryEntry>();
            foreach (var entry in powerupCounts)
            {
                if (entry == null || entry.count <= 0)
                    continue;

                var existingEntry = uniquePowerupCounts.Find(item => item.powerupId == entry.powerupId);
                if (existingEntry == null)
                    uniquePowerupCounts.Add(new PowerupInventoryEntry(entry.powerupId, entry.count));
                else
                    existingEntry.count = (int)Math.Min(int.MaxValue, (long)existingEntry.count + entry.count);
            }
            powerupCounts = uniquePowerupCounts;
        }

        /// <summary>Returns the saved quantity for a powerup, or zero when it is absent.</summary>
        public int GetPowerupCount(int powerupId)
        {
            var entry = powerupCounts.Find(item => item.powerupId == powerupId);
            return entry == null ? 0 : entry.count;
        }

        /// <summary>Sets a powerup quantity, removing its entry when the count is zero or less.</summary>
        public void SetPowerupCount(int powerupId, int count)
        {
            var entry = powerupCounts.Find(item => item.powerupId == powerupId);
            if (count <= 0)
            {
                if (entry != null)
                    powerupCounts.Remove(entry);
                return;
            }

            if (entry == null)
                powerupCounts.Add(new PowerupInventoryEntry(powerupId, count));
            else
                entry.count = count;
        }
    }

    /// <summary>Stores one powerup id and its saved quantity.</summary>
    [Serializable]
    public class PowerupInventoryEntry
    {
        /// <summary>Catalog identifier of the powerup.</summary>
        public int powerupId;
        /// <summary>Number of uses currently available.</summary>
        public int count;

        /// <summary>Creates an empty inventory entry for serialization.</summary>
        public PowerupInventoryEntry()
        {
        }

        /// <summary>Creates an inventory entry with an id and available-use count.</summary>
        public PowerupInventoryEntry(int powerupId, int count)
        {
            this.powerupId = powerupId;
            this.count = count;
        }
    }
}

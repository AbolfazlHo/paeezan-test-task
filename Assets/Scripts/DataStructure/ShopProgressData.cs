using System;
using System.Collections.Generic;

namespace DataStructure
{
    [Serializable]
    public class ShopProgressData
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public List<int> ownedThemeIds = new List<int>();
        public int selectedWorldThemeId = -1;
        public int selectedPlayerThemeId = -1;
        public List<PowerupInventoryEntry> powerupCounts = new List<PowerupInventoryEntry>();

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

        public int GetPowerupCount(int powerupId)
        {
            var entry = powerupCounts.Find(item => item.powerupId == powerupId);
            return entry == null ? 0 : entry.count;
        }

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

    [Serializable]
    public class PowerupInventoryEntry
    {
        public int powerupId;
        public int count;

        public PowerupInventoryEntry()
        {
        }

        public PowerupInventoryEntry(int powerupId, int count)
        {
            this.powerupId = powerupId;
            this.count = count;
        }
    }
}

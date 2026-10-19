using System;
using System.Collections.Generic;
using DataStructure;

namespace Data
{
    /// <summary>Represents the root object of the shop catalog JSON file.</summary>
    [Serializable]
    public class ShopDataWrapper
    {
        /// <summary>All shop item definitions loaded from the JSON catalog.</summary>
        public List<ShopItemDataModel> items;
    }
}

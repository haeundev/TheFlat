using System.Collections.Generic;

namespace Proto
{
    public class Player
    {
        public enum Property
        {
            Name, Gold, Items
        }
        
        public string Name;
        public int Gold;
        public List<int> Items;
        public Dictionary<string, string> Dic;

        public Player(string name, int gold)
        {
            Name = name;
            Gold = gold;
            Items = new List<int>() { 1, 2, 3, 5 };
            Dic = new Dictionary<string, string>() { { "123", "hello!" } };
        }

        public static string GetPropertyString(Property property)
        {
            return property switch
            {
                Property.Name => "Name",
                Property.Gold => "Gold",
                Property.Items => "Items",
            };
        }
    }
}

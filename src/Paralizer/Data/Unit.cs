using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Paralizer.Data
{

    public class UnitTerrainModifier
    {
        public string type;
        public decimal attack { get; set; }
        public decimal defence { get; set; }
        public decimal movement { get; set; }
    }

    public class Unit
    {
        public static string[] terra = new string[] {"plains",
    "river",
    "arctic",
    "desert",
    "woods",
    "forest",
    "jungle",
    "hills",
    "mountain",
    "urban",
    "marsh",
    "amphibious",
    "night",
    "fort"};
        public string title { get; set; }
        public List<string> usable_by { get; set; }
        public string type { get; set; }
        public string sprite { get; set; }
        public string active { get; set; }
        public string is_buildable { get; set; }
        public string unit_group { get; set; }
        public string is_mobile { get; set; }
        public decimal max_strength { get; set; }
        public decimal default_organisation { get; set; }
        public decimal default_morale { get; set; }
        public decimal officers { get; set; }
        public decimal repair_cost_multiplier { get; set; }
        public decimal build_cost_ic { get; set; }
        public decimal build_cost_manpower { get; set; }
        public decimal build_time { get; set; }
        public decimal maximum_speed { get; set; }
        public decimal transport_weight { get; set; }
        public decimal supply_consumption { get; set; }
        public decimal fuel_consumption { get; set; }
        public decimal radio_strength { get; set; }
        public decimal defensiveness { get; set; }
        public decimal toughness { get; set; }
        public decimal softness { get; set; }
        public decimal air_defence { get; set; }
        public decimal armor_value { get; set; }
        public decimal suppression { get; set; }
        public decimal soft_attack { get; set; }
        public decimal hard_attack { get; set; }
        public decimal air_attack { get; set; }
        public decimal ap_attack { get; set; }
        public List<UnitTerrainModifier> terrain_modifiers { get; set; }

        public decimal combat_width { get; set; }
        public decimal completion_size { get; set; }
        public string on_completion { get; set; }
        public decimal priority { get; set; }

        public static Unit FromProperty(JProperty prop)
        {
            Unit u = Newtonsoft.Json.JsonConvert.DeserializeObject<Unit>(prop.Value.ToString());
            u.title = prop.Name;
            u.terrain_modifiers = new List<UnitTerrainModifier>();
            foreach (var t in terra)
            {
                var obj = (prop.Value as JObject);
                JToken tok;
                UnitTerrainModifier terrain = new UnitTerrainModifier() { type = t, attack = 0, movement = 0, defence = 0 };

                if (obj.TryGetValue(t, out tok))
                {
                    var obj2 = tok as JObject;
                    if (obj2 != null)
                    {
                        if (obj2.TryGetValue("attack", out tok))
                        {
                            terrain.attack = tok.Value<decimal>();
                        }

                        if (obj2.TryGetValue("movement", out tok))
                        {
                            terrain.movement = tok.Value<decimal>();
                        }

                        if (obj2.TryGetValue("defence", out tok))
                        {
                            terrain.defence = tok.Value<decimal>();
                        }
                    }

                }


                u.terrain_modifiers.Add(terrain);
            }
            return u;
        }

    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using Shin_Megami_Tensei_Model.Enums;

namespace Shin_Megami_Tensei_Model
{
    public class JsonCharacter
    {
        public string Name { get; set; }
        public JsonStats Stats { get; set; }
        public string[] Skills { get; set; }

        public Dictionary<string, string> Affinity { get; set; }
    }

    public class JsonStats
    {
        public int HP { get; set; }
        public int MP { get; set; }
        public int Str { get; set; }
        public int Skl { get; set; }
        public int Mag { get; set; }
        public int Spd { get; set; }
        public int Lck { get; set; }
    }
    
    public class JsonAbility
    {
        public string name { get; set; }
        public AbilityType type { get; set; }
        public int cost { get; set; }
        public int power { get; set; }
        public TargetType target { get; set; }
        public string hits { get; set; }
        public string effect { get; set; }
    }

    public static class DataLoader
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } 
        };

        public static List<JsonCharacter> LoadJsonCharacters(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return new List<JsonCharacter>();
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<JsonCharacter>>(json, _opts) ?? new List<JsonCharacter>();
        }

        
        public static List<JsonAbility> LoadJsonAbilities(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return new List<JsonAbility>();
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<JsonAbility>>(json, _opts) ?? new List<JsonAbility>();
        }
        public static Ability GetAbilityByName(string name, string abilitiesJsonPath)
        {
            var allAbilities = LoadJsonAbilities(abilitiesJsonPath);
            
            var foundAbility = allAbilities.FirstOrDefault(
                a => string.Equals(a.name, name.Trim(), StringComparison.OrdinalIgnoreCase)
            );

            if (foundAbility == null)
            {
                return null;
            }
    
            return new Ability(foundAbility);
        }
        static Attributes MapStats(JsonStats s)
        {
            if (s == null) return null;
            return new Attributes
            {
                MaxHp = s.HP,
                CurrentHp = s.HP,
                MaxMp = s.MP,
                CurrentMp = s.MP,
                StrikeDmg = s.Str,
                SkillDmg = s.Skl,
                MagicDmg = s.Mag,
                Speed = s.Spd,
                Lck = s.Lck
            };
        }
        
        static Affinity MapAffinity(Dictionary<string, string> affinityDict)
        {
            var affinity = new Affinity
            {
                Weak = [],
                Resist = [],
                Null = [],
                Repel = [],
                Drain = [],
            };

            if (affinityDict == null) return affinity;

            foreach (var kv in affinityDict)
            {
                if (!Enum.TryParse<AbilityType>(kv.Key, true, out var abilityType))
                    continue;

                AffinityType affType = kv.Value switch
                {
                    "-"   => AffinityType.Neutral,
                    "Wk"  => AffinityType.Weak,
                    "Rs"  => AffinityType.Resist,
                    "Nu"  => AffinityType.Null,
                    "Rp"  => AffinityType.Repel,
                    "Dr"  => AffinityType.Drain,
                    _     => AffinityType.Neutral
                };

                switch (affType)
                {
                    case AffinityType.Weak: affinity.Weak.Add(abilityType); break;
                    case AffinityType.Resist: affinity.Resist.Add(abilityType); break;
                    case AffinityType.Null: affinity.Null.Add(abilityType); break;
                    case AffinityType.Repel: affinity.Repel.Add(abilityType); break;
                    case AffinityType.Drain: affinity.Drain.Add(abilityType); break;
                }
            }

            return affinity;
        }
        
        public static Samurai GetSamuraiByName(string name, string samuraiJsonPath)
        {
            var list = LoadJsonCharacters(samuraiJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            var s = new Samurai { Name = j.Name, Attributes = MapStats(j.Stats), Affinity = MapAffinity(j.Affinity) };
            return s;
        }

        public static Monster GetMonstruoByName(string name, string monsterJsonPath, string abilitiesJsonPath)
        {
            var list = LoadJsonCharacters(monsterJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;

            var monster = new Monster 
            { 
                Name = j.Name, 
                Attributes = MapStats(j.Stats),
                Affinity = MapAffinity(j.Affinity) 
            };


            if (j.Skills != null)
            {
                foreach (var skillName in j.Skills)
                {
                    var ability = GetAbilityByName(skillName.Trim(), abilitiesJsonPath);

                    if (ability != null)
                    {
                        monster.AddAbility(ability);
                    }
                }
            }

            return monster;
        }
    }
}

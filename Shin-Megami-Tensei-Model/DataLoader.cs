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
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() } 
        };

        private static List<T> LoadJsonList<T>(string jsonPath)
        {
            if (!File.Exists(jsonPath)) return new List<T>();
            var json = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
        }

        public static Ability GetAbilityByName(string name, string abilitiesJsonPath)
        {
            var allAbilities = LoadJsonList<JsonAbility>(abilitiesJsonPath);
            
            var foundAbility = allAbilities.FirstOrDefault(
                jsonAbility => string.Equals(jsonAbility.name, name.Trim(), StringComparison.OrdinalIgnoreCase)
            );

            if (foundAbility == null)
            {
                return null;
            }
    
            return new Ability(foundAbility);
        }
        static Attributes MapStats(JsonStats data)
        {
            if (data == null) return null;
            return new Attributes
            {
                MaxHp = data.HP,
                CurrentHp = data.HP,
                MaxMp = data.MP,
                CurrentMp = data.MP,
                StrikeDmg = data.Str,
                SkillDmg = data.Skl,
                MagicDmg = data.Mag,
                Speed = data.Spd,
                Lck = data.Lck
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
            var list = LoadJsonList<JsonCharacter>(samuraiJsonPath);
            var jsonCharacter = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (jsonCharacter == null) return null;
            var s = new Samurai { Name = jsonCharacter.Name, Attributes = MapStats(jsonCharacter.Stats), Affinity = MapAffinity(jsonCharacter.Affinity) };
            return s;
        }

        public static Monster GetMonsterByName(string name, string monsterJsonPath, string abilitiesJsonPath)
        {
            var list = LoadJsonList<JsonCharacter>(monsterJsonPath);
            var jsonCharacter = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (jsonCharacter == null) return null;

            var monster = new Monster 
            { 
                Name = jsonCharacter.Name, 
                Attributes = MapStats(jsonCharacter.Stats),
                Affinity = MapAffinity(jsonCharacter.Affinity) 
            };


            if (jsonCharacter.Skills != null)
            {
                foreach (var skillName in jsonCharacter.Skills)
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

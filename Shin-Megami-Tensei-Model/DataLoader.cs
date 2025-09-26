using System.Text.Json;

namespace Shin_Megami_Tensei_Model
{
    public class JsonCharacter
    {
        public string Name { get; set; }
        public JsonStats Stats { get; set; }
        public string[] Skills { get; set; } 
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
        public string type { get; set; }
        public int cost { get; set; }
        public int power { get; set; }
        public string target { get; set; }
        public string hits { get; set; }
        public string effect { get; set; }
    }


    public static class DataLoader
    {
        private static readonly JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
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
        
        public static Samurai GetSamuraiByName(string name, string samuraiJsonPath)
        {
            var list = LoadJsonCharacters(samuraiJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            var s = new Samurai { Name = j.Name, Attributes = MapStats(j.Stats) };
            return s;
        }

        public static Monster GetMonstruoByName(string name, string monsterJsonPath, string abilitiesJsonPath)
        {
            var list = LoadJsonCharacters(monsterJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;

            var monster = new Monster { Name = j.Name, Attributes = MapStats(j.Stats) };

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

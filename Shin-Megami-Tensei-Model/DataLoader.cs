using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Shin_Megami_Tensei_Model
{
    public class JsonCharacter
    {
        public string name { get; set; }
        public JsonStats stats { get; set; }
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
            var j = list.FirstOrDefault(x => string.Equals(x.name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            var s = new Samurai { Name = j.name, Attributes = MapStats(j.stats) };
            return s;
        }

        public static Monster GetMonstruoByName(string name, string monsterJsonPath)
        {
            var list = LoadJsonCharacters(monsterJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            return new Monster { Name = j.name, Attributes = MapStats(j.stats) };
        }
    }
}

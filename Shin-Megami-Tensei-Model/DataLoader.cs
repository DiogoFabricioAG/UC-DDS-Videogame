// DataLoader.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Shin_Megami_Tensei_Model
{
    // DTOs para deserializar el JSON tal como está (name + stats)
    public class JsonCharacter
    {
        public string name { get; set; }
        public JsonStats stats { get; set; }
    }
    public class JsonStats
    {
        public double HP { get; set; }
        public double MP { get; set; }
        public double Str { get; set; }
        public double Skl { get; set; }
        public double Mag { get; set; }
        public double Spd { get; set; }
        public double Lck { get; set; }
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

        static Atributos MapStats(JsonStats s)
        {
            if (s == null) return null;
            return new Atributos
            {
                hpMaximo = s.HP,
                hpActual = s.HP,
                mpMaximo = s.MP,
                mpActual = s.MP,
                strikeDmg = s.Str,
                skillDmg = s.Skl,
                magicDmg = s.Mag,
                speed = s.Spd,
                lck = s.Lck
            };
        }

        public static Samurai GetSamuraiByName(string name, string samuraiJsonPath)
        {
            var list = LoadJsonCharacters(samuraiJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            var s = new Samurai { nombre = j.name, atributos = MapStats(j.stats) };
            // el constructor de Samurai ya crea el arreglo de habilidades
            return s;
        }

        public static Monstruo GetMonstruoByName(string name, string monsterJsonPath)
        {
            var list = LoadJsonCharacters(monsterJsonPath);
            var j = list.FirstOrDefault(x => string.Equals(x.name, name, StringComparison.OrdinalIgnoreCase));
            if (j == null) return null;
            return new Monstruo { nombre = j.name, atributos = MapStats(j.stats) };
        }
    }
}

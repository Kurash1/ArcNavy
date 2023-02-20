using ArcShared;
using System.Text;
//Locations
string DefinePath = "C:\\Users\\Jesse\\Documents\\Paradox Interactive\\Europa Universalis IV\\mod\\ESB\\common\\units\\naval definitions\\";
string OutputPath = "C:\\Users\\Jesse\\Documents\\Paradox Interactive\\Europa Universalis IV\\mod\\ESB\\common\\units\\naval definitions\\test\\";
string LocalizationPath = "C:\\Users\\Jesse\\Documents\\Paradox Interactive\\Europa Universalis IV\\mod\\ESB\\";
Dictionary<string, Dictionary<string, Dictionary<string, string>>> classes = new();
StringBuilder localizationfile = new("l_english:");
Arc.LoadClasses(File.ReadAllText($"{DefinePath}types.txt"), "cultures", classes);

string[] types = { "galley", "heavy_ship", "light_ship", "transport" };
Dictionary<string,(int min, int max)> MinMax = new();

foreach (string type in types)
{
    Arc.LoadClasses(File.ReadAllText($"{DefinePath}{type}.txt"), type, classes);
    MinMax.Add(type,FindMinMax(type));
}
foreach (KeyValuePair<string,Dictionary<string, string>> culture in classes["cultures"])
{
    if (culture.Key == "default")
        continue;
    foreach (string type in types)
    {
        int iGet(string name, int other)
        {
            if (culture.Value.ContainsKey(name))
                return int.Parse(culture.Value[name]);
            else if (classes["cultures"]["default"].ContainsKey(name))
                return int.Parse(classes["cultures"]["default"][name]);
            else
                return other;
        }
        for (int i = Math.Max(MinMax[type].min, iGet("min_level", MinMax[type].min)); i < Math.Min(MinMax[type].max, iGet("max_level", MinMax[type].max)); i++)
        {
            string ship = i.ToString();
            string path = $"{OutputPath}{culture.Key}_{type.ToUpper()[0]}{ship.PadLeft(2, '0')}.txt";
            string file =
@$"type = {type}
hull_size = {Get("hull_size", "0")                                                                }
base_cannons = {Get("base_cannons", "0")                                                          }
blockade = {Get("blockade", "0")                                                                  }
sail_speed = {Get("sail_speed", "0.0")                                                              }
sailors = {Get("sailors", "0")                                                                    }
sprite_level = {Get("sprite_level", "0")                                                          }";
            File.WriteAllText(path, file);
            string Get(string key, string format)
            {
                float modifier = 1f;
                modifier += cfGet($"{key}");
                modifier += cfGet($"{type}_{key}");
                modifier += cfGet($"{type}_{key}_{i}");
                return (sfGet(key) * modifier).ToString(format);
            }
            float sfGet(string key)
            {
                if (classes[type][ship].ContainsKey(key))
                    return float.Parse(classes[type][ship][key]);
                else if (classes[type]["default"].ContainsKey(key))
                    return float.Parse(classes[type]["default"][key]);
                else
                    return 0f;
            }
            float cfGet(string key)
            {
                if (culture.Value.ContainsKey(key))
                    return float.Parse(culture.Value[key]);
                else if (classes["cultures"]["default"].ContainsKey(key))
                    return float.Parse(classes["cultures"]["default"][key]);
                else
                    return 0f;
            }
        }
    }
    if (culture.Value.ContainsKey("localization"))
    {
        List<string> KeyValues = Arc.ParseString(culture.Value["localization"]);
        for(int i = 0; i < KeyValues.Count; i += 2)
        {
            localizationfile.Append($"\n {KeyValues[i]} {KeyValues[i + 1]}");
        }
    }
}
if (!File.Exists(LocalizationPath + "localisation\\english\\naval_units_l_english.yml"))
    File.Create(LocalizationPath + "localisation\\english\\naval_units_english.yml").Dispose();
File.WriteAllText(LocalizationPath + "localisation\\english\\naval_units_english.yml", ConvertStringToUtf8Bom(localizationfile.ToString()));

string ConvertStringToUtf8Bom(string source)
{
    var data = Encoding.UTF8.GetBytes(source);
    var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
    var encoder = new UTF8Encoding(true);

    return encoder.GetString(result);
}
Console.Write("Press Any Key to Exit");
Console.ReadKey();
(int min, int max) FindMinMax(string type)
{
    int min = 0;
    while (!classes[type].ContainsKey(min.ToString()))
    {
        min++;
    }
    int max = min;
    while (classes[type].ContainsKey(max.ToString()))
    {
        max++;
    }
    return (min, max);
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace HuntTrainAssistant;

internal static class LocalizationManager
{
    private sealed class Translation
    {
        private readonly Dictionary<string, string> values = [];

        internal bool IsReady => values.Count > 0;

        internal Translation(string language)
        {
            var directory = Path.Combine(LocalizationPath, language);
            if(!Directory.Exists(directory))
            {
                PluginLog.Warning($"Localization directory not found: {directory}");
                return;
            }

            foreach(var file in Directory.GetFiles(directory, "*.json"))
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file));
                    if(data != null)
                    {
                        Flatten(data, "", values);
                    }
                }
                catch(Exception e)
                {
                    PluginLog.Error($"Failed to load localization file: {file}\n{e}");
                }
            }

            PluginLog.Information($"Loaded {values.Count} translations for {language}");
        }

        internal string Get(string key) => values.GetValueOrDefault(key);

        private static void Flatten(Dictionary<string, object> source, string prefix, Dictionary<string, string> target)
        {
            foreach(var (name, value) in source)
            {
                var key = prefix.Length == 0 ? name : $"{prefix}.{name}";
                switch(value)
                {
                    case string text:
                        target[key] = text;
                        break;
                    case JObject child:
                        Flatten(child.ToObject<Dictionary<string, object>>()!, key, target);
                        break;
                }
            }
        }
    }

    internal const string BaseLanguage = "en-US";

    private static readonly string LocalizationPath = Path.Combine(Svc.PluginInterface.AssemblyLocation.Directory!.FullName, "Localization");
    private static readonly Dictionary<string, Translation> Translations = [];

    internal static string[] AvailableLanguages { get; private set; } = [BaseLanguage];

    internal static void Initialize()
    {
        Translations.Clear();
        if(Directory.Exists(LocalizationPath))
        {
            foreach(var directory in Directory.EnumerateDirectories(LocalizationPath))
            {
                var language = Path.GetFileName(directory);
                var translation = new Translation(language);
                if(translation.IsReady)
                {
                    Translations[language] = translation;
                }
            }
        }

        AvailableLanguages = Translations.Keys.OrderBy(x => x).ToArray();
        if(!Translations.ContainsKey(BaseLanguage))
        {
            PluginLog.Error("Base localization is missing");
            AvailableLanguages = [BaseLanguage];
            return;
        }

        if(!Translations.ContainsKey(P.Config.Language))
        {
            P.Config.Language = BaseLanguage;
        }
    }

    internal static void SetLanguage(string language)
    {
        if(Translations.ContainsKey(language))
        {
            P.Config.Language = language;
        }
    }

    internal static string Get(string key, params object[] args)
    {
        var translation = Translations.GetValueOrDefault(P.Config.Language)?.Get(key)
            ?? Translations.GetValueOrDefault(BaseLanguage)?.Get(key);
        if(translation == null)
        {
            PluginLog.Warning($"Missing localization key: {key}");
            return key;
        }

        try
        {
            return args.Length == 0 ? translation : string.Format(translation, args);
        }
        catch(FormatException e)
        {
            PluginLog.Error($"Invalid localization format for key: {key}\n{e}");
            return translation;
        }
    }
}

internal static class Loc
{
    internal static string Get(string key, params object[] args) => LocalizationManager.Get(key, args);
}

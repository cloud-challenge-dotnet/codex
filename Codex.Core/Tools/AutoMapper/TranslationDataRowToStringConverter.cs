using AutoMapper;
using Codex.Core.Repositories;
using System.Globalization;
using System.Linq;

namespace Codex.Core.Tools.AutoMapper
{
    public class TranslationDataRowToStringConverter : ITypeConverter<TranslationDataRow, string>
    {
        public string Convert(TranslationDataRow source, string destination, ResolutionContext context)
        {
            string value;
            string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (source.ContainsKey(culture))
            {
                value = source[culture];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            if (culture != "en" && source.ContainsKey("en"))
            {
                value = source["en"];
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
            return source.FirstOrDefault(s => s.Key != "en" && s.Key != culture && !string.IsNullOrWhiteSpace(s.Value)).Value ?? "";
        }
    }
}

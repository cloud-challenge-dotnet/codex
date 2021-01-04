using AutoMapper;
using Codex.Core.Repositories;
using System.Globalization;

namespace Codex.Core.Tools.AutoMapper
{
    public class StringToTranslationDataRowConverter : ITypeConverter<string, TranslationDataRow>
    {
        public TranslationDataRow Convert(string source, TranslationDataRow destination, ResolutionContext context)
        {
            var translationDataRow = new TranslationDataRow();
            if (source != null)
            {
                translationDataRow.Add(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, source);
            }
            return translationDataRow;
        }
    }
}

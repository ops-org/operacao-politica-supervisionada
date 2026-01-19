using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace OPS.Importador.Comum.Utilities;


public sealed class CamaraEstadualDespesaTempClassMap : ClassMap<CamaraEstadualDespesaTemp>
{
    public CamaraEstadualDespesaTempClassMap()
    {
        Map(m => m.Cpf).Name("Codigo");
        Map(m => m.Nome);
        Map(m => m.NomeCivil);
        //Map(m => m.Partido);
        Map(m => m.Empresa);
        Map(m => m.CnpjCpf);
        Map(m => m.DataEmissao).TypeConverter(new NullableDateOnlyConverter());
        Map(m => m.TipoVerba);
        Map(m => m.TipoDespesa);
        Map(m => m.Valor);
        Map(m => m.DataVigencia).TypeConverter(new NullableDateOnlyConverter());
        Map(m => m.Documento);
        Map(m => m.Favorecido);
        Map(m => m.Observacao);
        Map(m => m.DataColeta).TypeConverter(new NullableDateOnlyConverter());

        Map(m => m.IdDeputado).Ignore();
        Map(m => m.Ano).Ignore();
        Map(m => m.Mes).Ignore();
        Map(m => m.Hash).Ignore();
        Map(m => m.Origem).Ignore();
    }
}

public class NullableDateTimeConverter : DefaultTypeConverter
{
    private readonly string _format;
    private readonly CultureInfo _culture;

    public NullableDateTimeConverter(string format = "yyyy-MM-dd HH:mm")
    {
        _format = format;
        _culture = CultureInfo.CreateSpecificCulture("en-US");
    }

    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (DateTime.TryParseExact(text, _format, _culture, DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParse(text, _culture, DateTimeStyles.None, out var result1))
            return result1;

        return base.ConvertFromString(text, row, memberMapData);
    }

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value == null || !(value is DateTime dateTime))
            return string.Empty;

        return dateTime.ToString(_format, _culture);
    }
}

public class NullableDateOnlyConverter : DefaultTypeConverter
{
    private readonly string _format;
    private readonly CultureInfo _culture;

    public NullableDateOnlyConverter(string format = "O")
    {
        _format = format;
        _culture = CultureInfo.CreateSpecificCulture("en-US");
    }

    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (DateOnly.TryParseExact(text, _format, _culture, DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParse(text, _culture, DateTimeStyles.None, out var result1))
            return DateOnly.FromDateTime(result1);

        return base.ConvertFromString(text, row, memberMapData);
    }

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value == null || !(value is DateOnly dateTime))
            return string.Empty;

        return dateTime.ToString(_format, _culture);
    }
}

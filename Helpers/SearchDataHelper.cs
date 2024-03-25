using System.Text.RegularExpressions;
using MediGuru.DataExtractionTool.Enumerations;
using MediGuru.DataExtractionTool.Models;

namespace MediGuru.DataExtractionTool.Helpers;

public static class SearchDataHelper
{
    public static EsSearchData ToElasticSearchModel(SearchData searchData)
    {
        
        var tariffCodeResult = Regex.Match(searchData.TariffCode, @"\d+").Value;
        return new EsSearchData
        {
            Id = searchData.Id,
            TariffCode = Convert.ToInt32(tariffCodeResult),
            TariffDescription = searchData.TariffDescription,
            HasOfficialDataPoints = searchData.HasOfficialDataPoints,
            HasUserDataPoints = searchData.HasUserDataPoints,
            StartPrice = searchData.StartPrice,
            EndPrice = searchData.EndPrice,
            MedicalAidSchemeId = searchData.MedicalAidSchemeId,
            MedicalAidSchemeName = searchData.MedicalAidSchemeName,
            Categories = CreateCategories(searchData.Categories),
            Doctors = CreateDoctors(searchData.Doctors),
            CreatedDate = searchData.CreationDate,
            UpdateDate = searchData.UpdateDate,
        };
    }

    private static List<EsCategory> CreateCategories(List<SearchDataPointDetail> items)
    {
        var finalItems = new List<EsCategory>();
        foreach (var item in items)
        {
            EsCategory category = item.Name.ToEnumFromDescriptionAttribute<EsCategory>();
            finalItems.Add(category);
        }

        return finalItems;
    }

    private static List<EsDoctorType> CreateDoctors(List<SearchDataPointDetail> items)
    {
        try
        {
            var finalItems = new List<EsDoctorType>();
            foreach (var item in items)
            {
                finalItems.Add(item.Name.ToEnumFromDescriptionAttribute<EsDoctorType>());
            }
            return finalItems;
        }
        catch (Exception ex)
        {
            throw;
        }
        
    }
}
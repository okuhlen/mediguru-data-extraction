using MediGuru.DataExtractionTool.Models;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class EsSearchDataRepository(MediGuruDbContext dbContext) : IEsSearchDataRepository
{
    public async Task<IList<SearchData>> Fetch(DateTime? startDate = null)
    {
        if (startDate.HasValue)
        {
            var allDataPoints = await (from dataPoints in dbContext.SearchDataPoints
                join categories in dbContext.Categories on dataPoints.CategoryId equals categories.CategoryId
                join doctors in dbContext.Disciplines on dataPoints.DisciplineId equals doctors.DisciplineId
                where dataPoints.DateAdded >= startDate
                select new
                {
                    Doctor = doctors,
                    Category = categories,
                    DataPointId = dataPoints.SearchDataId,
                }).ToListAsync().ConfigureAwait(false);
                    
            
            var query = from searchData in dbContext.SearchDatas
                join medicalScheme in dbContext.MedicalAidSchemes on searchData.MedicalAidSchemeId equals medicalScheme
                    .MedicalAidSchemeId
                where searchData.UpdateDate >= startDate
                select new
                {
                    searchData.Id,
                    searchData.TariffCode,
                    TariffDescription = searchData.TariffCodeDescription,
                    searchData.StartPrice,
                    searchData.EndPrice,
                    searchData.HasOfficialDataPoints,
                    searchData.HasUserDataPoints,
                    searchData.MedicalAidSchemeId,
                    MedicalAidSchemeName = medicalScheme.Name,
                    CreatedDate = searchData.CreatedDate,
                    LastUpdated = searchData.UpdateDate
                };

            var items = await query.ToListAsync().ConfigureAwait(false);
            return items.ConvertAll(x => new SearchData()
            {
                Id = x.Id,
                TariffCode = x.TariffCode,
                TariffDescription = x.TariffDescription,
                StartPrice = x.StartPrice,
                EndPrice = x.EndPrice,
                HasOfficialDataPoints = x.HasOfficialDataPoints,
                HasUserDataPoints = x.HasOfficialDataPoints,
                MedicalAidSchemeId = x.MedicalAidSchemeId,
                MedicalAidSchemeName = x.MedicalAidSchemeName,
                Categories = allDataPoints.Where(y => y.DataPointId == x.Id).Select(c => c.Category).DistinctBy(c => c.CategoryId).ToList().ConvertAll(z => new SearchDataPointDetail
                {
                    Id = z.CategoryId,
                    Name = z.Description
                }),
                Doctors = allDataPoints.Where(y => y.DataPointId == x.Id).Select(x => x.Doctor).DistinctBy(x => x.Description).ToList().ConvertAll(z => new SearchDataPointDetail
                {
                    Id = z.DisciplineId,
                    Name = z.Description,
                }),
                CreationDate = x.CreatedDate,
                UpdateDate = x.LastUpdated
                
            });
        }

        var allSearchDataPoints = await (from dataPoints in dbContext.SearchDataPoints
            join categories in dbContext.Categories on dataPoints.CategoryId equals categories.CategoryId
            join doctors in dbContext.Disciplines on dataPoints.DisciplineId equals doctors.DisciplineId
            select new
            {
                Doctor = doctors,
                Category = categories,
                DataPointId = dataPoints.SearchDataId,
            }).ToListAsync().ConfigureAwait(false);
        
        var searchQuery = from searchData in dbContext.SearchDatas
            join medicalScheme in dbContext.MedicalAidSchemes on searchData.MedicalAidSchemeId equals medicalScheme
                .MedicalAidSchemeId
            select new
            {
                searchData.Id,
                searchData.TariffCode,
                TariffDescription = searchData.TariffCodeDescription,
                searchData.StartPrice,
                searchData.EndPrice,
                searchData.HasOfficialDataPoints,
                searchData.HasUserDataPoints,
                searchData.MedicalAidSchemeId,
                MedicalAidSchemeName = medicalScheme.Name,
                CreatedDate = searchData.CreatedDate,
                LastUpdated = searchData.UpdateDate
            };

        var queryItems = await searchQuery.ToListAsync().ConfigureAwait(false);
        return queryItems.ConvertAll(x => new SearchData()
        {
            Id = x.Id,
            TariffCode = x.TariffCode,
            TariffDescription = x.TariffDescription,
            StartPrice = x.StartPrice,
            EndPrice = x.EndPrice,
            HasOfficialDataPoints = x.HasOfficialDataPoints,
            HasUserDataPoints = x.HasOfficialDataPoints,
            MedicalAidSchemeId = x.MedicalAidSchemeId,
            MedicalAidSchemeName = x.MedicalAidSchemeName,
            Categories = allSearchDataPoints.Where(y => y.DataPointId == x.Id).Select(c => c.Category).DistinctBy(c => c.CategoryId).ToList().ConvertAll(z => new SearchDataPointDetail
            {
                Id = z.CategoryId,
                Name = z.Description
            }),
            Doctors = allSearchDataPoints.Where(y => y.DataPointId == x.Id).Select(x => x.Doctor).DistinctBy(x => x.Description).ToList().ConvertAll(z => new SearchDataPointDetail
            {
                Id = z.DisciplineId,
                Name = z.Description
            }),
            CreationDate = x.CreatedDate,
            UpdateDate = x.LastUpdated
        });
    }
}
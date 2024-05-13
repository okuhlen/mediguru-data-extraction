using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Helpers;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

public sealed class WoolTruFileProcessor(
    IProcedureRepository procedureRepository,
    IProviderRepository providerRepository,
    MediGuruDbContext dbContext,
    IProviderProcedureRepository providerProcedureRepository,
    IProviderProcedureDataSourceTypeRepository dataSourceRepository,
    IDisciplineRepository disciplineRepository,
    ICategoryRepository categoryRepository)
{
    private Category FetchByFileName(List<Category> categoriesList, string fileName)
    {
        switch (fileName)
        {
            case WoolTruFileNameConstants.Anaestheist:
                return categoriesList.First(x => x.Description == "Anesthesiology");

            case WoolTruFileNameConstants.Cardiologist:
                return categoriesList.First(x => x.Description == "Cardiology");

            case WoolTruFileNameConstants.ClinicalHaematologists:
                return categoriesList.First(x => x.Description == "Haematology");

            case WoolTruFileNameConstants.MedicalTechnology:
            case WoolTruFileNameConstants.ClinicalTechnologies:
                return categoriesList.First(x => x.Description == "Technology");

            case WoolTruFileNameConstants.DentalPractitioners:
            case WoolTruFileNameConstants.DentalTherapists:
            case WoolTruFileNameConstants.Orthodontists:
            case WoolTruFileNameConstants.OrthopaedicSurgeons:
            case WoolTruFileNameConstants.Periodontists:
            case WoolTruFileNameConstants.Prosthodontists:
                return categoriesList.First(x => x.Description == "Dentistry");

            case WoolTruFileNameConstants.Dermatologists:
                return categoriesList.First(x => x.Description == "Dermatology");

            case WoolTruFileNameConstants.Gastroenterologist:
                return categoriesList.First(x => x.Description == "Gastroenterology");

            //todo: fix typo
            case WoolTruFileNameConstants.GeneralPractitioner:
                return categoriesList.First(x => x.Description == "GP (General Practioner)");

            case WoolTruFileNameConstants.Gynaecologist:
                return categoriesList.First(x => x.Description == "Gynaecology");

            case WoolTruFileNameConstants.MaxilloFacialAndOralSurg:
            case WoolTruFileNameConstants.PlasticAndReconstructiveSurgeon:
                return categoriesList.First(x => x.Description == "Plastic Recon Surgery");

            case WoolTruFileNameConstants.NeuroSurgeons:
                return categoriesList.First(x => x.Description == "NeuroSurgery");

            case WoolTruFileNameConstants.Neurologists:
                return categoriesList.First(x => x.Description == "Neurology");

            case WoolTruFileNameConstants.NursingServices:
                return categoriesList.First(x => x.Description == "Nursing");

            case WoolTruFileNameConstants.OccupationalTherapists:
            case WoolTruFileNameConstants.Physiotherapists:
                return categoriesList.First(x => x.Description == "Therapists");

            case WoolTruFileNameConstants.Oncology:
            case WoolTruFileNameConstants.RadiationOncology:
                return categoriesList.First(x => x.Description == "Oncology");

            case WoolTruFileNameConstants.Ophthalmologists:
                return categoriesList.First(x => x.Description == "Ophthalmology");

            case WoolTruFileNameConstants.Otorhinolaryngologists:
                return categoriesList.First(x => x.Description == "ENT (Ears Nose Throat Specialist)");

            case WoolTruFileNameConstants.Paediatricians:
                return categoriesList.First(x => x.Description == "Paediatrician");

            case WoolTruFileNameConstants.Pathologists:
                return categoriesList.First(x => x.Description == "Pathology");

            case WoolTruFileNameConstants.Physician:
                return categoriesList.First(x => x.Description == "Physician");

            case WoolTruFileNameConstants.Psychiatrists:
                return categoriesList.First(x => x.Description == "Psychiatry");

            case WoolTruFileNameConstants.Pulmonologist:
                return categoriesList.First(x => x.Description == "Pulmonology");

            case WoolTruFileNameConstants.Radiologists:
                return categoriesList.First(x => x.Description == "Radiology");

            case WoolTruFileNameConstants.Rheumatologist:
                return categoriesList.First(x => x.Description == "Rheumathology");

            case WoolTruFileNameConstants.Surgeons:
            case WoolTruFileNameConstants.ThoracicSurgeons:
                return categoriesList.First(x => x.Description == "Surgeons");

            case WoolTruFileNameConstants.Urologist:
                return categoriesList.First(x => x.Description == "Urology");

            default:
                return categoriesList.First(x => x.Description == "Uncategorized");
        }
    }

    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var categories = await categoryRepository.FetchAll();
            var cats = categories.ToList();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var provider = await providerRepository.FetchByName("WoolTru Healthcare Fund");
            var dataSourceType = await dataSourceRepository.FetchByNameAsync("WoolTru");
            Console.WriteLine("Now processing WoolTru data files. This will take a while. Please wait...");
            foreach (var file in Directory.GetFiles($"{Directory.GetCurrentDirectory()}/Files/WoolTru", "*.txt"))
            {
                var fileInfo = new FileInfo(file);
                fileInfo.Refresh();
                var category = FetchByFileName(cats, fileInfo.Name);
                var disciplineString = WoolTruFileNameHelper.GetDisciplineByFileName(fileInfo.Name);
                var discipline = await disciplineRepository.FetchByName(disciplineString);
                if (discipline is null ||
                    disciplineString.Equals("surgeons", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine($"Cannot find discipline for file: {file}. File skipped");
                    continue;
                }

                using var streamReader = new StreamReader(file);
                int lineNumber = 0;
                ProviderProcedure? providerProcedure = null;
                while (!streamReader.EndOfStream)
                {
                    //read the procedure code
                    if (lineNumber == 0)
                    {
                        var code = await streamReader.ReadLineAsync() ??
                                   throw new NullReferenceException("No tariff code was read");
                        
                        if (!int.TryParse(code, out _))
                        {
                            Console.WriteLine($"Could not parse the tariff code provided: {code}");
                            continue;
                        }

                        var procedure = await procedureRepository.FetchByCodeAndCategoryId(code, category.CategoryId).ConfigureAwait(false);
                        if (procedure == null)
                        {
                            continue;
                        }

                        providerProcedure = new ProviderProcedure
                        {
                            ProcedureId = procedure.ProcedureId,
                            ProviderProcedureDataSourceTypeId = dataSourceType.ProviderProcedureDataSourceTypeId,
                            ProviderId = provider.ProviderId,
                            DisciplineId = discipline.DisciplineId,
                            YearValidFor = 2023,
                        };
                        lineNumber++;
                        continue;
                    }

                    if (lineNumber == 1)
                    {
                        await streamReader.ReadLineAsync();
                        lineNumber++;
                        continue;
                    }

                    if (lineNumber == 2)
                    {
                        var price = streamReader.ReadLine().Trim();
                        ArgumentNullException.ThrowIfNull(price);
                        var formattedPrice = FormattingHelpers.GetFormattedWoolTruPrice(price);
                        providerProcedure.Price = formattedPrice;
                        providerProcedure.AdditionalNotes = null;
                        await providerProcedureRepository.InsertAsync(providerProcedure, false).ConfigureAwait(false);
                        providerProcedure = null;
                        lineNumber = 0;
                    }
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private static class WoolTruFileNameHelper
    {
        public static string GetDisciplineByFileName(string name)
        {
            switch (name)
            {
                case WoolTruFileNameConstants.Anaestheist:
                    return "Anaesthetists";
                case WoolTruFileNameConstants.Cardiologist:
                    return "Cardiology";
                case WoolTruFileNameConstants.Dermatologists:
                    return "Dermatology";
                case WoolTruFileNameConstants.Neurologists:
                    return "Neurology";
                case WoolTruFileNameConstants.Gastroenterologist:
                    return "Gastroenterology";
                case WoolTruFileNameConstants.Gynaecologist:
                    return "Obstetrics and Gynaecology";
                case WoolTruFileNameConstants.Oncology:
                    return "Medical Oncology";
                case WoolTruFileNameConstants.Ophthalmologists:
                    return "Opthalmology";
                case WoolTruFileNameConstants.Orthodontists:
                    return "Orthodontics";
                case WoolTruFileNameConstants.Paediatricians:
                    return "Paediatrics";
                case WoolTruFileNameConstants.Pathologists:
                    return "Pathology";
                case WoolTruFileNameConstants.Periodontists:
                    return "Periodontics";
                case WoolTruFileNameConstants.MaxilloFacialAndOralSurg:
                    return "Maxillo-facial and Oral Surgery";
                case WoolTruFileNameConstants.PlasticAndReconstructiveSurgeon:
                    return "Plastic and Reconstructive Surgery";
                case WoolTruFileNameConstants.NeuroSurgeons:
                    return "Neurosurgery";
                case WoolTruFileNameConstants.NursingServices:
                    return "Registered nurses";
                case WoolTruFileNameConstants.OccupationalTherapists:
                    return "Occupational Therapy";
                case WoolTruFileNameConstants.Physiotherapists:
                    return "Physiotherapists";
                case WoolTruFileNameConstants.RadiationOncology:
                    return "Radiotherapy/Nuclear Medicine/Oncologist";
                case WoolTruFileNameConstants.Otorhinolaryngologists:
                    return "Otorhinolaryngology";
                case WoolTruFileNameConstants.Pulmonologist:
                    return "Pulmonology";
                case WoolTruFileNameConstants.Radiologists:
                    return "Diagnostic Radiology";
                case WoolTruFileNameConstants.Rheumatologist:
                    return "Spec.Phys/Int Med/Diabetes/Rheumatology/Nephrology/Endocrino";
                case WoolTruFileNameConstants.ThoracicSurgeons:
                    return "Cardio Thoracic Surgery";
                case WoolTruFileNameConstants.Urologist:
                    return "Urology";
                case WoolTruFileNameConstants.Physician:
                    return "Family Physician";
                case WoolTruFileNameConstants.DentalPractitioners:
                    return "General Dental Practice";
                case WoolTruFileNameConstants.DentalTherapists:
                    return "Dental therapy";
                case WoolTruFileNameConstants.Prosthodontists:
                    return "Prostodontics";
                case WoolTruFileNameConstants.GeneralPractitioner:
                    return "General Medical Practice";
                case WoolTruFileNameConstants.ClinicalHaematologists:
                    return "Clinical Haemotology";
                case WoolTruFileNameConstants.MedicalTechnology:
                    return "Medical technology - Pathology";
                case WoolTruFileNameConstants.ClinicalTechnologies:
                    return "Clinical Technology (General)";
                case WoolTruFileNameConstants.Psychiatrists:
                    return "Psychiatry";
                case WoolTruFileNameConstants.OrthopaedicSurgeons:
                    return "Orthopaedics";
                case WoolTruFileNameConstants.Surgeons:
                    return "Surgeons"; //TODO: How do I get all disciplines?
                default:
                    return "Uncategorized";
            }
        }
    }
}
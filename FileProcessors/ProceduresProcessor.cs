using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

internal sealed class ProceduresProcessor(
    MediGuruDbContext dbContext,
    IProcedureRepository procedureRepository,
    ICategoryRepository categoryRepository)
{
    /* 
     * A given code may appear in one or more text files, and this is because each discipline (eg: ophthalmologist, surgeon) may have different base rates set by the medical aid scheme.
     * The simplest would be to process the WoolTru text files; processing GEMS xlsx files need more thought.
     * WoolTru data files were manually copied from: https://wooltru-tariff.sctechnology.co.za/
     * Important to note here that the price is not to be added! the pricing will be added when providers are set up
     * SEE: MomentumFileProcessor as an example.
     * */
    public async Task ProcessAsync()
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var categoriesList = await categoryRepository.FetchAll();
            var cats = categoriesList.ToList();
            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var filesDirectory = $"{Directory.GetCurrentDirectory()}/Files/WoolTru/";
            foreach (var file in Directory.GetFiles(filesDirectory, "*.txt"))
            {
                using var streamReader = new StreamReader(file);
                int resetCount = 0;
                Procedure procedure = null;
                while (!streamReader.EndOfStream)
                {
                    if (resetCount == 0)
                    {
                        procedure = new Procedure
                        {
                            Code = streamReader.ReadLine()!.Trim(),
                            CreatedDate = DateTime.Now,
                        };
                        var category = FetchByFileName(cats, Path.GetFileName(file));
                        procedure.CategoryId = category.CategoryId;
                        procedure.Category = category;
                        resetCount++;
                        continue;
                    }

                    if (resetCount == 1)
                    {
                        procedure!.CodeDescriptor = streamReader.ReadLine();
                        resetCount++;
                        continue;
                    }

                    if (resetCount == 2)
                    {
                        await procedureRepository.InsertAsync(procedure, false);
                        procedure = null;
                        resetCount = 0;
                        //think this might be needed to move the cursor to the next line
                        var line = streamReader.ReadLine();
                        continue;
                    }
                }

                if (procedure != null)
                {
                    await procedureRepository.InsertAsync(procedure, false);
                }
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

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
}
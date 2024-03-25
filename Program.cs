using System.Diagnostics;
using MediGuru.DataExtractionTool;
using MediGuru.DataExtractionTool.Constants;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.FileProcessors;
using MediGuru.DataExtractionTool.FileProcessors.GEMS;
using MediGuru.DataExtractionTool.Models;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var stopwatch = Stopwatch.StartNew();
Console.Title = "MediGuru Data Extraction Tool v1";
Console.WriteLine("MEDIGURU DATA EXTRACTION TOOL v1 - Written by Okuhle Ngada");
Console.WriteLine();

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
var config = configBuilder.Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddLogging();

serviceCollection.AddSingleton<IConfiguration>(config);
serviceCollection.AddDbContext<MediGuruDbContext>(
    options =>
    {
        options.UseMySql(serverVersion: new MariaDbServerVersion(new Version(11, 2, 3)),
            mySqlOptionsAction: action =>
            {
                action.MigrationsHistoryTable("MediGuruMigrations");
                action.EnableIndexOptimizedBooleanColumns();
                action.MigrationsAssembly(AssemblyNameHelper.GetAssemblyName().FullName);
                action.EnableRetryOnFailure(6);
            },
            connectionString: config["ConnectionStrings:MediGuruUAT"]);
    });

serviceCollection.AddTransient<IProviderRepository, ProviderRepository>();
serviceCollection.AddTransient<IDisciplineRepository, DisciplineRepository>();
serviceCollection.AddTransient<IProcedureRepository, ProcedureRepository>();
serviceCollection.AddTransient<ICategoryRepository, CategoryRepository>();
serviceCollection.AddTransient<IProviderProcedureRepository, ProviderProcedureRepository>();
serviceCollection.AddTransient<IProviderProcedureDataSourceTypeRepository, ProviderProcedureDataSourceTypeRepository>();
serviceCollection.AddTransient<IProviderProcedureTypeRepository, ProviderProcedureTypeRepository>();
serviceCollection.AddTransient<ProviderInitializer>();
serviceCollection.AddTransient<MomentumFileProcessor>();
serviceCollection.AddTransient<DisciplinesFileProcessor>();
serviceCollection.AddTransient<ProceduresProcessor>();
serviceCollection.AddTransient<CategoriesFileProcessor>();
serviceCollection.AddTransient<ProviderProcedureDataSourceInitializer>();
serviceCollection.AddTransient<ProviderProcedureTypeInitializer>();
serviceCollection.AddTransient<WoolTruFileProcessor>();
serviceCollection.AddTransient<IMedicalAidNameRepository, MedicalAidNameRepository>();
serviceCollection.AddTransient<MedicalAidNamesProcessor>();
serviceCollection.AddTransient<MedicalLaboratoryTechnologistFileProcessor>();
serviceCollection.AddTransient<AcupunctureFileProcessor>();
serviceCollection.AddTransient<PathologyFileProcessor>();
serviceCollection.AddTransient<BiokineticsFileProcessor>();
serviceCollection.AddTransient<ChiropracticFileProcessor>();
serviceCollection.AddTransient<ClinicalTechnologyFileProcessor>();
serviceCollection.AddTransient<ContractedAnaesthesiologyFileProcessor>();
serviceCollection.AddTransient<ContractedDentalTherapyFileProcessor>();
serviceCollection.AddTransient<ContractedDentistsAndDentalSpecialistsFileProcessor>();
serviceCollection.AddTransient<ContractedMedicalPractitionersConsultativeServices>();
serviceCollection.AddTransient<ContractedMedicalPractitionersFileProcessor>();
serviceCollection.AddTransient<ContractedOralHygienistFileProcessor>();
serviceCollection.AddTransient<ContractedPhysiciansFileProcessor>();
serviceCollection.AddTransient<ContractedPsychiatryFileProcessor>();
serviceCollection.AddTransient<ContractedSurgeonsFileProcessor>();
serviceCollection.AddTransient<DentalTechnologyFileProcessor>();
serviceCollection.AddTransient<DieticiansFileProcessor>();
serviceCollection.AddTransient<HearingAidFileProcessor>();
serviceCollection.AddTransient<HomeopathsFileProcessor>();
serviceCollection.AddTransient<HospicesFileProcessor>();
serviceCollection.AddTransient<MedicalScientistFileProcessor>();
serviceCollection.AddTransient<WoolTruSurgeonsFileProcessor>();
serviceCollection.AddTransient<MentalHealthFileProcessor>();
serviceCollection.AddTransient<NaturopathyFileProcessor>();
serviceCollection.AddTransient<GenericGemsFileProcessor>();
serviceCollection.AddTransient<NursingFileProcessor>();
serviceCollection.AddTransient<RadiologyFileProcessor>();
serviceCollection.AddTransient<PsychometryRegisteredCounsellorsFileProcessor>();
serviceCollection.AddTransient<SpeechTherapyAudiologyFileProcessor>();

var serviceProvider = serviceCollection.BuildServiceProvider();

var momentumProcessor = serviceProvider.GetRequiredService<MomentumFileProcessor>();
var disciplinesProcessor = serviceProvider.GetRequiredService<DisciplinesFileProcessor>();
var proceduresProcessor = serviceProvider.GetRequiredService<ProceduresProcessor>();
var categoriesFileProcessor = serviceProvider.GetRequiredService<CategoriesFileProcessor>();
var providerInitializer = serviceProvider.GetRequiredService<ProviderInitializer>();
var sourceTypeInitializer = serviceProvider.GetRequiredService<ProviderProcedureDataSourceInitializer>();
var providerProcedureTypesInitializer = serviceProvider.GetRequiredService<ProviderProcedureTypeInitializer>();
var woolTruFileProcessor = serviceProvider.GetRequiredService<WoolTruFileProcessor>();
var dbContext = serviceProvider.GetRequiredService<MediGuruDbContext>();
var medicalAidProcessor = serviceProvider.GetRequiredService<MedicalAidNamesProcessor>();
var gemsMedicalTechnologyProcessor = serviceProvider.GetRequiredService<MedicalLaboratoryTechnologistFileProcessor>();
var gemsAcupuntureProcessor = serviceProvider.GetRequiredService<AcupunctureFileProcessor>();
var pathologyFileProcessor = serviceProvider.GetRequiredService<PathologyFileProcessor>();
var biokinetcsFileProcessor = serviceProvider.GetRequiredService<BiokineticsFileProcessor>();
var chiropractorsFileProcessor = serviceProvider.GetRequiredService<BiokineticsFileProcessor>();
var clinicalTechnologyFileProcessor = serviceProvider.GetRequiredService<ClinicalTechnologyFileProcessor>();
var contractedAnaesthisiologistProcessor = serviceProvider.GetRequiredService<ContractedAnaesthesiologyFileProcessor>();
var contractedDentalTherapist = serviceProvider.GetRequiredService<ContractedDentalTherapyFileProcessor>();
var contractedDentalSpecialistProcessor =
    serviceProvider.GetRequiredService<ContractedDentistsAndDentalSpecialistsFileProcessor>();
var contractedConsultativeServices =
    serviceProvider.GetRequiredService<ContractedMedicalPractitionersConsultativeServices>();
var contractedMedicalPractitionerFileProcessor =
    serviceProvider.GetRequiredService<ContractedMedicalPractitionersFileProcessor>();
var contractedOralHygienistsProcessor = serviceProvider.GetRequiredService<ContractedOralHygienistFileProcessor>();
var contractedPhysiciansFileProcessor = serviceProvider.GetRequiredService<ContractedPhysiciansFileProcessor>();
var contractedPsychiatryProcessor = serviceProvider.GetRequiredService<ContractedPsychiatryFileProcessor>();
var contractedSurgeonsProcessor = serviceProvider.GetRequiredService<ContractedSurgeonsFileProcessor>();
var contractedDentalTechnologyProcessor = serviceProvider.GetRequiredService<DentalTechnologyFileProcessor>();
var dieticiansFileProcessor = serviceProvider.GetRequiredService<DieticiansFileProcessor>();
var hearingAidProcessor = serviceProvider.GetRequiredService<HearingAidFileProcessor>();
var homeopathsProcessor = serviceProvider.GetRequiredService<HomeopathsFileProcessor>();
var hospicesFileProcessor = serviceProvider.GetRequiredService<HospicesFileProcessor>();
var medicalScientistsProcessor = serviceProvider.GetRequiredService<MedicalScientistFileProcessor>();
var wooltruSurgeonsProcessor = serviceProvider.GetRequiredService<WoolTruSurgeonsFileProcessor>();
var mentalHealthFileProcessor = serviceProvider.GetRequiredService<MentalHealthFileProcessor>();
var naturopathyFileProcessor = serviceProvider.GetRequiredService<NaturopathyFileProcessor>();
var dentalTherapyProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var oralHygieneProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var nonContractedDentistsAndDentalSpecialistsProcessor =
    serviceProvider.GetRequiredService<ContractedDentistsAndDentalSpecialistsFileProcessor>();
var nonContractedConsultativeServicesMedicalPractitioners =
    serviceProvider.GetRequiredService<ContractedMedicalPractitionersConsultativeServices>();
var nonContractedMedicalPractitioners2024 =
    serviceProvider.GetRequiredService<ContractedMedicalPractitionersFileProcessor>();
var nonContractedDentalTherapists = serviceProvider.GetRequiredService<ContractedDentalTherapyFileProcessor>();
var nonContractedOralHygienistProcessor = serviceProvider.GetRequiredService<ContractedOralHygienistFileProcessor>();
var nonContractedPsychiatristProcessor = serviceProvider.GetRequiredService<ContractedPsychiatryFileProcessor>();
var nursingFileProcessor = serviceProvider.GetRequiredService<NursingFileProcessor>();
var occupationalTherapistsFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var orthoptistsFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var orthoticAndProstheticsProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var physiotherapistsFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var phytotherapyFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var podiatryFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var psychologyFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var psychometryFileProcessor = serviceProvider.GetRequiredService<PsychometryRegisteredCounsellorsFileProcessor>();
var radiographyFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var radiologyFileProcessor = serviceProvider.GetRequiredService<RadiologyFileProcessor>();
var socialWorkersFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var sonographersFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var speechTherapyAudiologyFileProcessor = serviceProvider.GetRequiredService<SpeechTherapyAudiologyFileProcessor>();
var subAcuteFileProcessor = serviceProvider.GetRequiredService<GenericGemsFileProcessor>();
var contractedSurgeonsFileProcessor2023 = serviceProvider.GetRequiredService<ContractedSurgeonsFileProcessor>();

using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);
Console.WriteLine("Now clearing destination tables on database...");
await dbContext.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 0").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `ProviderProcedure`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `Procedure`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `Category`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `Discipline`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `ProviderProcedureDataSourceType`")
    .ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `ProviderProcedureType`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `Provider`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `MailingList`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE `MedicalAidScheme`").ConfigureAwait(false);
await dbContext.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS = 1").ConfigureAwait(false);
Console.WriteLine("Destination tables on database cleared!");
Console.WriteLine();
await transaction.CommitAsync().ConfigureAwait(false);

Console.WriteLine("Populating list of predefined medical aid names...");
await medicalAidProcessor.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating list of predefined Categories...");
await categoriesFileProcessor.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating list of predefined Disciplines...");
await disciplinesProcessor.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating list of predefined Sources of data...");
await sourceTypeInitializer.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating list of pre-defined provider procedure types...");
await providerProcedureTypesInitializer.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating list of procedures. Please wait a while...");
await proceduresProcessor.ProcessAsync().ConfigureAwait(false);
Console.WriteLine();
Console.WriteLine("Now creating Medical scheme providers...");
await providerInitializer.ProcessAsync().ConfigureAwait(false);
Console.WriteLine("Populating DB with 2023 WoolTru Rates. This will take a while...");
await woolTruFileProcessor.ProcessAsync().ConfigureAwait(false);

//hack
if (dbContext.Database.CurrentTransaction != null)
{
    await dbContext.Database.CommitTransactionAsync().ConfigureAwait(false);
}

Console.WriteLine("Populating DB with 2023 Momentum Rates. This will take a while...");
await momentumProcessor.ProcessAsync(BaseDirectories.Momentum2023BaseDirectory, 2023).ConfigureAwait(false);
Console.WriteLine("Populating DB with 2024 Momentum Rates. This will take a while...");
await momentumProcessor.ProcessAsync(BaseDirectories.Momentum2024BaseDirectory, 2024).ConfigureAwait(false);

//hack
if (dbContext.Database.CurrentTransaction != null)
{
    await dbContext.Database.CommitTransactionAsync().ConfigureAwait(false);
}

Console.WriteLine("Populating DB with 2023 and 2024 GEMS Rates. This will take a while...");
await contractedSurgeonsProcessor.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ContractedSurgeonsFile2024,
    YearValidFor = 2024,
    AdditionalNotes = "Contracted GEMS surgeons rates",
    CategoryName = "Surgeons",
    StartingRow = 110
}).ConfigureAwait(false);
await contractedSurgeonsFileProcessor2023.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ContractedSurgeonsFile2023,
    YearValidFor = 2023,
    AdditionalNotes = "Contracted GEMS surgeons rates",
    CategoryName = "Surgeons",
    StartingRow = 110,
}).ConfigureAwait(false);
await gemsMedicalTechnologyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = CategoryNameConstants.Technology,
    StartingRow = 13,
    FileLocation = GEMSFileLocations.MedicalLaboratoryTechnologists2024,
}).ConfigureAwait(false);
await gemsMedicalTechnologyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = CategoryNameConstants.Technology,
    StartingRow = 13,
    FileLocation = GEMSFileLocations.MedicalLaboratoryTechnologists2023,
}).ConfigureAwait(false);

await gemsAcupuntureProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = CategoryNameConstants.Acupuncture,
    StartingRow = 12,
    FileLocation = GEMSFileLocations.AcupunctureFile2024,
}).ConfigureAwait(false);
await gemsAcupuntureProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = CategoryNameConstants.Acupuncture,
    StartingRow = 12,
    FileLocation = GEMSFileLocations.AcupunctureFile2023,
}).ConfigureAwait(false);

await pathologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = CategoryNameConstants.Pathology,
    StartingRow = 1,
    FileLocation = GEMSFileLocations.PathologyFile2024,
}).ConfigureAwait(false);
await pathologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = CategoryNameConstants.Pathology,
    StartingRow = 2,
    FileLocation = GEMSFileLocations.PathologyFile2023,
}).ConfigureAwait(false);

await biokinetcsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.BiokineticsFile2024,
    CategoryName = "Biokinetics",
    StartingRow = 12,
}).ConfigureAwait(false);
await biokinetcsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.BiokineticsFile2023,
    CategoryName = "Biokinetics",
    StartingRow = 12,
}).ConfigureAwait(false);
await chiropractorsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.ChiropractorsFile2024,
    CategoryName = "Chiropractic",
    StartingRow = 11,
}).ConfigureAwait(false);
await chiropractorsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.ChiropractorsFile2023,
    CategoryName = "Chiropractic",
    StartingRow = 11,
}).ConfigureAwait(false);
await clinicalTechnologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ClinicalTechnologyFile2024,
    YearValidFor = 2024,
    StartingRow = 9,
    CategoryName = "Clinical Technology"
}).ConfigureAwait(false);
await clinicalTechnologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ClinicalTechnologyFile2023,
    YearValidFor = 2023,
    StartingRow = 9,
    CategoryName = "Clinical Technology",
}).ConfigureAwait(false);

await contractedAnaesthisiologistProcessor.ProcessAsync(new ProcessFileParameters
{
    AdditionalNotes = "Contracted Anaesthesiologist GEMS rate",
    StartingRow = 241,
    CategoryName = "Anaesthesiologist",
    YearValidFor = 2024,
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedAnaesthesiologistFile2024
}).ConfigureAwait(false);
await contractedAnaesthisiologistProcessor.ProcessAsync(new ProcessFileParameters
{
    AdditionalNotes = "Contracted Anaesthesiologist GEMS rate",
    StartingRow = 241,
    CategoryName = "Anaesthesiologist",
    YearValidFor = 2023,
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedAnaesthesiologistFile2023
}).ConfigureAwait(false);

await contractedDentalTherapist.ProcessAsync(new ProcessFileParameters
{
    AdditionalNotes = "Contracted Dental Therapist GEMS rate",
    StartingRow = 9,
    YearValidFor = 2024,
    CategoryName = "Dentistry",
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedDentalTherapy2024,
}).ConfigureAwait(false);
await contractedDentalTherapist.ProcessAsync(new ProcessFileParameters
{
    AdditionalNotes = "Contracted Dental Therapist GEMS rate",
    StartingRow = 9,
    YearValidFor = 2023,
    CategoryName = "Dentistry",
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedDentalTherapy2023,
}).ConfigureAwait(false);

await contractedDentalSpecialistProcessor.ProcessAsync(new ProcessFileParameters
{
    AdditionalNotes = "Contracted dental specialist GEMS rate",
    StartingRow = 72,
    YearValidFor = 2024,
    CategoryName = "Dentistry",
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedDentalSpecialists2024,
    EndingRow = 1156,
}).ConfigureAwait(false);

await contractedConsultativeServices.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.ContractedConsultativeServices2024,
    AdditionalNotes = "Contracted GEMS medical practitioners consultative rates",
    CategoryName = "Consultative Services",
    StartingRow = 5,
    IsContracted = true,
}).ConfigureAwait(false);
await contractedConsultativeServices.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.ContractedConsultativeServices2023,
    AdditionalNotes = "Contracted GEMS medical practitioners consultative rates",
    CategoryName = "Consultative Services",
    StartingRow = 5,
    IsContracted = true,
}).ConfigureAwait(false);

await contractedMedicalPractitionerFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.ContractedMedicalPractitioners2024,
    StartingRow = 109,
    IsContracted = true,
    AdditionalNotes = "Contracted GEMS medical practitioners",
}).ConfigureAwait(false);
await contractedMedicalPractitionerFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.ContractedMedicalPractitioners2023,
    StartingRow = 109,
    AdditionalNotes = "Contracted GEMS medical practitioners",
    IsContracted = true,
}).ConfigureAwait(false);

await contractedOralHygienistsProcessor.ProcessAsync(new ProcessFileParameters
{
    StartingRow = 6,
    YearValidFor = 2024,
    IsContracted = true,
    AdditionalNotes = "Contracted GEMS oral hygienists",
    CategoryName = "Oral Hygiene",
    FileLocation = GEMSFileLocations.ContractedOralHygienistsFile2024,
}).ConfigureAwait(false);
await contractedOralHygienistsProcessor.ProcessAsync(new ProcessFileParameters
{
    StartingRow = 6,
    YearValidFor = 2023,
    AdditionalNotes = "Contracted GEMS oral hygienists",
    CategoryName = "Oral Hygiene",
    IsContracted = true,
    FileLocation = GEMSFileLocations.ContractedOralHygienistsFile2023,
}).ConfigureAwait(false);

await contractedPhysiciansFileProcessor.ProcessAsync(new ProcessFileParameters
{
    StartingRow = 110,
    YearValidFor = 2024,
    IsContracted = true,
    AdditionalNotes = "Contracted GEMS physicians",
    FileLocation = GEMSFileLocations.ContractedPhysiciansFile2024
}).ConfigureAwait(false);
await contractedPhysiciansFileProcessor.ProcessAsync(new ProcessFileParameters
{
    StartingRow = 110,
    YearValidFor = 2023,
    AdditionalNotes = "Contracted GEMS physicians",
    FileLocation = GEMSFileLocations.ContractedPhysiciansFile2023,
    IsContracted = true,
}).ConfigureAwait(false);

await contractedPsychiatryProcessor.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ContractedPsychiatristFile2024,
    YearValidFor = 2024,
    StartingRow = 7,
    CategoryName = "Psychiatry",
    AdditionalNotes = "Contracted GEMS Psychiatrist tariffs",
    IsContracted = true,
}).ConfigureAwait(false);
await contractedPsychiatryProcessor.ProcessAsync(new ProcessFileParameters
{
    FileLocation = GEMSFileLocations.ContractedPsychiatristFile2023,
    YearValidFor = 2023,
    StartingRow = 7,
    CategoryName = "Psychiatry",
    IsContracted = true,
    AdditionalNotes = "Contracted GEMS Psychiatrist tariffs",
}).ConfigureAwait(false);

await contractedDentalTechnologyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.DentalTechniciansFile2024,
    StartingRow = 6,
    CategoryName = "Technology",
    IsContracted = true,
}).ConfigureAwait(false);
await contractedDentalTechnologyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.DentalTechniciansFile2023,
    StartingRow = 6,
    CategoryName = "Technology",
    IsContracted = true,
}).ConfigureAwait(false);

await dieticiansFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.DieticiansFile2024,
    StartingRow = 14,
    CategoryName = "Dietetics",
}).ConfigureAwait(false);
await dieticiansFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.DieticiansFile2023,
    StartingRow = 14,
    CategoryName = "Dietetics",
}).ConfigureAwait(false);

await hearingAidProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 9,
    CategoryName = "Audiology",
    FileLocation = GEMSFileLocations.HearingAidFile2024,
}).ConfigureAwait(false);
await hearingAidProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 9,
    CategoryName = "Audiology",
    FileLocation = GEMSFileLocations.HearingAidFile2023,
}).ConfigureAwait(false);

await homeopathsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.HomeopathsFile2024,
    CategoryName = "Homeopathy",
    StartingRow = 16
}).ConfigureAwait(false);
await homeopathsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.HomeopathsFile2023,
    CategoryName = "Homeopathy",
    StartingRow = 16,
}).ConfigureAwait(false);
await hospicesFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.HospicesFile2024,
    StartingRow = 7,
    CategoryName = "Hospices"
}).ConfigureAwait(false);
await hospicesFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.HospicesFile2023,
    StartingRow = 7,
    CategoryName = "Hospices"
}).ConfigureAwait(false);
await medicalScientistsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.MedicalScientistFile2024,
    CategoryName = "Medical Scientists",
    StartingRow = 7,
}).ConfigureAwait(false);
await medicalScientistsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.MedicalScientistFile2023,
    CategoryName = "Medical Scientists",
    StartingRow = 7,
}).ConfigureAwait(false);
await mentalHealthFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = "Mental Health",
    StartingRow = 13,
    FileLocation = GEMSFileLocations.MentalHealthFile2024
}).ConfigureAwait(false);
await mentalHealthFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = "Mental Health",
    StartingRow = 13,
    FileLocation = GEMSFileLocations.MentalHealthFile2023
}).ConfigureAwait(false);
await naturopathyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = "Naturopaths",
    FileLocation = GEMSFileLocations.NaturopathFile2024,
    StartingRow = 8,
}).ConfigureAwait(false);
await naturopathyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = "Naturopaths",
    FileLocation = GEMSFileLocations.NaturopathFile2023,
    StartingRow = 8,
}).ConfigureAwait(false);
await dentalTherapyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = "Dental Therapy",
    StartingRow = 9,
    FileLocation = GEMSFileLocations.NonContractedDentalTherapy2023,
    AdditionalNotes = "GEMS Non-Contracted dental therapists 2023",
    IsNonContracted = true,
}, "95", "Dental Therapy").ConfigureAwait(false);
await dentalTherapyProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = "Dental Therapy",
    StartingRow = 9,
    FileLocation = GEMSFileLocations.NonContractedDentalTherapy2024,
    AdditionalNotes = "GEMS Non-contracted dental therapists 2024",
    IsNonContracted = true,
}, "95", "Dental Therapy").ConfigureAwait(false);
await oralHygieneProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = "Oral Hygiene",
    StartingRow = 8,
    IsNonContracted = true,
    AdditionalNotes = "GEMS Non-Contracted oral hygienists",
    FileLocation = GEMSFileLocations.NonContractedOralHygiene2023,
}, "113", "Oral Hygienists").ConfigureAwait(false);
await oralHygieneProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = "Oral Hygiene",
    StartingRow = 8,
    IsNonContracted = true,
    AdditionalNotes = "GEMS non-contracted oral hygienists",
    FileLocation = GEMSFileLocations.NonContractedOralHygiene2024,
}, "113", "Oral Hygienists").ConfigureAwait(false);
await nonContractedDentistsAndDentalSpecialistsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    CategoryName = "Dentistry",
    StartingRow = 72,
    FileLocation = GEMSFileLocations.NonContractedDentistsAndDentalSpecialists2024,
    AdditionalNotes = "GEMS Non-Contracted dentists and dental specialists",
    IsNonContracted = true
}).ConfigureAwait(false);
await nonContractedDentistsAndDentalSpecialistsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    CategoryName = "Dentistry",
    StartingRow = 72,
    FileLocation = GEMSFileLocations.NonContractedDentistsAndDentalSpecialists2023,
    AdditionalNotes = "GEMS Non-contracted dentists and dental specialists",
    IsNonContracted = true,
}).ConfigureAwait(false);
await nonContractedConsultativeServicesMedicalPractitioners.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    AdditionalNotes = "GEMS non-contracted medical practitioners consultative services",
    StartingRow = 5,
    IsNonContracted = true,
    CategoryName = "Consultative Services",
    FileLocation = GEMSFileLocations.NonContractedMedicalPractitionersConsultativeServices2024,
}).ConfigureAwait(false);
await nonContractedConsultativeServicesMedicalPractitioners.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    AdditionalNotes = "GEMS non-contracted medical practitioners consultative services",
    IsNonContracted = true,
    FileLocation = GEMSFileLocations.NonContractedMedicalPractitionersConsultativeServices2023,
    CategoryName = "Consultative Services",
    StartingRow = 5,
}).ConfigureAwait(false);
await nonContractedMedicalPractitioners2024.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    AdditionalNotes = "GEMS non-contracted medical practitioners",
    IsNonContracted = true,
    FileLocation = GEMSFileLocations.NonContractedMedicalPractitionersFile2024,
    StartingRow = 40,
}).ConfigureAwait(false);
await nonContractedMedicalPractitioners2024.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    AdditionalNotes = "GEMS non-contracted medical practitioners",
    IsNonContracted = true,
    StartingRow = 40,
    FileLocation = GEMSFileLocations.NonContractedMedicalPractitionersFile2023
}).ConfigureAwait(false);
await nonContractedOralHygienistProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    AdditionalNotes = "GEMS non-contracted oral hygienists",
    IsNonContracted = true,
    StartingRow = 6,
    CategoryName = "Oral Hygiene",
    FileLocation = GEMSFileLocations.NonContractedOralHygiene2024
}).ConfigureAwait(false);
await nonContractedOralHygienistProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    AdditionalNotes = "GEMS non-contracted oral hygienists",
    IsNonContracted = true,
    StartingRow = 6,
    CategoryName = "Oral Hygiene",
    FileLocation = GEMSFileLocations.NonContractedOralHygiene2023
}).ConfigureAwait(false);
await nonContractedPsychiatristProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    AdditionalNotes = "GEMS non-contracted psychiatrist",
    CategoryName = "Psychiatry",
    FileLocation = GEMSFileLocations.NonContractedPsychiatristFile2024,
    StartingRow = 6,
    IsNonContracted = true
}).ConfigureAwait(false);
await nonContractedPsychiatristProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    AdditionalNotes = "GEMS non-contracted psychiatrist",
    CategoryName = "Psychiatry",
    FileLocation = GEMSFileLocations.NonContractedPsychiatristFile2023,
    StartingRow = 6,
    IsNonContracted = true,
}).ConfigureAwait(false);
await nursingFileProcessor.ProcessAsync(new ProcessFileParameters
{
    CategoryName = "Nursing",
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.NursingFile2024,
    StartingRow = 26,
}).ConfigureAwait(false);
await nursingFileProcessor.ProcessAsync(new ProcessFileParameters
{
    CategoryName = "Nursing",
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.NursingFile2023,
    StartingRow = 26
}).ConfigureAwait(false);
await occupationalTherapistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.OccupationalTherapistsFile2024,
    StartingRow = 22,
    CategoryName = "Occupational Therapy"
}, "66", "Occupational Therapists").ConfigureAwait(false);
await occupationalTherapistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.OccupationalTherapistsFile2023,
    StartingRow = 22,
    CategoryName = "Occupational Therapy",
}, "66", "Occupational Therapists").ConfigureAwait(false);
await orthoptistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 5,
    FileLocation = GEMSFileLocations.OrthoptistsFile2024,
    CategoryName = "Orthoptists"
}, "74", "Orthoptists").ConfigureAwait(false);
await orthoptistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 5,
    FileLocation = GEMSFileLocations.OrthoptistsFile2023,
    CategoryName = "Orthoptists"
}, "74", "Orthoptists").ConfigureAwait(false);
await orthoticAndProstheticsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.OrthoticAndProstheticsFile2024,
    StartingRow = 4,
    CategoryName = "Orthotics and Prosthetics"
}, "87", "Orthotist & Prosthetist").ConfigureAwait(false);
await orthoticAndProstheticsProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.OrthoticAndProstheticsFile2023,
    StartingRow = 4,
    CategoryName = "Orthotics and Prosthetics"
}, "87", "Orthotist & Prosthetist").ConfigureAwait(false);
await physiotherapistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.PhysiotherapistsFile2024,
    CategoryName = "Physiotherapy",
    StartingRow = 30,
}, "72", "Physiotherapists").ConfigureAwait(false);
await physiotherapistsFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.PhysiotherapistsFile2023,
    StartingRow = 30,
    CategoryName = "Physiotherapy"
}, "72", "Physiotherapists").ConfigureAwait(false);
await phytotherapyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.PhytotherapyFile2024,
    StartingRow = 7,
    CategoryName = "Phytotherapists"
}, "103", "Phytotherapy").ConfigureAwait(false);
await phytotherapyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.PhytotherapyFile2023,
    StartingRow = 7,
    CategoryName = "Phytotherapists"
}, "103", "Phytotherapy").ConfigureAwait(false);
await podiatryFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 17,
    FileLocation = GEMSFileLocations.PodiatristsFile2024,
    CategoryName = "Podiatrists",
}, "68", "Podiatry").ConfigureAwait(false);
await podiatryFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 17,
    FileLocation = GEMSFileLocations.PodiatristsFile2023,
    CategoryName = "Podiatrists",
}, "68", "Podiatry").ConfigureAwait(false);
await psychologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.PsychologistsFile2024,
    StartingRow = 15,
    CategoryName = "Psychology",
}, "86", "Psychologists").ConfigureAwait(false);
await psychologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.PsychologistsFile2023,
    StartingRow = 15,
    CategoryName = "Psychology",
}, "86", "Psychologists").ConfigureAwait(false);
await psychometryFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 8,
    FileLocation = GEMSFileLocations.PsychometryRegisteredCounsellorFile2024,
    CategoryName = "Psychometry and Counsellors"
}).ConfigureAwait(false);
await psychometryFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 8,
    FileLocation = GEMSFileLocations.PsychometryRegisteredCounsellorFile2023,
    CategoryName = "Psychometry and Counsellors"
}).ConfigureAwait(false);
await radiographyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 10,
    FileLocation = GEMSFileLocations.RadiographyFile2024,
    CategoryName = "Radiographers"
}, "39", "Radiography").ConfigureAwait(false);
await radiographyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 10,
    FileLocation = GEMSFileLocations.RadiographyFile2023,
    CategoryName = "Radiographers"
}, "39", "Radiography").ConfigureAwait(false);
await radiologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 29,
    FileLocation = GEMSFileLocations.RadiologyFile2024,
    CategoryName = "Radiology",
}).ConfigureAwait(false);
await radiologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 29,
    FileLocation = GEMSFileLocations.RadiologyFile2023,
    CategoryName = "Radiology",
}).ConfigureAwait(false);
await socialWorkersFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    StartingRow = 14,
    FileLocation = GEMSFileLocations.SocialWorkersFile2024,
    CategoryName = "Social Workers"
}, "89", "Social Workers").ConfigureAwait(false);
await socialWorkersFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    StartingRow = 14,
    FileLocation = GEMSFileLocations.SocialWorkersFile2023,
    CategoryName = "Social Workers"
}, "89", "Social Workers").ConfigureAwait(false);
await sonographersFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.SonographersFile2024,
    StartingRow = 6,
    CategoryName = "Sonographers",
}, "39", "Sonographers", "4").ConfigureAwait(false);
await sonographersFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.SonographersFile2023,
    StartingRow = 6,
    CategoryName = "Sonographers",
}, "39", "Sonographers", "4").ConfigureAwait(false);
await speechTherapyAudiologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.SpeechTherapyAudiologyFile2024,
    StartingRow = 13,
    CategoryName = "Speech Therapists & Audiologists",
}, new List<(string Code, string Name, string SubCode)>
{
    new("82", "Speech Therapy", "1"),
    new("82", "Audiology", "2")
}).ConfigureAwait(false);
await speechTherapyAudiologyFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.SpeechTherapyAudiologyFile2023,
    StartingRow = 13,
    CategoryName = "Speech Therapists & Audiologists",
}, new List<(string Code, string Name, string SubCode)>
{
    new("82", "Speech Therapy", "0"),
    new("83", "Audiology", "0"),
}).ConfigureAwait(false);
await subAcuteFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2024,
    FileLocation = GEMSFileLocations.SubAcuteFacilitiesFile2024,
    StartingRow = 7,
    CategoryName = "Sub-Acute Facilities"
}, "49", "Sub-acute Facilities").ConfigureAwait(false);
await subAcuteFileProcessor.ProcessAsync(new ProcessFileParameters
{
    YearValidFor = 2023,
    FileLocation = GEMSFileLocations.SubAcuteFacilitiesFile2023,
    StartingRow = 7,
    CategoryName = "Sub-Acute Facilities",
}, "49", "Sub-acute Facilities").ConfigureAwait(false);
Console.WriteLine();
Console.WriteLine(
    "Now Processing last WoolTru file: surgeons.txt. This processor makes use of existing disciplines which wouldnt be available earlier");
await wooltruSurgeonsProcessor.ProcessAsync().ConfigureAwait(false);

Console.Clear();
PrintHeading();
Console.WriteLine("Database has been pre-populated successfully!");
stopwatch.Stop();

using var writer = new StreamWriter("execution_report.txt", true);
writer.WriteLine($"{DateTime.Now}: The database took: {stopwatch.Elapsed.Hours} hours, {stopwatch.Elapsed.Minutes} minutes, and {stopwatch.Elapsed.Seconds} seconds to complete");

Console.WriteLine($"The database took: {stopwatch.Elapsed.Hours} hours, {stopwatch.Elapsed.Minutes} minutes, and {stopwatch.Elapsed.Seconds} seconds to complete");
Console.WriteLine("Execution results are written to file in bin directory");

static void PrintHeading()
{
    Console.WriteLine("MEDIGURU DATA EXTRACTOR v1");
    Console.WriteLine();
}
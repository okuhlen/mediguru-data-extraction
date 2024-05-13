using System.Text;
using ClosedXML.Excel;
using MediGuru.DataExtractionTool.DatabaseModels;
using MediGuru.DataExtractionTool.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.FileProcessors;

//NOTES:
//Momentum does not have tariff descriptors. As a makeshift solution, we use the descriptors from GEMS, if there is a match.
internal sealed class MomentumFileProcessor(
    MediGuruDbContext dbContext,
    IProviderRepository providerRepository,
    IDisciplineRepository disciplineRepository,
    IProviderProcedureRepository providerProcedureRepository,
    IProcedureRepository procedureRepository,
    IProviderProcedureDataSourceTypeRepository sourceTypeRepository)
{
    public async Task ProcessAsync(string baseDirectory, int yearValidFor)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var ticks = DateTime.Now.Ticks;
            await using var writer = new StreamWriter($"momentum_copy_results_{ticks}.txt", false, Encoding.UTF8);
            var provider = await providerRepository.FetchByName("Momentum Health").ConfigureAwait(false);
            var sourceType = await sourceTypeRepository.FetchByNameAsync("Momentum").ConfigureAwait(false);
            var disciplinesCodes = await disciplineRepository.FetchAll().ConfigureAwait(false);
            var proceduresList = await procedureRepository.FetchAll().ConfigureAwait(false);
            using var transaction = await dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

            if (!Directory.Exists(baseDirectory))
                Directory.CreateDirectory(baseDirectory);

            try
            {
                foreach (var file in Directory.GetFiles(baseDirectory, "*.xlsx"))
                {
                    Console.WriteLine($"Now processing file: {file}");
                    using var workbook = new XLWorkbook(file);
                    var worksheet = workbook.Worksheets.First() ??
                                    throw new Exception("There are no worksheets in this document");

                    var startingRow = GetLastRowToSkip(file);
                    foreach (var row in worksheet.Rows())
                    {
                        //rows 1 to 4 (or 3) are headers, which we do not need.
                        if (row.RowNumber() <= startingRow)
                            continue;

                        var tariffCodeText = row.Cell("A").GetString();
                        if (string.IsNullOrEmpty(tariffCodeText) || string.IsNullOrWhiteSpace(tariffCodeText))
                        {
                            Console.WriteLine($"Row {row.RowNumber()} skipped due to being empty");
                            continue;
                        }

                        if (!int.TryParse(tariffCodeText, out _))
                        {
                            Console.WriteLine($"Could not convert row: {row.RowNumber()} in {file}. Code: {tariffCodeText}");
                            continue; //move to next row
                        }
                        string disciplines;

                        if (row.Cell("B").TryGetValue<string>(out var d))
                        {
                            disciplines = d;
                        }
                        else if (row.Cell("B").TryGetValue<int>(out var e))
                        {
                            disciplines = Convert.ToString(e);
                        }
                        else if (row.Cell("B").TryGetValue<double>(out var f))
                        {
                            disciplines = Convert.ToString(f);
                        }
                        else
                        {
                            throw new Exception($"Could not get cell value: {row.Cell("B").Value.Type}");
                        }

                        double price = default;
                        if (row.Cell("C").TryGetValue<double>(out var cellValue))
                        {
                            price = cellValue;
                        }
                        else if (row.Cell("C").TryGetValue<string>(out var cellvalue) &&
                                 !string.IsNullOrWhiteSpace(cellvalue) && !string.IsNullOrEmpty(cellvalue))
                        {
                            price = Convert.ToDouble(cellvalue);
                        }
                        else
                        {
                            price = 0;
                        }

                        var procedure = proceduresList.FirstOrDefault(x => x.Code == tariffCodeText);
                        if (procedure == null)
                        {
                            await writer.WriteLineAsync(
                                $"ERROR: Could not find procedure code: {tariffCodeText}. File processing: {file}. Please investigate.").ConfigureAwait(false);
                            continue;
                        }

                        if (!disciplines.Contains(","))
                        {
                            var discipline = disciplinesCodes.FirstOrDefault(x => x.Code == disciplines);
                            if (discipline is null)
                            {
                                await writer.WriteLineAsync($"could not find discipline: {discipline}").ConfigureAwait(false);
                                continue;
                            }

                            var newOne = new ProviderProcedure()
                            {
                                ProviderId = provider.ProviderId,
                                DisciplineId = discipline.DisciplineId,
                                ProcedureId = procedure.ProcedureId,
                                Procedure = procedure,
                                Price = price,
                                ProviderProcedureDataSourceTypeId = sourceType.ProviderProcedureDataSourceTypeId,
                                ProviderProcedureDataSourceType = sourceType,
                                YearValidFor = yearValidFor,
                                NonPayable = string.IsNullOrWhiteSpace(disciplines),
                            };
                            await providerProcedureRepository.InsertAsync(newOne, false).ConfigureAwait(false);
                            continue;
                        }

                        //for each of the disciplines in the document, add the respective procedure code
                        var displineIds = disciplines.Split(",").ToList();
                        foreach (var disciplineCode in displineIds)
                        {
                            var discipline = disciplinesCodes.FirstOrDefault(x => x.Code == disciplineCode);
                            if (discipline is null)
                            {
                                await writer.WriteLineAsync(
                                    $"Missing Discipline: {disciplineCode}. Please investigate further");
                            }
                            else
                            {
                                var newProcedure = new ProviderProcedure()
                                {
                                    ProviderId = provider.ProviderId,
                                    DisciplineId = discipline.DisciplineId,
                                    Price = price,
                                    NonPayable = price == default,
                                    YearValidFor = yearValidFor,
                                    ProviderProcedureDataSourceTypeId = sourceType.ProviderProcedureDataSourceTypeId,
                                    ProviderProcedureDataSourceType = sourceType,
                                    ProcedureId = procedure.ProcedureId,
                                };

                                await providerProcedureRepository.InsertAsync(newProcedure, false)
                                    .ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (InvalidCastException)
            {
                await transaction.RollbackAsync();
                await writer.FlushAsync();
                writer.Close();
                throw;
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
            writer.Close();
        }).ConfigureAwait(false);
    }

    private int GetLastRowToSkip(string fileName)
    {
        //this is highly irregular but neccessary. 
        var fileInfo = new FileInfo(fileName);
        switch (fileInfo.Name)
        {
            case FileNameConstants.Pysiotherapists:
            case FileNameConstants.HealthcarePractitioners:
            case FileNameConstants.Radiologists:
            case FileNameConstants.GeneralPractioners:
            case FileNameConstants.Pathology:
                return 3;

            case FileNameConstants.Specialists:
                return 4;

            default:
                throw new NotSupportedException($"The selected file is not known: {fileName}");
        }
    }

    private class FileNameConstants
    {
        public const string Pathology = "momentum_health_pathologists.xlsx";
        public const string HealthcarePractitioners = "momentum_health_healthcare_practitioners.xlsx";
        public const string Pysiotherapists = "momentum_health_physiotherapists.xlsx";
        public const string Radiologists = "momentum_health_radiologists.xlsx";
        public const string Specialists = "momentum_health_specialists.xlsx";
        public const string GeneralPractioners = "momentum_health_general_practitioners.xlsx";
    }
}
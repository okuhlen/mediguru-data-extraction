using MediGuru.DataExtractionTool.Repositories;
using Quartz;

namespace MediGuru.DataExtractionTool.Tasks;

public sealed class SearchDataIndexerTask(
    ISearchDataIndexer searchDataIndexer,
    ITaskExecutionHistoryRepository taskExecutionHistoryRepository)
    : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Now preparing to index documents");
        var lastExecutionDate = await taskExecutionHistoryRepository
            .GetTaskLastSuccessfulRunDate(TaskNameConstants.SearchDataIndexer).ConfigureAwait(false);
        await searchDataIndexer.UpdateIndex(lastExecutionDate).ConfigureAwait(false);
        Console.WriteLine("Index complete");
    }
}
using Microsoft.EntityFrameworkCore;

namespace MediGuru.DataExtractionTool.Repositories;

public sealed class TaskExecutionHistoryRepository(MediGuruDbContext mediGuruDbContext)
    : ITaskExecutionHistoryRepository
{
    public async Task<DateTime?> GetTaskLastSuccessfulRunDate(string taskName)
    {
        var result = await mediGuruDbContext.TaskExecutionHistories
            .OrderByDescending(x => x.DateAdded)
            .FirstOrDefaultAsync(x => x.Success == true && x.Name == taskName)
            .ConfigureAwait(false);
        if (result is null)
        {
            return null;
        }

        return result.ExecutionEndTime;
    }
}
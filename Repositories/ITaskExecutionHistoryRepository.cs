namespace MediGuru.DataExtractionTool.Repositories;

public interface ITaskExecutionHistoryRepository
{
    Task<DateTime?> GetTaskLastSuccessfulRunDate(string taskName);
}
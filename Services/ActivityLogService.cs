using cv.deltareco.com.Data;
using Newtonsoft.Json;

public class ActivityLogService
{
    private readonly ApplicationDbContext _context;

    public ActivityLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string table, string action, string recordId, object oldData, object newData, string user)
    {
        var log = new cv.deltareco.com.Models.ActivityLog
        {
            TableName = table,
            ActionType = action,
            RecordId = recordId,
            OldValues = oldData != null ? JsonConvert.SerializeObject(oldData) : "{}",
            NewValues = newData != null ? JsonConvert.SerializeObject(newData) : "{}",
            PerformedBy = user,
            PerformedAt = DateTime.Now
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}

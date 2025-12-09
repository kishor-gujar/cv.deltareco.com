using cv.deltareco.com.Data;
using cv.deltareco.com.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class ActivityLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _http;

    public ActivityLogService(ApplicationDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http = http;
    }

    private string GetUser()
    {
        return _http.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }

    // CREATE LOG
    public async Task LogCreateAsync(object entity)
    {
        await LogAsync(entity, null, "Create");
    }

    // UPDATE LOG
    public async Task LogUpdateAsync(object oldEntity, object newEntity)
    {
        await LogAsync(newEntity, oldEntity, "Update");
    }

    // DELETE LOG
    public async Task LogDeleteAsync(object entity)
    {
        await LogAsync(entity, null, "Delete");
    }

    private async Task LogAsync(object entity, object oldEntity, string action)
    {
        if (entity == null) return;

        Type type = entity.GetType();
        string tableName = type.Name;

        // Primary Key
        var keyProp = type.GetProperty("Id");
        string recordId = keyProp?.GetValue(entity)?.ToString();

        // Serialize JSON safely
        string oldValues = oldEntity != null
            ? JsonConvert.SerializeObject(oldEntity)
            : "empty";

        string newValues = action == "Delete"
            ? "empty"
            : JsonConvert.SerializeObject(entity) ?? "empty";

        var log = new ActivityLog
        {
            TableName = tableName,
            ActionType = action,
            RecordId = recordId,
            OldValues = string.IsNullOrWhiteSpace(oldValues) ? "empty" : oldValues,
            NewValues = string.IsNullOrWhiteSpace(newValues) ? "empty" : newValues,
            PerformedBy = GetUser(),
            PerformedAt = DateTime.Now
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

}

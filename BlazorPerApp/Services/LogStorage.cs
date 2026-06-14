using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorPerApp.Models;
using Microsoft.JSInterop;

namespace BlazorPerApp.Services
{
    public class LogStorage
    {
        private const string StorageKey = "blazorperapp_logs";
        private readonly IJSRuntime _js;

        public LogStorage(IJSRuntime js)
        {
            _js = js;
        }

        public async Task AppendAsync(LogEntry entry)
        {
            var existingJson = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            List<LogEntry> list;
            if (string.IsNullOrEmpty(existingJson))
            {
                list = new List<LogEntry>();
            }
            else
            {
                try
                {
                    list = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
                catch
                {
                    list = new List<LogEntry>();
                }
            }

            list.Add(entry);
            var newJson = JsonSerializer.Serialize(list);
            await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, newJson);
        }

        public async Task<List<LogEntry>> GetAllAsync()
        {
            var result = new List<LogEntry>();

            var existingJson = await _js.InvokeAsync<string>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(existingJson))
            {
                try
                {
                    var main = JsonSerializer.Deserialize<List<LogEntry>>(existingJson);
                    if (main != null) result.AddRange(main);
                }
                catch { }
            }

            // Also read global JS errors captured by the index.html script
            try
            {
                var jsErrJson = await _js.InvokeAsync<string>("localStorage.getItem", "blazorperapp_global_errors");
                if (!string.IsNullOrEmpty(jsErrJson))
                {
                    var jsErrors = JsonSerializer.Deserialize<System.Collections.Generic.List<System.Text.Json.JsonElement>>(jsErrJson);
                    if (jsErrors != null)
                    {
                        foreach (var el in jsErrors)
                        {
                            var msg = el.GetProperty("message").GetString() ?? "";
                            var file = el.TryGetProperty("filename", out var f) ? f.GetString() : null;
                            var stack = el.TryGetProperty("error", out var s) ? s.GetString() : null;
                            result.Add(new LogEntry
                            {
                                Timestamp = DateTime.TryParse(el.GetProperty("time").GetString(), out var dt) ? dt : DateTime.UtcNow,
                                Level = "Error",
                                Category = "GlobalJS",
                                Message = msg + (file != null ? $" ({file})" : string.Empty),
                                Exception = stack
                            });
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        public async Task ClearAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }
}

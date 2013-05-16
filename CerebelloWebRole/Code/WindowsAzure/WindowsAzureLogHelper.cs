using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace CerebelloWebRole.Code.WindowsAzure
{
    public static class WindowsAzureLogHelper
    {
        public class TraceLogsEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Timestamp { get; set; }
            public string EventTickCount { get; set; }
            public string DeploymentId { get; set; }
            public string Role { get; set; }
            public string RoleInstance { get; set; }
            public int Level { get; set; }
            public int Pid { get; set; }
            public int Tid { get; set; }
            public int EventId { get; set; }
            public string Message { get; set; }
        }

        public static List<TraceLogsEntity> GetLogEvents(int page, int? filterLevel, string filterRoleInstance, string filterPath)
        {
            var storageAccount =
                CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));

            var cloudTableClient = storageAccount.CreateCloudTableClient();
            var serviceContext = cloudTableClient.GetDataServiceContext();
            IQueryable<TraceLogsEntity> traceLogsTable = serviceContext.CreateQuery<TraceLogsEntity>("WADLogsTable");

            var startDate = DateTime.UtcNow.AddHours(-3 * page).Ticks;
            var endDate = DateTime.UtcNow.AddHours(-3 * (page - 1)).Ticks;

            var selection = from row in traceLogsTable
                            where string.Compare(row.PartitionKey, "0" + startDate, StringComparison.Ordinal) > 0
                            where string.Compare(row.PartitionKey, "0" + endDate, StringComparison.Ordinal) <= 0
                            select row;

            if (filterLevel != null)
            {
                selection = from row in selection
                            where row.Level == filterLevel.Value
                            select row;
            }
            else
            {
                selection = from row in selection
                            where row.Level == 2 || row.Level == 3 || row.Level == 4
                            select row;
            }

            if (!string.IsNullOrWhiteSpace(filterRoleInstance))
                selection = from row in selection
                            where string.Compare(row.RoleInstance, filterRoleInstance, StringComparison.Ordinal) == 0
                            select row;

            if (!string.IsNullOrWhiteSpace(filterPath))
            {
                var filterPathNext = filterPath.Substring(0, filterPath.Length - 1) + (char)(filterPath[filterPath.Length - 1] + 1);

                selection = from row in selection
                            where string.Compare(row.Message, filterPath, StringComparison.Ordinal) >= 0
                            where string.Compare(row.Message, filterPathNext, StringComparison.Ordinal) < 0
                            select row;
            }

            var query = selection.AsTableServiceQuery();
            var result = query.Execute().ToList();
            result.Reverse();

            return result;
        }
    }
}
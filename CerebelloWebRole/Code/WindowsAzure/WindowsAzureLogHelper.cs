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
            public string Level { get; set; }
            public string EventId { get; set; }
            public string Pid { get; set; }
            public string Tid { get; set; }
            public string Message { get; set; }
        }

        public static List<TraceLogsEntity> GetLastDayLogEvents()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
            var cloudTableClient = storageAccount.CreateCloudTableClient();
            var serviceContext = cloudTableClient.GetDataServiceContext();
            IQueryable<TraceLogsEntity> traceLogsTable = serviceContext.CreateQuery<TraceLogsEntity>("WADLogsTable");
            var selection = from row in traceLogsTable where String.Compare(row.PartitionKey, "0" + DateTime.UtcNow.AddHours(-3).Ticks, StringComparison.Ordinal) >= 0 select row;
            var query = selection.AsTableServiceQuery<TraceLogsEntity>();
            var result = query.Execute().ToList().Where(i => new[] { "2", "3", "4" }.Contains(i.Level)).ToList();
            result.Reverse();

            return result;
        }
    }
}
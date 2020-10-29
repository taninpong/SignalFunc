// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Azure.Cosmos.Table;
using System.Linq;

namespace FunctionApp2
{
    public static class SignalFunc
    {
        [FunctionName("SignalFunc")]
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            //Microsoft.SignalRService.ClientConnectionConnected
            //Microsoft.SignalRService.ClientConnectionDisconnected
            var table = GetConnectionTable();
            var jsonDoc = JsonDocument.Parse(eventGridEvent.Data.ToString());
            var connId = jsonDoc.RootElement.GetProperty("connectionId").GetString();
            var userId = jsonDoc.RootElement.GetProperty("userId").GetString();
            log.LogInformation(eventGridEvent.EventType);
            log.LogInformation(connId);
            log.LogInformation(userId);
            //TODO  Insert And Delete UserLogin/Logout
            if (eventGridEvent.EventType.ToString() == "Microsoft.SignalRService.ClientConnectionConnected")
            {
                EmployeeEntity employeeEntity = new EmployeeEntity(userId, connId);
                TableOperation insertOperation = TableOperation.InsertOrReplace(employeeEntity);
                table.Execute(insertOperation);
            }
            else
            {
                var query = new TableQuery<EmployeeEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, connId));
                foreach (var item in table.ExecuteQuery(query))
                {
                    var delete = TableOperation.Delete(item);
                    table.Execute(delete);
                }
            }
        }

        private static CloudTable GetConnectionTable()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=signalab;AccountKey=xbad3br/3o0AglWZ4iM1WdepVOlm9CSoMRmDbUlvFYmmUmJTlHF2hxqvsnC99fELsLvhQE1YzAi1x3mLOh9Yhg==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference("demotable2");
        }
    }
}

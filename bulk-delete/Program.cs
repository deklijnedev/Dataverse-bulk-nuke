using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages; // Cruciaal voor BulkDeleteRequest!
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;

if (args.Length == 0) {
    Console.WriteLine("❌ ERROR: Je moet een schema name meegeven!");
    return;
}

string tableName = args[0].ToLower();
string url = "xxx.crm4.dynamics.com";
string clientId = "";
string clientSecret = "";
string connectionString = $"AuthType=ClientSecret;Url={url};ClientId={clientId};ClientSecret={clientSecret};SkipDiscovery=True";

using var client = new ServiceClient(connectionString);

if (!client.IsReady) {
    Console.WriteLine($"❌ Verbinding mislukt: {client.LastError}");
    return;
}

Console.WriteLine($"🛰️ Verbonden! GIGA-NUKE geactiveerd voor {tableName}...");

// De query die bepaalt WAT er weg moet (alles in dit geval)
var bulkDeleteQuery = new QueryExpression(tableName) {
    ColumnSet = new ColumnSet(false)
};

var bulkDeleteRequest = new BulkDeleteRequest {
    JobName = $"Nuke {tableName} - {DateTime.Now:yyyy-MM-dd HH:mm}",
    QuerySet = new[] { bulkDeleteQuery },
    StartDateTime = DateTime.Now,
    RecurrencePattern = string.Empty,
    SendEmailNotification = false,
    ToRecipients = new Guid[] { },
    CCRecipients = new Guid[] { }
};

try {
    // Hier stond in je screenshot waarschijnlijk een fout met 'client'
    var response = (BulkDeleteResponse)await client.ExecuteAsync(bulkDeleteRequest);
    Console.WriteLine("\n✅ BOEM! De Bulk Delete Job is afgevuurd op de server.");
    Console.WriteLine($"🆔 Job ID: {response.JobId}");
    Console.WriteLine("Dataverse ruimt die 1 miljoen records nu intern op. Je VPS kan chillen.");
}
catch (Exception ex) {
    Console.WriteLine($"❌ Fout tijdens het afvuren: {ex.Message}");
}

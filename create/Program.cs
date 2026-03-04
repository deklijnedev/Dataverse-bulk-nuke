using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Diagnostics;
using System.ServiceModel;

// --- CONFIG ---
string connectionString = "AuthType=ClientSecret;Url=https://xxx.crm4.dynamics.com;ClientId="";ClientSecret="";
int totaalRecords = 1000000; 
int batchGrootte = 1000;      // Maximaal voor Bypass mode
int maxThreads = 52;         

// --- DE BEUKER ASCII ART ---
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine(@"
██████  ███████     ██████  ███████ ██    ██ ██   ██ ███████ ██████  
██   ██ ██          ██   ██ ██      ██    ██ ██  ██  ██      ██   ██ 
██   ██ █████       ██████  █████   ██    ██ █████   █████   ██████  
██   ██ ██          ██   ██ ██      ██    ██ ██  ██  ██      ██   ██ 
██████  ███████     ██████  ███████  ██████  ██   ██ ███████ ██   ██ 
                                                   [CREATE 1M TEST]
");
Console.ResetColor();

var alleBatches = new List<EntityCollection>();
for (int i = 0; i < (totaalRecords / batchGrootte); i++)
{
    var collection = new EntityCollection { EntityName = "account" };
    for (int j = 0; j < batchGrootte; j++)
    {
        collection.Entities.Add(new Entity("account") { ["name"] = $"DE-BEUKER-{i}-{j}" });
    }
    alleBatches.Add(collection);
}

Console.WriteLine($"🚀 RAKET GELADEN: {totaalRecords:N0} records met BYPASS.");
var stopwatch = Stopwatch.StartNew();
int verwerkt = 0;

using var serviceClient = new ServiceClient(connectionString);
var options = new ParallelOptions { MaxDegreeOfParallelism = maxThreads };

await Parallel.ForEachAsync(alleBatches, options, async (batch, token) => {
    bool success = false;
    while (!success)
    {
        try
        {
            var request = new CreateMultipleRequest 
            { 
                Targets = batch,
                ["BypassCustomPluginExecution"] = true // DE TURBO KNOP
            };
            
            await serviceClient.ExecuteAsync(request);

            Interlocked.Add(ref verwerkt, batchGrootte);
            if (verwerkt % 5000 == 0)
            {
                double speed = verwerkt / stopwatch.Elapsed.TotalSeconds;
                Console.Write($"\r🔥 BEUK-STATUS: {verwerkt:N0} / {totaalRecords:N0} records... ({speed:F0} rec/s)");
            }
            success = true;
        }
        catch (Exception ex)
        {
            var retryAfter = TimeSpan.FromSeconds(10);
            if (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
            {
                if (ex.InnerException is FaultException<OrganizationServiceFault> fault && fault.Detail.ErrorDetails.ContainsKey("Retry-After"))
                    retryAfter = (TimeSpan)fault.Detail.ErrorDetails["Retry-After"];
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[⏳] STRAFBANK! Wacht {retryAfter.TotalSeconds}s...");
                Console.ResetColor();
                await Task.Delay(retryAfter);
            }
            else
            {
                Console.WriteLine($"\n❌ ERROR op Thread {Environment.CurrentManagedThreadId}: {ex.Message}");
                break;
            }
        }
    }
});

stopwatch.Stop();
Console.WriteLine($"\n\n🏆 MISSIE VOLBRACHT in {stopwatch.Elapsed.TotalMinutes:F2} minuten.");

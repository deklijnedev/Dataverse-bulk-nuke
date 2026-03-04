using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Diagnostics;

// --- CONFIG ---
string connectionString = "AuthType=ClientSecret;Url=https:://xxx.crm4.dynamics.com;ClientId="";ClientSecret="";
string targetTable = "cr2f7_elastictest"; 
int totaalRecords = 1000000;
int batchGrootte = 1000; 
int maxThreads = 50; 

// --- DE ELASTIC BEUKER V4.3 ---
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
██████   ███████     ██████  ███████ ██    ██ ██   ██ ███████ ██████  
██   ██  ██          ██   ██ ██      ██    ██ ██  ██  ██      ██   ██ 
██   ██  █████       ██████  █████   ██    ██ █████   █████   ██████  
██   ██  ██          ██   ██ ██      ██    ██ ██  ██  ██      ██   ██ 
██████   ███████     ██████  ███████  ██████  ██   ██ ███████ ██  ██  
                    [ NO-HTTPCLIENT MODE ]
                    [   THREADS: 50      ]
");
Console.ResetColor();

var alleBatches = new List<EntityCollection>();
Random rnd = new Random();

Console.WriteLine("📦 Batches laden...");
for (int i = 0; i < (totaalRecords / batchGrootte); i++)
{
    var collection = new EntityCollection { EntityName = targetTable };
    for (int j = 0; j < batchGrootte; j++)
    {
        var e = new Entity(targetTable);
        e["cr2f7_name"] = $"BEUK-{i}-{j}";
        e["partitionid"] = $"p-{rnd.Next(1, 21)}"; 
        collection.Entities.Add(e);
    }
    alleBatches.Add(collection);
}

using var serviceClient = new ServiceClient(connectionString);

Console.WriteLine($"🚀 RAKET KLAAR: {totaalRecords:N0} records.");
var stopwatch = Stopwatch.StartNew();
int verwerkt = 0;
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
                ["BypassCustomPluginExecution"] = true 
            };

            await serviceClient.ExecuteAsync(request);

            Interlocked.Add(ref verwerkt, batchGrootte);
            if (verwerkt % 5000 == 0)
            {
                double speed = verwerkt / stopwatch.Elapsed.TotalSeconds;
                Console.Write($"\r🔥 STATUS: {verwerkt:N0} / {totaalRecords:N0} records... ({speed:F0} rec/s)    ");
                Console.Out.Flush();
            }
            success = true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[!] LAG/RETRY: {ex.Message.Substring(0, Math.Min(ex.Message.Length, 50))}...");
            Console.ResetColor();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
});

stopwatch.Stop();
Console.WriteLine($"\n\n🏆 KLAAR IN {stopwatch.Elapsed.TotalMinutes:F2} MINUTEN!");

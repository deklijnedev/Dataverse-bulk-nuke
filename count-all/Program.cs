using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;

string connectionString = "AuthType=ClientSecret;Url=https://xxx.crm4.dynamics.com;ClientId="";ClientSecret="";
string entityName = args.Length > 0 ? args[0] : "account"; 

try 
{
    using var serviceClient = new ServiceClient(connectionString);
    if (!serviceClient.IsReady) { Console.WriteLine($"❌ Fout: {serviceClient.LastError}"); return; }

    Console.WriteLine($"🔍 Diepe scan op {entityName.ToUpper()} via Helsinki...");

    int totalCount = 0;
    int pageNumber = 1;
    string pagingCookie = null;

    while (true)
    {
        // We vragen alleen het ID op om data-overhead te minimaliseren
        QueryExpression query = new QueryExpression(entityName)
        {
            ColumnSet = new ColumnSet(entityName + "id"), 
            PageInfo = new PagingInfo
            {
                Count = 5000,
                PageNumber = pageNumber,
                PagingCookie = pagingCookie
            }
        };

        EntityCollection result = serviceClient.RetrieveMultiple(query);
        totalCount += result.Entities.Count;

        // Dit is je dashboard in de terminal
        Console.Write($"\r🛰️  Records geteld: {totalCount:N0}...");

        if (result.MoreRecords)
        {
            pageNumber++;
            pagingCookie = result.PagingCookie;
        }
        else
        {
            break;
        }
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\n\n✅ SCAN VOLTOOID!");
    Console.WriteLine($"📊 TOTAAL RECORDS: {totalCount:N0}");
    Console.ResetColor();
}
catch (Exception ex)
{
    Console.WriteLine($"\n💥 CRITICAL FAILURE: {ex.Message}");
}

Custom C# scripts to test the performance of dataverse.

Benchmarking Power Automate perfomance vs C#

---

# Comparison not fair

Yes I know it is not fair to compare the performance of Power Automate to a compiled C# binary, but trust me some functional consultants in the Power Platform world need convincing.

So whenever a functional Power Platform consultant wants to do big data operations with just Power Automate, use this repo to show him the better alternative.

The test was simple: I wanted to create 1 M records in my dataverse environment and wanted to benchmark the performance of both Power Automate and C#.

When I finished the test I came to the realisation that Power Automate is a usefull tool, but you need to combine it with the raw power of C# whenever tasks get too complex.

Unfortunately out in the wild a lot of horendous stuff is built with just Power Automate, my mission is simple: convince people their is a better alternative than to doing everything in low-code.

SO in the future when you are building something with low-code, for the love of god when it gets too complex implement custom high-code (like the code in this repo)

# Power Automate (cloud flows): 

- API requests needed: 1.000.000
- Time to complete: 25 days (with a default license on 1 serviceaccount) 

# Compiled C# binary

- API requests needed: 1000
- Time to complete: 40 minutes

The performance below is only at the start of the run and for the test and I used an VPS with 64 cores 
- Create script: creates +- 239 records a second
- Create-elastic: creates +- 600 records a second

Warnings:
- LLM's are used to generate the code, so it is far from perfect (I created all the code in just 10 minutes)
- If you take this code somewhere other than a test environment, please take some time to rewrite the code



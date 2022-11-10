# sfa-roatp-company-structure-explorer
Service to retrieve company data for RoATP companies (and parent companies)

1. Clone this repo
2. Publish db locally
3. Add app.config file to root of solution:

```
{
    "AppConfig": {
        "DatabaseConnectionString": <local db connection string>,
        "ApiUser": <companies house api key>,
        "LogFile": <path and file name of log file, eg. c:\temp\roatp-log.txt>
    }
}

```

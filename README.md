# IntegrationPlaybook-LogicAppErrorReport
This is a tool that will produce a report of errors from logic apps.  You can run it for a date range and it will generate a report with all of the errors across your logic apps and which action failed.


IntegrationPlaybook.LogicApps.ErrorReport.exe <tenantId> <subscriptionId> <clientId> <clientSecret> <excelOutputPath> <startDate> <endDate> <resourceGroups>

The below is an example powershell script that would run the report for the previous 7 days:

$dateFormat = 'yyyy-MM-ddTHH:mm:ss.fff'

$tenantId = 'TBC'
$subscriptionId = 'TBC'
$clientId = 'TBC'
$clientSecret = 'TBC'
$excelPath = 'c:\Temp\LogicAppReport.xlsx'
$startDate = [DateTime]::Today.Date.AddDays(-7).ToString($dateFormat)
$endDate = [DateTime]::Today.Date.AddDays(1).ToString($dateFormat)
$resourceGroup = 'TBC'
$workingDirectory = 'C:\ProgramData\chocolatey\bin'

$arguments = "$tenantId $subscriptionId $clientId $clientSecret $excelPath $startDate $endDate $resourceGroup"
Write-Host $arguments
Start-Process -Wait -FilePath 'IntegrationPlaybook.LogicApps.ErrorReport.exe' -WorkingDirectory $workingDirectory -ArgumentList $arguments



There is more info about it in the integration playbook:
https://www.integration-playbook.io/docs/en/logic-app-error-report-tool


<#
.SYNOPSIS
    Fetches Jira issue details (summary, status, description, etc.) from natus-jira.atlassian.net
    and prints them as Markdown.

.DESCRIPTION
    Wraps the Jira Cloud REST API v3 enhanced search endpoint (POST /rest/api/3/search/jql — the
    endpoint the old GET/POST /rest/api/3/search was retired in favor of in 2025). Supports three
    mutually exclusive lookup modes: explicit issue keys, a free-text search scoped to the project,
    or a raw JQL override. With none of those, it lists the project backlog using the same JQL/order
    as the project's default board list view.

.PARAMETER Keys
    One or more issue keys, e.g. ASWD-101, ASWD-102.

.PARAMETER Search
    Free-text search term(s), scoped to -ProjectKey, matched against summary/description.

.PARAMETER Jql
    A raw JQL query, used as-is instead of building one from -Keys/-Search.

.PARAMETER ProjectKey
    Project key to scope -Search and the default backlog listing to. Defaults to $env:JIRA_PROJECT_KEY
    or 'ASWD'.

.PARAMETER MaxResults
    Maximum number of issues to return across all pages. Defaults to 25.

.EXAMPLE
    ./fetch-jira-issue.ps1 -Keys ASWD-101,ASWD-102

.EXAMPLE
    ./fetch-jira-issue.ps1 -Search "add patient"

.EXAMPLE
    ./fetch-jira-issue.ps1 -Jql "project = ASWD AND issuetype = Epic"

.EXAMPLE
    ./fetch-jira-issue.ps1
    Lists the ASWD backlog, ordered the same way as the project's default board list view.
#>
[CmdletBinding(DefaultParameterSetName = 'ListAll')]
param(
    [Parameter(ParameterSetName = 'ByKeys')]
    [string[]] $Keys,

    [Parameter(ParameterSetName = 'BySearch')]
    [string] $Search,

    [Parameter(ParameterSetName = 'ByJql')]
    [string] $Jql,

    [string] $ProjectKey = $(if ($env:JIRA_PROJECT_KEY) { $env:JIRA_PROJECT_KEY } else { 'ASWD' }),

    [int] $MaxResults = 25
)

$ErrorActionPreference = 'Stop'

function Import-LocalEnvFile {
    $envFile = Join-Path $PSScriptRoot '..\.env.local'
    if (-not (Test-Path $envFile)) {
        return
    }

    foreach ($line in Get-Content $envFile) {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) {
            continue
        }

        $parts = $trimmed.Split('=', 2)
        if ($parts.Count -ne 2) {
            continue
        }

        $name = $parts[0].Trim()
        $value = $parts[1].Trim()
        if (-not (Test-Path "env:$name")) {
            Set-Item -Path "env:$name" -Value $value
        }
    }
}

function Get-JiraSettings {
    Import-LocalEnvFile

    $baseUrl = if ($env:JIRA_BASE_URL) { $env:JIRA_BASE_URL.TrimEnd('/') } else { 'https://natus-jira.atlassian.net' }
    $email = $env:JIRA_EMAIL
    $token = $env:JIRA_API_TOKEN

    if (-not $email -or -not $token) {
        throw "JIRA_EMAIL and/or JIRA_API_TOKEN are not set. Set them as environment variables, or " +
              "create '$PSScriptRoot\..\.env.local' with JIRA_EMAIL=... and JIRA_API_TOKEN=... " +
              "(see SKILL.md for how to generate a token)."
    }

    $pair = [System.Text.Encoding]::UTF8.GetBytes("$($email):$($token)")
    $authHeader = 'Basic ' + [Convert]::ToBase64String($pair)

    return @{
        BaseUrl = $baseUrl
        Headers = @{
            Authorization = $authHeader
            Accept        = 'application/json'
        }
    }
}

function Invoke-JiraApi {
    param(
        [Parameter(Mandatory)] [hashtable] $Settings,
        [Parameter(Mandatory)] [string] $Path,
        [hashtable] $Body
    )

    $uri = "$($Settings.BaseUrl)$Path"

    try {
        if ($Body) {
            $json = $Body | ConvertTo-Json -Depth 10
            return Invoke-RestMethod -Uri $uri -Method Post -Headers $Settings.Headers -Body $json -ContentType 'application/json'
        }

        return Invoke-RestMethod -Uri $uri -Method Get -Headers $Settings.Headers
    }
    catch {
        $response = $_.Exception.Response
        if (-not $response) {
            throw
        }

        $statusCode = [int]$response.StatusCode
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $bodyText = $reader.ReadToEnd()

        if ($statusCode -eq 401) {
            throw "Jira authentication failed (401). Check JIRA_EMAIL and JIRA_API_TOKEN. Raw response: $bodyText"
        }
        if ($statusCode -eq 404) {
            throw "Jira returned 404 Not Found for '$Path'. Check the issue key(s) and JIRA_BASE_URL. Raw response: $bodyText"
        }

        throw "Jira API error ($statusCode) calling '$Path': $bodyText"
    }
}

function ConvertFrom-JiraHtml {
    param([string] $Html)

    if (-not $Html) {
        return '_(no description)_'
    }

    $text = $Html
    $text = $text -replace '(?i)<li[^>]*>', "`n- "
    $text = $text -replace '(?i)</(li|p|div|h[1-6])>', "`n"
    $text = $text -replace '(?i)<br\s*/?>', "`n"
    $text = $text -replace '<[^>]+>', ''
    $text = [System.Net.WebUtility]::HtmlDecode($text)
    $text = $text -replace "`r`n", "`n"
    $text = ($text -split "`n" | ForEach-Object { $_.Trim() }) -join "`n"
    $text = $text -replace "`n{3,}", "`n`n"

    return $text.Trim()
}

function Format-JiraIssue {
    param([Parameter(Mandatory)] $Issue)

    $fields = $Issue.fields
    $type = $fields.issuetype.name
    $status = $fields.status.name
    $summary = $fields.summary
    $priority = if ($fields.priority) { $fields.priority.name } else { 'None' }
    $assignee = if ($fields.assignee) { $fields.assignee.displayName } else { 'Unassigned' }

    $lines = @()
    $lines += "## $($Issue.key) - $type`: $summary"
    $lines += "- Status: $status"
    $lines += "- Priority: $priority"
    $lines += "- Assignee: $assignee"

    if ($fields.parent) {
        $parentSummary = $fields.parent.fields.summary
        $lines += "- Parent: $($fields.parent.key) - $parentSummary"
    }

    if ($fields.labels -and $fields.labels.Count -gt 0) {
        $lines += "- Labels: $($fields.labels -join ', ')"
    }

    $lines += ''
    $lines += '### Description'

    $descriptionHtml = $null
    if ($Issue.renderedFields -and $Issue.renderedFields.description) {
        $descriptionHtml = $Issue.renderedFields.description
    }

    $lines += (ConvertFrom-JiraHtml $descriptionHtml)

    return ($lines -join "`n")
}

function Get-EscapedJqlString {
    param([string] $Value)

    return $Value -replace '\\', '\\\\' -replace '"', '\"'
}

$settings = Get-JiraSettings

switch ($PSCmdlet.ParameterSetName) {
    'ByKeys' {
        $quotedKeys = ($Keys | ForEach-Object { '"' + (Get-EscapedJqlString $_) + '"' }) -join ','
        $effectiveJql = "key in ($quotedKeys)"
    }
    'BySearch' {
        $escaped = Get-EscapedJqlString $Search
        $effectiveJql = "project = $ProjectKey AND text ~ `"$escaped`""
    }
    'ByJql' {
        $effectiveJql = $Jql
    }
    default {
        $effectiveJql = "project = $ProjectKey ORDER BY cf[10019] ASC"
    }
}

$fieldsToRequest = @('summary', 'description', 'issuetype', 'status', 'priority', 'assignee', 'parent', 'labels')
$issues = New-Object System.Collections.Generic.List[object]
$nextPageToken = $null

do {
    $body = @{
        jql        = $effectiveJql
        fields     = $fieldsToRequest
        maxResults = [Math]::Min(50, $MaxResults - $issues.Count)
        expand     = 'renderedFields'
    }
    if ($nextPageToken) {
        $body.nextPageToken = $nextPageToken
    }

    $page = Invoke-JiraApi -Settings $settings -Path '/rest/api/3/search/jql' -Body $body
    foreach ($issue in $page.issues) {
        $issues.Add($issue)
    }

    $nextPageToken = $page.nextPageToken
} while ($nextPageToken -and $issues.Count -lt $MaxResults)

if ($issues.Count -eq 0) {
    Write-Output "No issues found for JQL: $effectiveJql"
    exit 0
}

$output = ($issues | ForEach-Object { Format-JiraIssue $_ }) -join "`n`n---`n`n"
Write-Output $output

if ($nextPageToken) {
    Write-Output "`n_(more results exist beyond MaxResults=$MaxResults; narrow the query or increase -MaxResults)_"
}

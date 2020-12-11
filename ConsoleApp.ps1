Import-Module PowershellForXti -Force

$script:consoleAppConfig = [PSCustomObject]@{
    RepoOwner = "JasonBenfield"
    RepoName = "XTI_ConsoleApp"
    AppName = "XTI_ConsoleApp"
    AppType = "Package"
    ProjectDir = ""
}

function ConsoleApp-New-XtiIssue {
    param(
        [Parameter(Mandatory, Position=0)]
        [string] $IssueTitle,
        $Labels = @(),
        [string] $Body = "",
        [switch] $Start
    )
    $script:consoleAppConfig | New-XtiIssue @PsBoundParameters
}

function ConsoleApp-Xti-StartIssue {
    param(
        [Parameter(Position=0)]
        [long]$IssueNumber = 0,
        $IssueBranchTitle = "",
        $AssignTo = ""
    )
    $script:consoleAppConfig | Xti-StartIssue @PsBoundParameters
}

function ConsoleApp-New-XtiVersion {
    param(
        [Parameter(Position=0)]
        [ValidateSet("major", "minor", "patch")]
        $VersionType = "minor",
        [ValidateSet("Development", "Production", "Staging", "Test")]
        $EnvName = "Production"
    )
    $script:consoleAppConfig | New-XtiVersion @PsBoundParameters
}

function ConsoleApp-New-XtiPullRequest {
    param(
        [Parameter(Position=0)]
        [string] $CommitMessage
    )
    $script:consoleAppConfig | New-XtiPullRequest @PsBoundParameters
}

function ConsoleApp-Xti-PostMerge {
    param(
    )
    $script:consoleAppConfig | Xti-PostMerge @PsBoundParameters
}

function ConsoleApp-Publish {
    param(
        [switch] $Prod
    )
    $script:consoleAppConfig | Xti-PublishPackage @PsBoundParameters
}

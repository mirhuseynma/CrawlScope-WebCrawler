$dirs = @(
    "C:\Users\ThinkPad\Desktop\Projects\FinalProjects\CrawlScope\backend\CrawlScope\src\Core\CrawlScope.Domain",
    "C:\Users\ThinkPad\Desktop\Projects\FinalProjects\CrawlScope\backend\CrawlScope\src\Core\CrawlScope.Application",
    "C:\Users\ThinkPad\Desktop\Projects\FinalProjects\CrawlScope\backend\CrawlScope\src\Infrastructure\CrawlScope.Infrastructure",
    "C:\Users\ThinkPad\Desktop\Projects\FinalProjects\CrawlScope\backend\CrawlScope\src\Infrastructure\CrawlScope.Persistence",
    "C:\Users\ThinkPad\Desktop\Projects\FinalProjects\CrawlScope\backend\CrawlScope\src\Presentation\CrawlScope.Api"
)

foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) { continue }
    
    $globalUsingsFile = Join-Path $dir "GlobalUsings.cs"
    if (Test-Path (Join-Path $dir "GlobalUsing.cs")) {
        $globalUsingsFile = Join-Path $dir "GlobalUsing.cs"
    }
    
    # Common namespaces we want to make global per layer
    $commonUsings = @(
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Threading",
        "System.Threading.Tasks"
    )
    
    # Collect all usings
    $files = Get-ChildItem -Path $dir -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch "obj\\|bin\\|GlobalUsing" }
    
    $allUsings = @{}
    foreach ($file in $files) {
        $content = Get-Content $file.FullName
        foreach ($line in $content) {
            if ($line -match "^\s*using\s+([a-zA-Z0-9_\.]+)\s*;\s*$") {
                $ns = $matches[1]
                if (-not $allUsings.ContainsKey($ns)) {
                    $allUsings[$ns] = 0
                }
                $allUsings[$ns]++
            }
        }
    }
    
    # Pick usings that appear in more than 2 files or are common
    $usingsToGlobalize = @()
    foreach ($key in $allUsings.Keys) {
        if ($allUsings[$key] -ge 3 -or $commonUsings -contains $key) {
            $usingsToGlobalize += $key
        }
    }
    
    # Create or update GlobalUsings.cs
    $globalContent = @()
    foreach ($ns in ($usingsToGlobalize | Sort-Object)) {
        $globalContent += "global using $ns;"
    }
    
    Set-Content -Path $globalUsingsFile -Value ($globalContent -join "`r`n") -Encoding UTF8
    
    # Remove these usings from all files
    foreach ($file in $files) {
        $content = Get-Content $file.FullName
        $newContent = @()
        $modified = $false
        foreach ($line in $content) {
            if ($line -match "^\s*using\s+([a-zA-Z0-9_\.]+)\s*;\s*$") {
                $ns = $matches[1]
                if ($usingsToGlobalize -contains $ns) {
                    $modified = $true
                    continue
                }
            }
            $newContent += $line
        }
        
        if ($modified) {
            # Trim leading empty lines
            while ($newContent.Count -gt 0 -and [string]::IsNullOrWhiteSpace($newContent[0])) {
                $newContent = $newContent | Select-Object -Skip 1
            }
            Set-Content -Path $file.FullName -Value ($newContent -join "`r`n") -Encoding UTF8
        }
    }
}

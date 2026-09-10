param(
    [string]$Model = "gpt-4o-mini-tts",
    [string]$Voice = "cedar",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$audioFolder = Join-Path $projectRoot "Assets\Branding\ProfessorGuide\Audio"
$apiKey = [Environment]::GetEnvironmentVariable("OPENAI_API_KEY")

$voiceInstruction = @"
Speak Malay as a warm male science professor for a Form 1 digital science lab game.
Use a calm, confident, friendly teacher tone. Keep pronunciation clear and natural.
Do not sound like a cartoon. Use moderate speed, gentle energy, and short natural pauses.
"@

$lines = @(
    @{
        File = "voice_welcome_ms.wav"
        Text = "Selamat datang ke Makmal Sains Digital."
        Context = "First launch welcome"
    },
    @{
        File = "voice_signedout_ms.wav"
        Text = "Log masuk dahulu supaya kemajuan eksperimen anda dapat disimpan."
        Context = "Signed out prompt"
    },
    @{
        File = "voice_signedin_ms.wav"
        Text = "Selamat kembali. Mari sambung penerokaan sains anda."
        Context = "Signed in welcome"
    },
    @{
        File = "voice_start_ms.wav"
        Text = "Tekan Mulakan Amali untuk memilih eksperimen sains anda."
        Context = "Start experiments hover"
    },
    @{
        File = "voice_settings_ms.wav"
        Text = "Laraskan bunyi, paparan, dan pengalaman pembelajaran di sini."
        Context = "Settings hover"
    },
    @{
        File = "voice_achievements_ms.wav"
        Text = "Di sini anda boleh melihat pencapaian dan kemajuan setiap amali."
        Context = "Achievements hover"
    }
)

Write-Host "Professor Guide OpenAI voice generation"
Write-Host "Project: $projectRoot"
Write-Host "Output:  $audioFolder"
Write-Host "Model:   $Model"
Write-Host "Voice:   $Voice"

if ($DryRun) {
    Write-Host ""
    Write-Host "Dry run only. Clips that would be generated:"
    foreach ($line in $lines) {
        Write-Host ("- {0}: {1}" -f $line.File, $line.Text)
    }
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        Write-Host ""
        Write-Host "OPENAI_API_KEY is not currently set."
    }
    exit 0
}

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    throw "OPENAI_API_KEY is not set. Set it in your environment before running this script."
}

if (-not (Test-Path -LiteralPath $audioFolder)) {
    New-Item -ItemType Directory -Force -Path $audioFolder | Out-Null
}

$headers = @{
    "Authorization" = "Bearer $apiKey"
    "Content-Type" = "application/json"
}

foreach ($line in $lines) {
    $outputPath = Join-Path $audioFolder $line.File
    $backupPath = "$outputPath.bak"

    if ((Test-Path -LiteralPath $outputPath) -and -not (Test-Path -LiteralPath $backupPath)) {
        Copy-Item -LiteralPath $outputPath -Destination $backupPath
    }

    $body = @{
        model = $Model
        voice = $Voice
        input = $line.Text
        instructions = $voiceInstruction
        response_format = "wav"
    } | ConvertTo-Json -Depth 5

    Write-Host ("Generating {0} ({1})..." -f $line.File, $line.Context)

    Invoke-WebRequest `
        -Uri "https://api.openai.com/v1/audio/speech" `
        -Method Post `
        -Headers $headers `
        -Body $body `
        -OutFile $outputPath `
        -UseBasicParsing

    $item = Get-Item -LiteralPath $outputPath
    if ($item.Length -lt 16000) {
        throw "Generated file is suspiciously small: $outputPath ($($item.Length) bytes)"
    }
}

Write-Host ""
Write-Host "Done. Generated professor voice WAV files:"
Get-ChildItem -LiteralPath $audioFolder -Filter "voice_*_ms.wav" |
    Select-Object Name, Length, LastWriteTime |
    Format-Table -AutoSize

Write-Host ""
Write-Host "Next: return to Unity and let the assets reimport, then run the professor guide setup if needed."

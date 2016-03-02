# Start of Build Script
$start = [DateTime]::Now
.\psake.ps1 -version "1.5.0.2"
$end = [DateTime]::Now
$diff = $end - $start
"Time Elapsed: $diff"


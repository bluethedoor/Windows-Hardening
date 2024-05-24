Function Set-LogSize {
    <#
    .SYNOPSIS
        Increase Code Integrity Operational Event Logs size from the default 1MB to user defined size
        Also automatically increases the log size by 1MB if the current free space is less than 1MB and the current maximum log size is less than or equal to 10MB.
        This is to prevent infinitely expanding the max log size automatically.
    .PARAMETER LogSize
        Size of the Code Integrity Operational Event Log
    .INPUTS
        System.Int64
    .OUTPUTS
        System.Void
    #>
    [CmdletBinding()]
    [OutputType([System.Void])]
    param (
        [parameter(Mandatory = $false)][System.UInt64]$LogSize
    )
    Begin {
        Write-Verbose -Message 'Set-LogSize function started...'

        [System.String]$LogName = 'Microsoft-Windows-CodeIntegrity/Operational'
        [System.Diagnostics.Eventing.Reader.EventLogConfiguration]$Log = New-Object -TypeName System.Diagnostics.Eventing.Reader.EventLogConfiguration -ArgumentList $LogName
        [System.IO.FileInfo]$LogFilePath = [System.Environment]::ExpandEnvironmentVariables($Log.LogFilePath)
        [System.Double]$CurrentLogFileSize = $LogFilePath.Length
        [System.Double]$CurrentLogMaxSize = $Log.MaximumSizeInBytes
    }
    Process {
        if (-NOT $LogSize) {
            if (($CurrentLogMaxSize - $CurrentLogFileSize) -lt 1MB) {
                if ($CurrentLogMaxSize -le 10MB) {
                    Write-Verbose -Message "Increasing the Code Integrity log size by 1MB because its current free space ($(($CurrentLogMaxSize - $CurrentLogFileSize) / 1MB)) is less than 1MB"
                    $Log.MaximumSizeInBytes = $CurrentLogMaxSize + 1MB
                    $Log.IsEnabled = $true
                    $Log.SaveChanges()
                }
            }
        }
        else {
            Write-Verbose -Message "Setting Code Integrity log size to $LogSize"
            $Log.MaximumSizeInBytes = $LogSize
            $Log.IsEnabled = $true
            $Log.SaveChanges()
        }
    }
}
Export-ModuleMember -Function 'Set-LogSize'

# SIG # Begin signature block
# MIILkgYJKoZIhvcNAQcCoIILgzCCC38CAQExDzANBglghkgBZQMEAgEFADB5Bgor
# BgEEAYI3AgEEoGswaTA0BgorBgEEAYI3AgEeMCYCAwEAAAQQH8w7YFlLCE63JNLG
# KX7zUQIBAAIBAAIBAAIBAAIBADAxMA0GCWCGSAFlAwQCAQUABCCuTnFeCbqlhOQk
# i/DSYn2LuTZd+ob1NH3w8cj5DJeHcaCCB9AwggfMMIIFtKADAgECAhMeAAAABI80
# LDQz/68TAAAAAAAEMA0GCSqGSIb3DQEBDQUAME8xEzARBgoJkiaJk/IsZAEZFgNj
# b20xIjAgBgoJkiaJk/IsZAEZFhJIT1RDQUtFWC1DQS1Eb21haW4xFDASBgNVBAMT
# C0hPVENBS0VYLUNBMCAXDTIzMTIyNzExMjkyOVoYDzIyMDgxMTEyMTEyOTI5WjB5
# MQswCQYDVQQGEwJVSzEeMBwGA1UEAxMVSG90Q2FrZVggQ29kZSBTaWduaW5nMSMw
# IQYJKoZIhvcNAQkBFhRob3RjYWtleEBvdXRsb29rLmNvbTElMCMGCSqGSIb3DQEJ
# ARYWU3B5bmV0Z2lybEBvdXRsb29rLmNvbTCCAiIwDQYJKoZIhvcNAQEBBQADggIP
# ADCCAgoCggIBAKb1BJzTrpu1ERiwr7ivp0UuJ1GmNmmZ65eckLpGSF+2r22+7Tgm
# pEifj9NhPw0X60F9HhdSM+2XeuikmaNMvq8XRDUFoenv9P1ZU1wli5WTKHJ5ayDW
# k2NP22G9IPRnIpizkHkQnCwctx0AFJx1qvvd+EFlG6ihM0fKGG+DwMaFqsKCGh+M
# rb1bKKtY7UEnEVAsVi7KYGkkH+ukhyFUAdUbh/3ZjO0xWPYpkf/1ldvGes6pjK6P
# US2PHbe6ukiupqYYG3I5Ad0e20uQfZbz9vMSTiwslLhmsST0XAesEvi+SJYz2xAQ
# x2O4n/PxMRxZ3m5Q0WQxLTGFGjB2Bl+B+QPBzbpwb9JC77zgA8J2ncP2biEguSRJ
# e56Ezx6YpSoRv4d1jS3tpRL+ZFm8yv6We+hodE++0tLsfpUq42Guy3MrGQ2kTIRo
# 7TGLOLpayR8tYmnF0XEHaBiVl7u/Szr7kmOe/CfRG8IZl6UX+/66OqZeyJ12Q3m2
# fe7ZWnpWT5sVp2sJmiuGb3atFXBWKcwNumNuy4JecjQE+7NF8rfIv94NxbBV/WSM
# pKf6Yv9OgzkjY1nRdIS1FBHa88RR55+7Ikh4FIGPBTAibiCEJMc79+b8cdsQGOo4
# ymgbKjGeoRNjtegZ7XE/3TUywBBFMf8NfcjF8REs/HIl7u2RHwRaUTJdAgMBAAGj
# ggJzMIICbzA8BgkrBgEEAYI3FQcELzAtBiUrBgEEAYI3FQiG7sUghM++I4HxhQSF
# hqV1htyhDXuG5sF2wOlDAgFkAgEIMBMGA1UdJQQMMAoGCCsGAQUFBwMDMA4GA1Ud
# DwEB/wQEAwIHgDAMBgNVHRMBAf8EAjAAMBsGCSsGAQQBgjcVCgQOMAwwCgYIKwYB
# BQUHAwMwHQYDVR0OBBYEFOlnnQDHNUpYoPqECFP6JAqGDFM6MB8GA1UdIwQYMBaA
# FICT0Mhz5MfqMIi7Xax90DRKYJLSMIHUBgNVHR8EgcwwgckwgcaggcOggcCGgb1s
# ZGFwOi8vL0NOPUhPVENBS0VYLUNBLENOPUhvdENha2VYLENOPUNEUCxDTj1QdWJs
# aWMlMjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1Db25maWd1cmF0aW9u
# LERDPU5vbkV4aXN0ZW50RG9tYWluLERDPWNvbT9jZXJ0aWZpY2F0ZVJldm9jYXRp
# b25MaXN0P2Jhc2U/b2JqZWN0Q2xhc3M9Y1JMRGlzdHJpYnV0aW9uUG9pbnQwgccG
# CCsGAQUFBwEBBIG6MIG3MIG0BggrBgEFBQcwAoaBp2xkYXA6Ly8vQ049SE9UQ0FL
# RVgtQ0EsQ049QUlBLENOPVB1YmxpYyUyMEtleSUyMFNlcnZpY2VzLENOPVNlcnZp
# Y2VzLENOPUNvbmZpZ3VyYXRpb24sREM9Tm9uRXhpc3RlbnREb21haW4sREM9Y29t
# P2NBQ2VydGlmaWNhdGU/YmFzZT9vYmplY3RDbGFzcz1jZXJ0aWZpY2F0aW9uQXV0
# aG9yaXR5MA0GCSqGSIb3DQEBDQUAA4ICAQA7JI76Ixy113wNjiJmJmPKfnn7brVI
# IyA3ZudXCheqWTYPyYnwzhCSzKJLejGNAsMlXwoYgXQBBmMiSI4Zv4UhTNc4Umqx
# pZSpqV+3FRFQHOG/X6NMHuFa2z7T2pdj+QJuH5TgPayKAJc+Kbg4C7edL6YoePRu
# HoEhoRffiabEP/yDtZWMa6WFqBsfgiLMlo7DfuhRJ0eRqvJ6+czOVU2bxvESMQVo
# bvFTNDlEcUzBM7QxbnsDyGpoJZTx6M3cUkEazuliPAw3IW1vJn8SR1jFBukKcjWn
# aau+/BE9w77GFz1RbIfH3hJ/CUA0wCavxWcbAHz1YoPTAz6EKjIc5PcHpDO+n8Fh
# t3ULwVjWPMoZzU589IXi+2Ol0IUWAdoQJr/Llhub3SNKZ3LlMUPNt+tXAs/vcUl0
# 7+Dp5FpUARE2gMYA/XxfU9T6Q3pX3/NRP/ojO9m0JrKv/KMc9sCGmV9sDygCOosU
# 5yGS4Ze/DJw6QR7xT9lMiWsfgL96Qcw4lfu1+5iLr0dnDFsGowGTKPGI0EvzK7H+
# DuFRg+Fyhn40dOUl8fVDqYHuZJRoWJxCsyobVkrX4rA6xUTswl7xYPYWz88WZDoY
# gI8AwuRkzJyUEA07IYtsbFCYrcUzIHME4uf8jsJhCmb0va1G2WrWuyasv3K/G8Nn
# f60MsDbDH1mLtzGCAxgwggMUAgEBMGYwTzETMBEGCgmSJomT8ixkARkWA2NvbTEi
# MCAGCgmSJomT8ixkARkWEkhPVENBS0VYLUNBLURvbWFpbjEUMBIGA1UEAxMLSE9U
# Q0FLRVgtQ0ECEx4AAAAEjzQsNDP/rxMAAAAAAAQwDQYJYIZIAWUDBAIBBQCggYQw
# GAYKKwYBBAGCNwIBDDEKMAigAoAAoQKAADAZBgkqhkiG9w0BCQMxDAYKKwYBBAGC
# NwIBBDAcBgorBgEEAYI3AgELMQ4wDAYKKwYBBAGCNwIBFTAvBgkqhkiG9w0BCQQx
# IgQgvhHgRABVmYV9tw0D9Aai0dv5tFWVBqYeFXk4be+cl/gwDQYJKoZIhvcNAQEB
# BQAEggIAKJGKYbANPeURgkSHNes2SuLhFLMuENeGj91G2CxW7hpGaEfLbi57COii
# qlosalIxNp1EvinSrr9Hhj5v4rUhK9bdimDD3/cfwMP7x4xzekD3yZ4kgTZdSXJB
# NDsqa+vBXdbAcCIq5/85BHrrrJ0Orb6odxU9GogMu5zTVS5C+VpIf2yLzIWNPvqi
# pMu1QpI+pMPv7SKWEQgowstypt64IR8aXPSnfjEkeeivnRBoSwH7Ep7tbDJiPuRe
# 5SREGXtnueLG23OrW6PpzLPSlLU3m4eWmszZs1jnVwynbQY0A9dvWKpygewPDG7p
# WPA0ebu4U39HoaicMl6RVo6gwSUhtiII4+82c1nBQ52bHV7aMHn5RWG+vGTgl62N
# fUZOgvkFGqEMG+TWAK0PWSMDY5sylJEbiEv40XOWCOwwafyBoPinC/XCC3cz3mqt
# d6tCG4fuE34T/VugGsWJUd5vGvH/VrdjZZ0Te9swR/KuKdY+6P3dsO3FEEn4QNDY
# isKLSmbFm+xyF4ZJpki7YvaOtklsxQuuvfG6TG0oAxPnhWVFj5UINw+vLDX3QGwV
# Po0FxMDlayZCRUf6TE+F7Ry9QTusFXisLE3gX2BaZhPaXztxbzpvZs32cx91yVT0
# Lk3RKoF1hXEDl9S75ING+TBXD6ZH1HqQs24TeDMDeq435s/F+j4=
# SIG # End signature block

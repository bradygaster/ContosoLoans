for ($i = 0; $i -lt 1; $i++) {
    $postParams = @{customerId=((New-Guid).ToString());loanAmount=(Get-Random -Minimum 1000 -Maximum 20000)}
    Invoke-WebRequest -Uri http://localhost:5002/loans -Method POST -Body ($postParams|ConvertTo-Json) -ContentType "application/json"
}
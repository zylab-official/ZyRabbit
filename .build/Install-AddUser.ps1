. "$PSScriptRoot\Util-RabbitMqPath.ps1"
write-host 'install:Add RabbitMq User "ZyRabbit"' -ForegroundColor Green

$rabbitMqPath = Get-RabbitMQPath
$rabbitmqctl = "'$rabbitMqPath\sbin\rabbitmqctl.bat'"

Write-Host "Found Comand Line Tool at $rabbitmqctl" -ForegroundColor Green

$createUser = "cmd.exe /C $rabbitmqctl add_user ZyRabbit ZyRabbit"
$setPermission = "cmd.exe /C $rabbitmqctl set_permissions -p / ZyRabbit `".*`" `".*`" `".*`""

Invoke-Expression -Command:$createUser
Invoke-Expression -Command:$setPermission

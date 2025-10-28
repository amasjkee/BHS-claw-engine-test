#!/bin/bash
# Скрипт для запуска консольного режима

# Ищем dotnet в стандартных местах
DOTNET_PATH=""

# Проверяем стандартные пути
if command -v dotnet &> /dev/null; then
    DOTNET_PATH="dotnet"
elif [ -f "$HOME/.dotnet/dotnet" ]; then
    DOTNET_PATH="$HOME/.dotnet/dotnet"
elif [ -f "/usr/bin/dotnet" ]; then
    DOTNET_PATH="/usr/bin/dotnet"
elif [ -f "/usr/local/bin/dotnet" ]; then
    DOTNET_PATH="/usr/local/bin/dotnet"
else
    echo ".NET не найден! Установите .NET 8.0 SDK"
    echo "Скачать: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "Запуск консольного режима"
echo "Используется: $DOTNET_PATH"
echo ""

$DOTNET_PATH run

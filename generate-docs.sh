#!/bin/bash

# Скрипт для генерации Doxygen документации

echo "Генерация Doxygen документации для BHS Claw Engine Test..."

# Проверяем наличие doxygen
if ! command -v doxygen &> /dev/null; then
    echo "Ошибка: Doxygen не установлен"
    echo "Установите doxygen: sudo pacman -S doxygen"
    exit 1
fi

# Генерируем документацию
echo "Запуск Doxygen..."
doxygen Doxyfile

if [ $? -eq 0 ]; then
    echo "Документация успешно сгенерирована"
    echo "Откройте html/index.html в браузере для просмотра"
else
    echo "Ошибка при генерации документации"
    exit 1
fi

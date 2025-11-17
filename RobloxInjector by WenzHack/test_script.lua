-- Базовый тестовый скрипт для проверки инжектора
-- Этот скрипт проверяет работу инжектора и выводит информацию в консоль

print("=========================================")
print("NEVERWENZ Injector - Тестовый скрипт")
print("=========================================")

-- Проверка основных функций
local function testBasicFunctions()
    print("[TEST] Проверка базовых функций...")
    
    -- Проверка game
    if game then
        print("[✓] game доступен")
    else
        print("[✗] game недоступен")
        return false
    end
    
    -- Проверка workspace
    if workspace then
        print("[✓] workspace доступен")
    else
        print("[✗] workspace недоступен")
        return false
    end
    
    -- Проверка Players
    if game.Players then
        print("[✓] Players доступен")
        local LocalPlayer = game.Players.LocalPlayer
        if LocalPlayer then
            print("[✓] LocalPlayer найден: " .. tostring(LocalPlayer.Name))
        end
    else
        print("[✗] Players недоступен")
    end
    
    return true
end

-- Проверка UI функций
local function testUIFunctions()
    print("[TEST] Проверка UI функций...")
    
    local success, result = pcall(function()
        local StarterGui = game:GetService("StarterGui")
        if StarterGui then
            StarterGui:SetCore("SendNotification", {
                Title = "NEVERWENZ Injector",
                Text = "Тестовый скрипт успешно выполнен!",
                Duration = 5
            })
            print("[✓] Уведомление отправлено")
            return true
        end
        return false
    end)
    
    if success and result then
        print("[✓] UI функции работают")
    else
        print("[✗] Ошибка при работе с UI: " .. tostring(result))
    end
end

-- Проверка выполнения скрипта
local function runTests()
    print("\n[INFO] Запуск тестов...\n")
    
    local basicTest = testBasicFunctions()
    if basicTest then
        testUIFunctions()
    end
    
    print("\n=========================================")
    print("Тестирование завершено")
    print("=========================================\n")
end

-- Запуск тестов
runTests()

-- Дополнительная информация
print("[INFO] Версия Roblox: " .. tostring(game:GetService("HttpService"):GetAsync("https://clientsettingscdn.roblox.com/v2/settings/application/PCDesktopClient")))
print("[INFO] Время выполнения: " .. os.date("%X"))


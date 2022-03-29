# Глобальные настройки ZFramework

Глобальные настройки, это ряд переменных лежащих GStore, которые меняют базовые возможности движка, например выключают загрузку UI по умолчанию.
Чтобы изменить эту переменную достаточно вызвать:
```c# 
App.GStore.Set<bool>("NoSave", true);
```

Лучшим местом вызвать изменение настроек является подпись на хук <b>OnBeforeStart</b>. Когда происходит вся переконфигурация системы и модулей после загрузки всех данных.

Обычно все подобные переменные описаны в файлах, в названии которых присутствует слово Store.

## Список настроек (GameConfigStore.cs)
- ```c# public static bool IsDevelopmentVersion;```<br>Активирует некоторые возможности, которые не должны попадать в релизную версию (обычно это всякие чит панели и прочее)
- ```c# public static bool NoSceneUnload;```<br>
- ```c# public static bool NoUI = false;```<br>Отключает базовый UI в целом
- ```c# public static bool NoMainMenuUI = false;```<br>Отключает главное меню
- ```c# public static bool NoSave = false;```<br>Отключает сохранение прогресса

## Настройки некоторых базовых модулей

### Модуль AudioSystem (AudioStore.cs)
- ```c# public static bool Music = true;```<br>Отключает музыку
- ```c# public static bool Sounds = true;```<br>Отключает звук

### Модуль MultiLanguage (LanguageStore.cs)
- ```c# public static string Locale = Translater.Locale.RU.ToString();```<br>Назначает язык по умолчанию

### Модуль Logs (LogsStore.cs)
- ```c# public static int LogLevel = 0;```<br>Назначает уровень доступных логов
0 - DEBUG,<br>
1 - WARNING,<br>
2 - ERROR,<br>
3 - DISABLED<br>
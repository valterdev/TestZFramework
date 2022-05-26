using System;
using System.Collections.Generic;

namespace ZFramework.Editor
{
    /// <summary>
    /// Класс описывающий мета данные модулей ZFramework
    /// </summary>
    [Serializable]
    public class ModuleInfo
    {
        /// <summary>
        /// данные хеш код генерируется автоматически и использует название модуля, но при этом у модулей с одинаковым название он будет генерировать разный хеш код.
        /// Хеш код задается 1 раз при создании модуля.
        /// </summary>
        public string hash;
        public string category;
        public string name;

        /// <summary>
        /// Название менеджера модуля, которое будет использовано при генерации файла Z0_PreInit.cs
        /// </summary>
        public string custom_manager_name;

        public string version;
        public string author;
        public string site;
        public string email;

        /// <summary>
        /// Тип модуля. От него зависит в какой папке будет лежать модуль (System или Domain).
        /// Соотвественно все системный модули имеют тип = 0. А все модули непосредственно для игры тип = 1.
        /// </summary>
        public int type;
        public int state;
        public string description;

        public bool preinit;
        /// <summary>
        /// Скрытое свойство, которое задаются вручную в json файле. Если оно true, это значит, что без этого модуля система работать не будет (либо будет, но такое не предполагается).
        /// </summary>
        public bool critical;

        /// <summary>
        /// зависимости от других модулей
        /// </summary>
        public List<string> dependencies;

        /// <summary>
        /// используемые в модуле независимые скрипты (помещаются в папку Modules/System/Utils)
        /// </summary>
        public List<string> utils;

        /// <summary>
        /// используемые в модуле плагины юнити (UPM | .unitypackage | Packages/manifest.json)
        /// </summary>
        public List<string> plugins;

        /// <summary>
        /// используемые в модуле файлы лежащие в папке контент, а не в папке с модулем
        /// </summary>
        public List<string> content_files;

        [NonSerialized]
        /// <summary>
        /// скрытое свойство, определяющее какие модули активны, а какие нет. Активные лежат в папке Assets, а не активные в папке ZFramework/DisabledModules/ в корневой папке проекта.
        /// </summary>
        public bool active;

        public string GetModulePath()
        {
            return "App/Modules/" + GetModuleCategoryByType(type) + "/" + name;
        }

        public static string GetModuleCategoryByType(int _value)
        {
            switch (_value)
            {
            case 0:
                return "System/Core";

            case 1:
                return "System/Data";

            case 2:
                return "System/Network";

            case 3:
                return "System/Other";

            case 10:
                return "Domain/Mechanics_Features";

            case 11:
                return "Domain/Meta";

            case 12:
                return "Domain/UI";

            case 13:
                return "Domain/Other";

            default: return string.Empty;
            }
        }
    }
}
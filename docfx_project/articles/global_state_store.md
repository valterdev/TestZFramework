# Глобальное хранилище данных

ZFramework хранит данные в Глобальном хранилище состояний. Это позволяет избавиться от связности между модулями, а так же безболезненно удалять модули, без изменения визуальных компонентов (view).

Принцип работы его следующий: <br>
Каждый модуль расширяет глобальный статический класс GSHelp (Для примера будем использовать модуль внутриигровых покупок InnApp)
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public static partial class GSHelp // InnAppStore
    {
        public static int IntVar;

        public class InnAppConf 
        {
            public static string Server;
            public static bool Active;
        }
    }
}
```

При инициализации фреймворк пробегает по всем полям и встроенным классам и собирает информацию о переменных и заносит их в Глобальное хранилище. Чтобы получить доступ к этим переменных нужно вызвать следующий метод:
```c#
App.GStore.Get<int>("IntVar")
```

Чтобы не держать в голове название всех переменных, нужно воспользовать самим классом GSHelp. Так как он статически и все поля в нем тоже, то мы увидим все доступные нам переменные:
```c#
App.GStore.Get<int>(GSHelp.IntVar)
```
Все что останется, это удалить GSHelp. и оставшийся IntVar взять в ковычки "IntVar".

Для того, чтобы добраться до переменных внутреннего класса нужно всего лишь сделать так:
```c#
App.GStore.Get<int>(GSHelp.InnAppConf.Server)
```
Затем удалить GSHelp. и взять в кавычки оставшееся, заменив точку на слеш:
```c#
App.GStore.Get<int>("InnAppConf/Server")
```

Чтобы изменить переменную, используется метод Set<T>:
```c#
App.GStore.Set<bool>("InnAppConf/Active", true);
```

К любой из этих переменных можно привязать визуальный компонент сделав его реактивным. Подробнее об этом читайте в реазделе "Реактивные компоненты"

## Атрибуты

Переменные в GSHelp можно расширять атрибутами и функциями.

Перменные глобального хранилища обладают следующими атрибутами:
1. \[GSStoreIgnore\] - данная перменная будет игнорироваться и не будет занесена в глобальное хранилище
2. \[GSSRegPreHelper("PreFunction")\] - прежде чем мы получим данную переменную при помощи GStore.Get, будет вызвана функция PreFunction

Подробнее расмотрим функции хелперы для переменных. По сути это аналог свойств языка c#. У нас есть перменная и при помощи атрибута \[GSSRegPreHelper("PreFunction")\], мы связываем эту переменную с функцией, имя которой указано в ковычках.

```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFramework
{
    public static partial class GSHelp // InnAppStore
    {
        [GSSRegPreHelper("PreFunction")]
        public static int IntVar;

        public class InnAppConf 
        {
            public static string Server;
            public static bool Active;
        }

        private void PreFunction() {
            // do something before get IntVar
        }
    }
}
```
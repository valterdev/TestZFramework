# CI/CD

Для автоматической сборки билдов для всей команды были настроены Unity Cloud Build и App Center.

Чтобы билд собрался нужно залить изменения в ветку dev (отправить мердж / пулл реквест из своих рабочих веток в основную dev ветку).

После чего в Unity Cloud Build будут собраны билды и отправлены веб-хуки нашему демону. Который сформирует необходимый запрос и создаст релиз билд в App Center. А App Center в свою очередь отправит в Slack в рабочий чат cv-builds с ссылками на билд, чтобы его могли скачать другие члены команды и поиграть/потестировать.

## Как это работает изнутри?

- Настраивается UCB на связь с нашим репозиторием в gitlab, чтобы автоматически собирал билды, когда заливаются изменения в определенную ветку.
- Как билд будет собран отправляется вебхук на наш демон с информацией об успешном билде
- Демон получив веб хук, формирует обратный запрос на получение shareId билда, вида:
https://build-api.cloud.unity3d.com/api/v1/orgs/${orgid}/projects/${projectid}/buildtargets/${buildtargetid}/builds/${buildNumber}/share
- В итоге получает ответ с телом:
``` json
{
    “shareid”: “id”,
    “shareExpiry”: “2022-01-26T07:41:16.745Z”
}
```
- Откуда берем shareid и получает итоговую ссылку на билд вида:
https://developer.cloud.unity3d.com/share/share.html?shareId=${shareid}
- Далее отправляем запрос в App Center:
api.appcenter.ms/v0.1/apps/${orgid}/${appid}}/releases
с телом:
``` json
{
    “build_version”: “0.1",
    “build_number”: “5",
    “external_download_url”: “https://developer.cloud.unity3d.com/share/share.html?shareId=${shareid}”
}
```
Если при запуске ItemChecker выходит ошибка что-то связанное с MSEdgeDriver или Browser, типо не подходит версия или что-то похожее.

Скорее Всего нужно обновить версию "msedgedriver.exe", для этого:
1. Открываешь ссылку "Microsoft Edge WebDriver"
2. Скачиваешь драйвер из "Канал Stable" -> "x64"
3. Заменяшь файл "msedgedriver.exe" в папке с программой.
4. Запускаешь ItemChecker.

Если продолжает ругатся на версию:
1. Открываешь браузер MS Edge
2. В адресной строке всталяешь "edge://settings/help"
3. Смотришь версию
4. Заходишь "msedgewebdriverstorage.z22.web.core.windows.net"
5. Находишь подходящию версию и скачиваешь x64.
6. Заменяшь файл "msedgedriver.exe" в папке с программой.

====================================================================================

Если при запуске ItemChecker выходит ошибка 403 Forbiden

1. Нажимаешь "ОК"
	1.1. Лучше немного подождать минут 2-3.
2. Перезапускаешь.
3. И так пока не запуститься.

====================================================================================

Если при запуске ItemChecker просит установить .NET 6.x.x

1. Открываешь ссылку ".NET 6 Desktop Runtime"
2. Скачиваешь установчик из ".NET Desktop Runtime 6.x.x" -> "Windows x64"
3. Перезапускаешь.

====================================================================================

Как пользоваться BaseUpdater.

1. Запускаешь файл "UpdateBase.exe"
2. Далее выбираешь что нужно добавить в списке, рекомендуется выбрать CheckAll
	2.1. При получение SteamId программа может долго думать это нормально, он получает Id 25 предметов каждый 5 минут.
3. После завершении в папке с программой создаеться готовая база файл "steamBase.json"
4. Копируешь файл в папку по пути "Документы -> ItemChecker"
5. В настройках ItemChecker ставишь галочку использовать локальную базу (Use local SteamBase).
6. Перезапускаешь ItemChecker.

************************************************************************************

If an error occurs when running ItemChecker, something related to MSEdgeDriver or Browser, the version is not suitable or something similar.

Most likely, you need to update the version of "msedgedriver.exe", for this:
1. Open the link "Microsoft Edge WebDriver"
2. Download the driver from "Stable Channel" -> "x64"
3. Replace the file "msedgedriver.exe" in the folder with the program.
4. Run ItemChecker.

If it continues to swear at the version:
1. Open the MS Edge browser
2. In the address bar you put "edge://settings/help"
3. You watch the version
4. Go to "msedgewebdriverstorage.z22.web.core.windows.net"
5. Find the right version and download x64.
6. Replace the file "msedgedriver.exe" in the folder with the program.

==================================================================================

If you get a 403 Forbiden error when running ItemChecker

1. Click "OK"
1.1. It is better to wait a little 2-3 minutes.
2. Restart.
3. And so on until it starts.

==================================================================================

If ItemChecker asks to install .NET 6.x.x when starting

1. Open the link ".NET 6 Desktop Runtime"
2. Download the installer from ".NET Desktop Runtime 6.x.x" -> "Windows x64"
3. Restart.

==================================================================================

How to use BaseUpdater.

1. Run the file "UpdateBase.exe"
2. Next, choose what you want to add to the list, it is recommended to select CheckAll
2.1. When receiving a SteamId, the program may think for a long time that this is normal, it receives an Id of 25 items every 5 minutes.
3. After completion, a ready-made base file "steamBase.json" is created in the folder with the program
4. Copy the file to a folder along the path "Documents -> ItemChecker"
5. In the ItemChecker settings, check the Use local SteamBase box.
6. Restart ItemChecker.
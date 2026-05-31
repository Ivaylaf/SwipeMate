SwipeMate - readMe.txt
======================

1. Съдържание
На този носител са приложени:
- PDF версия на документацията
- DOCX версия на документацията
- проектът SwipeMate
- този readMe.txt файл

2. Предназначение на проекта
SwipeMate е учебен дипломен проект. Приложението е подготвено основно за локална демонстрация и защита. В настоящия вариант не е публикувано в публична cloud среда.

3. Необходими условия за стартиране
- Windows компютър
- Visual Studio 2022
- .NET 9 SDK
- .NET MAUI workload
- PostgreSQL
- Windows Machine и/или Android Emulator

4. Настройка на базата данни
- Създава се PostgreSQL база данни с име swipemate
- В SwipeMate.Api/appsettings.json се задава правилният connection string
- При стартиране на API проекта миграциите и началните данни се прилагат автоматично

5. Стартиране на проекта
1. Отваря се решението SwipeMate.sln във Visual Studio 2022.
2. Задава се startup project на SwipeMate.Api.
3. Стартира се SwipeMate.Api.
4. След това се задава startup project на SwipeMate.Mobile.
5. Стартира се мобилният клиент на Windows Machine или Android Emulator.

6. Адрес на backend сървъра
- За Windows Machine:
  http://localhost:5274
- За Android Emulator:
  http://10.0.2.2:5274

7. Първо тестване
За първоначална демонстрация е необходимо:
- да се стартира API проектът
- да се стартира мобилният клиент
- да се създадат поне два потребителски акаунта
- да се добави единият потребител като приятел на другия
- да се създаде групова сесия
- да се приеме поканата от втория акаунт
- да се зададат филтри и да започне swipe гласуване

8. Демонстрационни данни
- Системата създава роли User и Admin
- Администраторски акаунт:
  username: admin
  password: Admin123!

9. Backup/export на базата
- От админ акаунта се отваря "Админ панел".
- Бутонът "Създай backup файл" създава JSON export на основните таблици без паролни хешове.
- За пълен PostgreSQL backup може да се използва и pg_dump:
  pg_dump -h localhost -p 5432 -U postgres -d swipemate -F c -f swipemate.backup
- В папка Tools има готов скрипт:
  powershell -ExecutionPolicy Bypass -File Tools\backup-database.ps1

10. Бележка
При бъдещо развитие проектът може да бъде публикуван в cloud среда. Тогава локалните URL адреси ще бъдат заменени с публичен backend адрес.

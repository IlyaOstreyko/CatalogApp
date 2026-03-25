многослойное приложение 
с окном логирования, 
валидацией ввода данных, 
окном регистрации 
с проверками на наличие ника и мэйла, 
соответствие паролей.
при создании продуктов так же есть валидация ввода данных.
Client — WPF‑клиент с MVVM, UI, сервисами для API, авторизации и SignalR;
Server — Web API + SignalR hub, контроллеры для продуктов, файлов и аутентификации;
Infrastructure — реализация доступа к данным, репозитории, файловое хранилище и UnitOfWork;
Shared — DTO, настройки и общие контракты между клиентом и сервером.
1. использовать скрипт в mssql init.sql
2. настроить DefaultConnection в CatalogApp.Server/DefaultConnection
3. проверить соответствие "BaseUrl": "https://localhost:7088" 
в CatalogApp.Client/appsettings.json 
и CatalogApp.Client/Program.cs/builder.Services.AddCors
все данные хранятся в базе, и картинки тоже
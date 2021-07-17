# Sohbet
### Asp.NET Core Chat app (SignalR)

*App uses:*
* Cookies for authentication.
* SignalR for WebSocket connections.
* Sql connections (without Entity Framework) to interact with database. (Login/Register).

*App has:*
* a single controller, and 2 view pages.
* a single chat hub to handle SignalR WebSocket connections.
* 3 models;
  * QueryManager: For handling database interactions.
  * UserModel: For user credentials.
  * ErrorViewModel: Not really used but still necessary, i think.
* Resources for the 2 view pages, and the layout page for the purpose of supporting the Turkish language.
* a few icons used in the app are placed in the wwwroot folder.

#### Login Page
![image](https://user-images.githubusercontent.com/59491631/125852265-aa14eba3-ff1c-4f7e-84c7-8d42047f84cb.png)

#### Index (Chat) Page
![image](https://user-images.githubusercontent.com/59491631/125853902-ab501258-6e9f-4af5-9488-4be695280f59.png)

### To run the project in Visual Studio

* Create the database using the script. (using Microfost SQL Server Manager) (idk, if it works with other sql servers like mySql)
* Change the 'sohbetdb' connection string in the Sohbet/appsettings.json file.
  * For Windows-Microsoft SQL Server;<br/>
You can see your connection string by adding the created database in Visual Studio's Server Explorer, and connection string is shown at the properties.<br/> (If Server Explorer in Visual Studio can't detect your server, be sure that Sql Server Agent service is running.)
* And just open the solution and start the app

### To deploy the project to Linux

About hosting and deployment, microsoft has a documentation;
https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-5.0

Considering the document, the best suit for me was to host the app on Linux (Ubuntu) with Nginx as reverse proxy, since i had an AWS Ec2 VPS (actually VPC) machine.

And this was the microsoft document that guided me;
https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0


#### Notes that i took to help myself and some of you in the future while following the guide
* Because my website didn't have an SSL certificate, i removed the 5001 port in the "program.cs" file as document directed.
* Also the Microsoft.AspNetCore.HttpOverrides NuGet package had to be installed so that my app could forward headers or something so that kestrel could use a Nginx as a proxy.
* the useForwardHeaders middleware had to be put in the "configure" function in the "startup.cs" file.
* And then the app is published from within Visual Studio. (btw if the "keep the connection string at runtime" option while publishing confuses you as it did confuse me, it should not be ticked.")

* My Nginx 'default' file was this in the end (btw the most top tag which is 'http' is invisibly there as i understand):
(remember to change server_name for your own domain name)
`
  map $http_connection $connection_upgrade {
    "~*Upgrade" $http_connection;
    default keep-alive;
}

  server {
    listen 80;
    server_name zekicaneksi.com *.zekicaneksi.com;

    location / {
        proxy_pass         http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }

    # Configure the SignalR Endpoint
    location /chat {
      # App server url
      proxy_pass http://localhost:5000;

      # Configuration for WebSockets
      proxy_set_header Upgrade $http_upgrade;
      proxy_set_header Connection $connection_upgrade;
      proxy_cache off;
      # WebSockets were implemented after http/1.0
      proxy_http_version 1.1;

      # Configuration for ServerSentEvents
      proxy_buffering off;

      # Configuration for LongPolling or if your KeepAliveInterval is longer than 60 seconds
      proxy_read_timeout 100s;

      proxy_set_header Host $host;
      proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
      proxy_set_header X-Forwarded-Proto $scheme;
    }
  }
`

In case you need to see what processes that listen to ports are running,
sudo lsof -i -P -n | grep LISTEN -> lists processes that listen to ports
sudo killall <name> -> kills all <name> processes
 
After setting the Nginx and running the app with dotnet Sohbet.dll like document directed, my app was working fine except the SQL database part (Login/Register). So i took a break from the guide and started following this;
https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15
 *just for the record; my edition was (developer, free ...) one.*
 I did both of the optional path environment steps.
 After the "connect locally" chapter, when you're set for writing queries, this is how you create the database and the table;
 (i couldn't figure out how to run a script so i ran these line by line, thank you microsoft)
`
USE [master]
GO
CREATE DATABASE [sohbetdb]
GO
USE [sohbetdb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users]( [Id] [int] IDENTITY(1,1) NOT NULL, [Nick] [nvarchar](15) NOT NULL, [Password] [nvarchar](15) NOT NULL ) ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [sohbetdb] SET  READ_WRITE 
GO
` 
 
 By executing this query, i made sure that i created the database.
`
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG='sohbetdb'
GO
`
 
 Afterwards, i closed everything related to servers (Nginx, Kestrel) by using these:
 `
sudo lsof -i -P -n | grep LISTEN -> lists processes that listen to ports
sudo killall <name> -> kills all <name> processes
`
 
 And set the "appsettings.js" file's "sohbetdb" connection string following this syntax;
 "exampleConnectionString": "Server=myServerAddress;Database=myDataBase;User Id=myUsername;password=myPassword;Trusted_Connection=False;MultipleActiveResultSets=true;"
 Mine was this in the end (except the password part):
 Server=localhost;Database=sohbetdb;User Id=SA;password=MYACTUALPASS;Trusted_Connection=False;MultipleActiveResultSets=true;
 
 Afterwards i started Ngnix (sudo service nginx start) and navigated to the app's directory, and started my Kestrel server (dotnet Sohbet.dll).
 Checked if my website is working properly including the Login/Register features, and they were. So i returned to the main doc's "montior the app" part.
 
 My service file in the end was;
 `
[Unit]
Description=blablabal on Ubuntu

[Service]
WorkingDirectory=/var/www/testNetCoreApp
ExecStart=/snap/bin/dotnet /var/www/testNetCoreApp/Sohbet.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
`
 
Note that WorkingDirectory is the directory where the app is, ExecStart is where the dotnet executable is (you can find out with "whereis dotnet") after the ExecStart, the path of the app's assembly dll file (which should be in the WorkingDirectory i suppose)
Removed the comment line about restart since i read that it could cause some problems.
Changed the user to me which was ubuntu. And as the document says, the user needs to have proper rights for app's files. So if you haven't done this already, this is how you give permission to the app files;
sudo chown -R ubuntu /var/www/appFolder  (ubuntu is the user's name, and the path is for the app's folder)
 
 I skipped the timeout and escape characters part. And in the end i was fine but if you aren't, you can comeback to check it out i guess.
 
 If you did something wrong, and the "systemctl status" command gave an error, this is how you restart the service after you change the service definition file;
 (I think first command is enough itself, but idk, i just did both everytime i changed the service definition file)
sudo systemctl daemon-reload
sudo systemctl start kestrel-helloapp.service
 
And then check the status again
sudo systemctl status kestrel-helloapp.service
 
And that's all for a basic setup. Make sure to give a read about the rest of the doc about securtiy and all.

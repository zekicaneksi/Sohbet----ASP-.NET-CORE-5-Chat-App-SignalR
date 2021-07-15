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
* Resources for the 2 view pages, and the layout page for the purpose of supporting Turkish language.
* a few icons used in the app in the wwwroot folder.

#### Login Page
![image](https://user-images.githubusercontent.com/59491631/125852265-aa14eba3-ff1c-4f7e-84c7-8d42047f84cb.png)

#### Index (Chat) Page
![image](https://user-images.githubusercontent.com/59491631/125852461-32273410-596c-4fc1-bfd4-e59ef285b58b.png)


#### To be able to run the project;

* Create the database using the script.
* Change the 'sohbetdb' connection string in the Sohbet/appsettings.json file.
  * For Windows-Microsoft SQL Server;<br/>
You can see your connection string by adding the created database in Visual Studio's Server Explorer, and connection string is shown at the properties.<br/> (if Server Explorer in Visual Studio can't detect your database, be sure that Sql Server Agent service is running.)

## DayZ Database

### Description

Server mod for DayZ Standalone allowing developers of other mods to work with SQLite database.


### Why is this needed?

The modding capabilities of DayZ are limited to the API provided by the game developers.

To store user data on the server side, you can only use binary or JSON serialization to files in your server's profiles folder. This solution works well with mod config files, but absolutely not intended for storing large amounts of data about players, items, etc.

This mod allows you to execute arbitrary SQL queries to the local SQLite database.


### For server owners (how to install)

1. Download last release from github.
2. Extract archive to your server folder.
3. Run "{DAYZ_SERVER}/DatabaseServer/DayzDatabaseServer.exe" manually. This application does not need to be rebooted along with the server and may work for a long time. We recommend that you run it along with the system start. If you have several DayZ servers on the same computer, start several instances with different "--port" parameter.
4. After the first start of the server, the configuration file "DatabaseOptions.json" will created in the "profiles" folder. Edit the database port if needed.


### How does it work

Launching "DayzDatabaseServer.exe" creates a REST service on the port specified in the command line launch parameters. 
The standard port (if not specified) is 2312. 
This port must be closed for external incoming connections using the firewall settings, since all interaction with the DayZ server occurs locally.

Available command line options for "DayzDatabaseServer.exe"
```
  -p, --port    (Default: 2312) Sets the server port.
  --no-logs     (Default: false) Disable logging to file and output.
  --debug       (Default: false) Enable debug logging (Print all queries to the log).
  --help        Display this help screen.
  --version     Display version information.
```

A separate database file will be created for each mod. 
This removes collisions between mods. The database files are stored in the "{DAYZ_SERVER}/profiles/storage" folder. 
These are standard SQLite files, so they can be edited with any utility that supports SQLite databases.


### For mod developers (how to use in other mods)

Add this mod in your mod's dependencies (in config.cpp like ```requiredAddons[] = {..., "Database"};```)
To start working with the database, (install the mod on your server)[### For server owners (how to install)]

#### Examples

###### QuerySync:
Processes query and returns data immediately (thread blocking operation!).
> Excessive use of synchronous database queries can cause freezes on your server. We recommend using QueryAsync whenever possible.
```C++
DatabaseResponse response;
if (GetDatabase().QuerySync("MyModName", "SELECT * FROM my_table", response))
{
	Print("Query success: " + response.GetRowsCount());
	for (int i = 0; i < response.GetRowsCount(); i++) 
	{
		Print("Row " + i + ": id = " + response.GetValue(i, 0) + "; name = " + response.GetValue(i, 1));
	}
}
else
{
	Print("Query failed");
}
```


###### QueryAsync:
Processes query and calls callback function when finished
```C++
void MyQueryCallback(DatabaseResponse response)
{
	if (!response)
	{
		Print("Query failed");
		return;
	}

	Print("Query success: " + response.GetRowsCount());
	for (int i = 0; i < response.GetRowsCount(); i++) 
	{
		Print("Query row " + i + ": id = " + response.GetValue(i, 0) + "; name = " + response.GetValue(i, 1));
	}
}

...

GetDatabase().QueryAsync("MyModName", "SELECT * FROM my_table", this, "MyQueryCallback");
```


###### TransactionSync:
Processes transaction (multiple queries) and returns data immediately (thread blocking operation!).
The transaction consists of several queries and if one of them fails, the result of all previous commands will be rejected.
> Excessive use of synchronous database queries can cause freezes on your server. We recommend using TransactionAsync whenever possible.
```C++
DatabaseResponse response;
if (GetDatabase().TransactionSync("MyModName", {
	"CREATE TABLE IF NOT EXISTS my_table(id integer PRIMARY KEY, name text NOT NULL)"
	"INSERT INTO my_table (name) VALUES ('Dont shoot, im friendly!')",
	"SELECT * FROM my_table"
}, response))
{
	Print("Query success: " + response.GetRowsCount());
	for (int i = 0; i < response.GetRowsCount(); i++) 
	{
		Print("Query row " + i + ": id = " + response.GetValue(i, 0) + "; name = " + response.GetValue(i, 1));
	}
}
else
{
	Print("Query failed");
}
```


###### TransactionAsync:
Processes transaction (multiple queries) and calls callback function when finished.
The transaction consists of several queries and if one of them fails, the result of all previous commands will be rejected.
```C++
void MyQueryCallback(DatabaseResponse response)
{
	if (!response)
	{
		Print("Query failed");
		return;
	}

	Print("Query success: " + response.GetRowsCount());
	for (int i = 0; i < response.GetRowsCount(); i++) 
	{
		Print("Query row " + i + ": id = " + response.GetValue(i, 0) + "; name = " + response.GetValue(i, 1));
	}
}

...

GetDatabase().TransactionAsync("MyModName", {
	"CREATE TABLE IF NOT EXISTS my_table(id integer PRIMARY KEY, name text NOT NULL)"
	"INSERT INTO my_table (name) VALUES ('Dont shoot, im friendly!')",
	"SELECT * FROM my_table"
}, this, "MyQueryCallback");
```

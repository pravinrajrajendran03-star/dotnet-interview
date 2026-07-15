SOLUTION.md

1. Problems Identified: 

1. SQL Injection Vulnerability
2. Hard coded conenction string
3. No DI in previously explicitly retun the logic
4. Interface Added and extended the service
5. Async/Await structural coding
6. Action verb corrected
7. Proper Naming for all controller, Models, services.
8. create individual dto for create, update.
9. Added Pagination.
10. Existing testcase fixed.
11. Deleted old Test Cases written new test cases on controller and service level.


2. Architectural Decisions: 

Three Layer Architecture:
Controller => Service => Database

If needed we can use CQRS Design Pattern.


3.  How to Run: Clear instructions for running the application and tests

Prerequistites:
.NET 8 sdk
Git
vscode or visual studio

todo.db will create automatically in first run.
We can see data in vscode extension SQLite Viewer

swagger we can see in below port: https://localhost:7186/swagger/index.html

cd c:\Projects\TodoAPI

To Build:
dotnet build
To run Test:
dotnet test

To run:
Dotnet run

Debugger mode also we can use.




4. API Documentation: How to use the endpoints

  All the Endpoint and definition in OpenApi.json file.

  CRUD of TODO and Pagination support Enabled.


5. Future Improvements: What would you do if you had more time?

1. Structural Logging
2. EF core Migration and implematation with postgresql DB
3. CQRS Pattern If project Grows with Requirment
4. Authentication and Authorization
5. Rate limiting and load balancing based on users.
6. Integration Test
7. We can build CI CD and we can do whatever needed when application demands


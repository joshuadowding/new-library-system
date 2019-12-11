# NLS "New Library System"

## Technology Stack
- ASP.NET Core (3.1) MVC Project
- .NET Standard (2.1) Class Library
- dotNetRDF (2.2.0) Library
- IIS Express (7.x)
- Apache Jena Fuseki (3.13.1)

## Publishing & Deployment
Pre-configured 'folder' export:
- **Self-Contained** Deployment Mode
- **win-x86** Deployment Target

## Running
1. Extract the contents of all three .zip files provided.
2. Open 'apache-jena-fuseki-3.13.1' and launch the server by double-clicking 'fuseki-server.bat'.
3. Once Fuseki is running, open 'new-library-system-1.0.1' and launch the application by double-clicking 'NLS.exe'.
4. Once the terminal window appears, copy and paste the link containing the HTTP address into your browser.

### Note
- Please do not try to access the application using the HTTPS address - the application doesn't support that.
- While this has been compiled to be a self-contained application, this application might require the [.NET Core 3.1 runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1) appropriate for your system.

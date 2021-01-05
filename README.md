# ScrawlBrawl
A game built for everyone by anyone. Put your wordsmithing and drawing skills to the test with one of many game modes, perfect for small gatherings and huge events alike.

Still a work in progress. Reach out to us if you are interested in contributing!

## Developer Setup (Windows only):
Disclaimer: Running the application will result in logs, errors, and metrics (such as CPU / Memory usage) being collected from your device.
### Downloads
  - Install Visual Studio 2019 (see visualstudio.microsoft.com)
  - Install NodeJS (see nodejs.org)
  - Download this repository
  - Install .NET Core SDK
  - Install PostSharp Community
### Run
  - VS 2019 - Backend, BackendAutomatedTestClient, BackendTests
  - Unity - Unity client
  - VS Code - Web Frontend
### Manual First-time setup
  - Ensure PostSharp license is set up

## To play over LAN / WiFi
Disclaimer: Running this project in this manner involves adding an exclusion to your firewall which may put your computer at risk of an attack. Proceeding with the steps below is an acknowledgement that you understand, accept, and take responsibility for the risks and potential negative outcomes. Also note, it has been many months since this flow has been tested/needed.
  - Add an inbound firewall rule to allow inbound TCP on the port you plan to use (ex: 50403)
  - Edit the ApplicationHost.config file to include a binding for your LAN IP (ctrl+f 'localhost' and add a new entry)
  - Run Visual Studio as an administrator (otherwise IIS express can't be bound to your LAN IP)
  - Ensure the server and client are on the same wifi, enter 'http://[yourip]:[yourport]' in a web browser
  
  
## How To Simulate Players Locally For Debugging/Playthrough
### Using Multiple Browsers
  1. Start up x browsers (chrome, edge, etc + incognito)
### Using AutomatedTestingClient
  1. Run executable with "-browsers -users 4 -games "Chaotic Cooperation" -defaultparams"
  2. Create breakpoint on some point of the test

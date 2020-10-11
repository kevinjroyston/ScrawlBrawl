# RoystonGame
Still a work in progress. Reach out to us if you are interested in contributing!

## Developer Setup (Windows only):
Disclaimer: Running the application will result in logs, errors, and metrics (such as CPU / Memory usage) being collected from your device.
### Downloads
  - Install Visual Studio 2019 (see visualstudio.microsoft.com)
  - Install NodeJS (see nodejs.org)
  - Download this repository
  - Install .NET Core SDK
  - TODO
### Run
  - TODO
  - VS 2019 - Backend, BackendAutomatedTestClient, BackendTests
  - Unity - Unity client
  - VS Code - Web Frontend

## To play over LAN / WiFi
Disclaimer: Running this project in this manner involves adding an exclusion to your firewall which may put your computer at risk of an attack. Proceeding with the steps below is an acknowledgement that you understand, accept, and take responsibility for the risks and potential negative outcomes.
  - Add an inbound firewall rule to allow inbound TCP on the port you plan to use (ex: 50403)
  - Edit the ApplicationHost.config file to include a binding for your LAN IP (ctrl+f 'localhost' and add a new entry)
  - Run Visual Studio as an administrator (otherwise IIS express can't be bound to your LAN IP)
  - Ensure the server and client are on the same wifi, enter 'http://[yourip]:[yourport]' in a web browser


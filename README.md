# RoystonGame
Very much a work in progress.

Disclaimer: Running this project locally involves running a web server on your local computer, doing this requires adding an exclusion to your firewall which may put your computer at risk of an attack. Make sure you understand the repercussions and potential risks of the setup steps below. You install / run this project at your own risk.

To develop/run locally (Windows only):
- Downloads
  - Install Visual Studio 2019 (see visualstudio.microsoft.com)
  - Install NodeJS (see nodejs.org)
  - Download this repository
- To test locally
  - Just hit the play button, no additional steps needed.
- To play over LAN / WiFi
  - Add an inbound firewall rule to allow inbound TCP on the port you plan to use (ex: 50403)
  - Edit the ApplicationHost.config file to include a binding for your LAN IP (ctrl+f 'localhost' and add a new entry)
  - Run Visual Studio as an administrator (otherwise IIS express can't be bound to your LAN IP)
  - Ensure the server and client are on the same wifi, enter 'http://[yourip]:[yourport]' in a web browser


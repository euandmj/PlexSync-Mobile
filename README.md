Mobile client for the [PlexSync Server](https://github.com/euandmj/PlexSync-Server). Remotely add torrents, view downloads or files on the server.

### Check the [google drive](https://drive.google.com/open?id=1einZzj-qU4NlYMcgrXWWuSjrAltXhYRW) for the latest build

<h2>Requirements:</h2>

* Plex Server: [here](https://www.plex.tv/)

* qBitTorrent with WEB API configured to listen on port 8080: available [here](https://www.qbittorrent.org/)

* python 3

* pip 3

<h2>Setup</h2>

1. Download the latest server build and extract file contents

2. Configure your config.ini 

3. Download and install the APK

4. In the server directory install python packages with `pip install -r requirements.txt`

5. Make sure Plex Media Server and qBitTorrent are running. 

6. Start up the server with `python3 main.py`

  **The mobile client should now be able to connect to the server**

### Configuration Options

The server can be configured via the config.ini file.

| Section     	| Config      	| Description                                            	|
|-------------	|-------------	|--------------------------------------------------------	|
| General     	| savepath    	| the default directory for torrents to be downloaded to 	|
| Server      	| name        	| name of the machine for logging purposes               	|
|             	| host        	| ipv4 of the server                                     	|
|             	| port        	| port for the server to listen to                       	|
| Plex        	| server      	| plex media server name                                 	|
|             	| username    	| your plex account username                             	|
|             	| password    	| your plex account password                             	|
|             	| directories 	| the various libararies for the plex media i.e. movies  	|
| qBittorrent 	| host        	| the hostname of the server running qBitTorrent WebUI   	|
|             	| username    	| the username for the qBitTorrent WebUI client          	|
|             	| password    	| the password the for the qBitTorrent WebUI client      	|



<h3>Navigation Menu</h3>
<img src = https://github.com/euandmj/PlexSync-Mobile/blob/master/build_images/menu.jpg width="356" height="633">

<h3>Send Magnets</h3>
<img src = https://github.com/euandmj/PlexSync-Mobile/blob/master/build_images/send_magn.jpg width="356" height="633">

<h4>Disclaimer</h4>
this is a personal project with limited testing on a OnePlus3 amd64, despite this effort has been made to make it as crossplatform as possible. This app comes with no guarantee of stability. 

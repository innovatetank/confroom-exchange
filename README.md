# confroom-exchange
Tablet based management system for conference rooms that integrates with Exchange. This project 
should be used in corporate environments that have designated conference rooms whose schedules are
controlled via Microsoft Exchange. The idea is to have an iPad mounted on the wall outside of each
conference room. The iPad indicates what meetings are upcoming and in-progress, as well as allows anyone
to walk up to the iPad and instantly book the room for an impromptu meeting. 

This project consists of two pieces. The first piece, *ConfRoomServer*, is a C# NancyFx application that 
runs on a Windows server, and is installed as a Windows service. *ConfRoomServer* connects and interacts with
your exchange server. 

The second piece, *ConfRoomClient* is the tablet application. *ConfRoomClient* is an Ionic Framework 
cross platform mobile app that utilizes Apache Cordova for packaging the app for iOS, Android, etc. 

## ConfRoomServer Installation

1. First, clone this repo to a Windows development machine that has Visual Studio 2013+ and .NET Framework 4.0+
2. Build the ConfRoomServer.sln 
3. On your Windows Server, make a folder for ConfRoomServer, i.e. `C:\ConfRoomServer`
4. Copy the contents `bin\Debug` folder from the compiled ConfRoomServer to your server folder.
5. Register the ConfRoomServer.exe as a Windows Server:

```
cd\Windows\Microsoft.NET\Framework64\v4.0.30319
InstallUtil C:\ConfRoomServer\ConfRoomServer.exe
```

6. Open Services from the Control Panel and verify that ConfRoomServer service is there. Don't start the service until it is configured.

## ConfRoomServer Configuration Settings

Configuration settings for the server can be applied either by setting environment variables or by App settings to the `C:\ConfRoomServer\ConfroomServer.exe.config` file. Some settings use App settings, others use environment variables. Some can use both. The ones that use both, environment variables override App settings.

##### listenerUri

Set with appSetting `listenerUri`.

This is the URL that the server service listens on, for connections from the iPad application. The default port is 8977 but you can change it to whatever you want.
You will have less problems if the port number is > 1024. The service **must** run on HTTPS, and was designed this way because of App Transport Security restrictions in iOS 9. 

##### Exchange credentials 

This is the credentials used to authenticate to Microsoft Exchange server. The user should have a mailbox created, although the mailbox is not 
used for scheduling. 

Set with environment variables:
* CONFROOMSERVER_DOMAIN
* CONFROOMSERVER_USER
* CONFROOMSERVER_PASSWORD

The `exchangeUri` setting you find in Appsettings is no longer used. The server now uses Autodiscovery to locate the exchange server, using the email address
that is passed in for the conference room.

##### companyLogoUrl

Set with appSetting `companyLogoUrl` or environment variable `CONFROOMSERVER_LOGOURL`.

This is a JPG image to use as the background for the left side of the iPad screen. The one we are using is 1298x1536 and is 47K. We found that 
an image that is larger file size (i.e. 100k+) does not work; it is probably hitting some kind of serialization limitation in the web interface. 

##### pollingIntervalSeconds

Set with appSetting `pollingIntervalSeconds` or environment variable `CONFROOMSERVER_POLLINGINTERVAL`.

This is the number of seconds that the iPad application waits between refreshing the screen (updating the clock, getting new appointment data
from Exchange, etc).  Default is 60 seconds. 

##### availableColor

Set with appSetting `availableColor` or environment variable `CONFROOMSERVER_AVAILABLECOLOR`.

The banner on the iPad app turns this color when the room is available (no meeting in progress). Should be a standard web color, i.e. `#0000FF` (green).

##### busyColor

Set with appSetting `busyColor` or environment variable `CONFROOMSERVER_BUSYCOLOR`.

The banner on the iPad app turns this color when the room is busy (meeting in progress). Should be a standard web color, i.e. `#FF0000` (red).

##### availableRoomText

Set with appSetting `availableRoomText` or environment variable `CONFROOMSERVER_AVAILABLEROOMTEXT`.

The banner on the iPad app shows this message in big bold letters when the room is available. Default value: AVAILABLE

##### busyRoomText

Set with appSetting `busyRoomText` or environment variable `CONFROOMSERVER_BUSYROOMTEXT`.

The banner on the iPad app shows this message in big bold letters when the room is busy. Default value: IN USE

##### adminMenuPassword

Set with appSetting `adminMenuPassword` or environment variable `CONFROOMSERVER_ADMINMNUPWD`.

The iPad application has an admin menu where you configure the connection to the server. This menu is key code protected. The admin password
is a numerical pin number that is used to access the menu. Defaults to 12345.

## ConfRoomServer SSL 

The iPad application expects to communicate with the ConfRoomServer over HTTPS, so you will need to setup an new certificate on your server for this to work. The examples below assumes you are trying to use a self-signed certificate for this process.

##### Here are some articles that can help you with this process:

[http://stackoverflow.com/questions/27553633/nancyfx-self-hosting-over-https](http://stackoverflow.com/questions/27553633/nancyfx-self-hosting-over-https)
[https://coderead.wordpress.com/2014/08/07/enabling-ssl-for-self-hosted-nancy/](https://coderead.wordpress.com/2014/08/07/enabling-ssl-for-self-hosted-nancy/)

##### Create new certificate

Use `openssh` from your dev machine (installed via Cygwin) to create a new certificate:

```
$ openssl req -x509 -newkey rsa:2048 -keyout key.pem -out cert.pem
$ openssl pkcs12 -export -in cert.pem -inkey key.pem -out mycert.pfx
```

Enter a password for your passphrase and write it down. 

Copy the mycert.pfx file to your ConfRoomServer server.

##### Install certificate

1. From the ConfRoomServer, run `mmc.exe`
2. Add the `Certificates` snap in
3. Right click on the `Personal` folder and click `Import`
4. Make sure the cert imported:
  * Under Personal, Certificates, click on properties on the cert. 
  * Go to the Details tab, scroll down in the list, copy Thumbprint to your clipboard and paste to notepad.
  * Remove all the spaces in the hash.  i.e. cb 21 13 b7... becomes cb2113b7...

##### Setup Access Control List (ACL) for URL

Run `powershell.exe` and run this command, replacing *servername* with your actual server name.

```
netsh http add urlacl url=https://servername:8977/ user="NT AUTHORITY\LOCAL SERVICE"
```

Verify that it was succesfully added:

```
netsh http show urlacl
```

##### Register the SSL cert to be used for your server port #

From powershell, replace `8977` with your server port specified above with `listenerUri`. Replace the value for `certhash` with the thumbprint you copied to notepad in the prior *Install Certificate* step. The `appid` is just a unique GUID. It is not important what it is, as long as it is unique.

```
netsh http add sslcert ipport=0.0.0.0:8977 appid="{06aabebd-3a91-4b80-8a15-adfd3c8a0b23}" certhash=1c1f3e00059f2a743417f59c102907b114e28bbc
```

Verify that it was succesfully added:

```
netsh http show sslcert
```

##### Make the cert Trusted

After completing the above steps, *then* you can drag it from the Personal certificate folder to the 
Trusted Root Certification Authorities folder, using the Certificates mmc snap-in.

##### Uninstall to change port:

If you need to change the port # after installing the cert, use the following powershell command to delete the sslcert:

```
netsh http delete sslcert ipport=0.0.0.0:8977
```

*NOTE, if you delete it, you will have to use a different GUID when importing it again*

##### Start the server

After completing all the above steps, you can start the ConfRoomServer service from the control panel.

## ConfRoomClient installation

From your mac, first install Ionic Framework and Apache Cordova.

```
npm install -g cordova ionic 
```

From your mac, clone the confroom-exchange repo.

Install bower dependencies:

```
bower install
```

Add ios platform:

```
ionic platform add ios
```

##### Configuring ios project

Using a text editor, open `platforms/ios/ConfRoomClient/ConfRoomClient-Info.plist` and add this section to the bottom, replacing **mydevmachine** and **myserver** with your actual machine names.

```
    <key>NSAppTransportSecurity</key>
    <dict>
      <key>NSExceptionDomains</key>
      <dict>
        <key>mydevmachine</key>
        <dict>
          <key>NSTemporaryExceptionAllowsInsecureHTTPLoads</key>
          <true/>
          <key>NSTemporaryExceptionMinimumTLSVersion</key>
          <string>TLSv1.0</string>
          <key>NSTemporaryExceptionRequiresForwardSecrecy</key>
          <false/>
        </dict>
        <key>myserver</key>
        <dict>
          <key>NSTemporaryExceptionAllowsInsecureHTTPLoads</key>
          <true/>
          <key>NSTemporaryExceptionMinimumTLSVersion</key>
          <string>TLSv1.0</string>
          <key>NSTemporaryExceptionRequiresForwardSecrecy</key>
          <false/>
        </dict>
      </dict>
    </dict>
```

If you are having trouble with this config of ATS, you can also try a more unsecured version, which completely turns off ATS:

```
    <key>NSAppTransportSecurity</key>
    <dict>
      <key>NSAllowsArbitraryLoads</key>
      <true/>
    </dict>
```

##### Building the ConfRoomClient

I've included a script to make repetitive building/emulating easier. It is in the `ConfRoomClient` folder and is called `emulate.sh`.  Make it executable and run it; from terminal, run:

```
chmod +x emulate.sh
./emulate.sh
```

This script will run `ionic build` and `ionic emulate` and run the iOS emulator.

To build from xCode and deploy to an actual device, open the project `/ConfRoomClient/platforms/ios/ConfRoomClient.xcodeproj` with Xcode.

##### Configuring the running app

First, verify that your iPad has network connectivity to your ConfRoomServer. I.E. it is connected to a wireless network that is routed to the server's network, or connected properly to a VPN, etc. 

When the app first runs, it will be stuck at a loading screen because you haven't told it where the ConfRoomServer is, what email to address to use for exchange, and the port # to use for the ConfRoomServer.  

Click on the hamburger (three dashes) in the top right corner and click on the menu item to configure. 

Enter the default password `12345`.

From the connections dialog that opens, enter the email address for your exchange conference room, the host name of the ConfRoomServer, and the port it is using. Click Save.

If everything is successful, the app should reload and pull the name of the conference room, show your background logo, and pull all the appointments for the room.

Enjoy!


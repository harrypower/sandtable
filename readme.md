# Steps to use UART on Beaglebone black
## 1. Edit /boot/uEnv.txt
  * `sudo nano /boot/uEnv.txt `
  * add `cape_enable=bone_capemgr.enable_partno=BB-UART1,BB-UART2`
    * note this is turning on /dev/ttyo1 and /dev/ttyo2
    * note this now will turn the uart back on at reboot
  * after you reboot you can test uart with `ls -l /dev/ttyo*` and you should get a list of the devices present.

## 2. Update beaglebone black debian image  
    ```
        cd /opt/scripts/tools/
        sudo ./update_kernel.sh
        sudo apt-get update
        sudo reboot
    ```
## 3. Wifi setup using connmanctl
  * configure connmanctl
    ```
      sudo connmanctl
      tether wifi disable
      enable wifi
      scan wifi
      services
      config wifi_*_managed_psk autoconnect on
      agent on
      connect wifi_*_managed_psk
      quit       
    ```
      * note in the above commands the `wifi_*_managed_psk` needs to be changed to the wifi identifier that shows up the `services` command is issued.  This identifier will be a long hex string but needs to be copied exactly.
      * note after the `connect` command is issued above connmanctl will ask for a password.  Simply enter the password for the wifi access point.
      * at this point the wifi is setup and should connect automatically at reboot but only if eithernet is not connect.  This automatic connection does not happen sometimes so the next part is needed to ensure autoconnection.
  * Install and adapt adafruit's wifi reset service.
    ```
    sudo git clone https://github.com/adafruit/wifi-reset.git
    cd wifi-reset
    chmod +x install.sh
    ```
    * at this point the service is there but it needs to be adapted to use connmanctl and then installed.  Edit the wifi-reset.sh file with nano to do only the following:
    ```
    sleep 30
    connmanctl connect wifi_*_managed_psk
    ```
    * note the `wifi_*_managed_psk` is the exact same as in the connmanctl configure steps above.
    * At this point this modified script needs to be installed to use from now on after system start up so do the following:
    ```
    sudo ./install.sh
    ```
    * now this install script simply sets up a service that runs connmanctl and connects to the wifi after a timed delay of 30 seconds.  
    * To turn off this system service use the following:
    ```
    systemctl disable wifi-reset.service
    ```

## 4. Remove some services on BBB
  ```
  sudo systemctl disable cloud9.service
  sudo systemctl disable bonescript.service
  sudo systemctl disable bonescript.socket
  sudo systemctl disable bonescript-autorun.service
  sudo apt-get remove npm
  sudo apt-get remove node*
  sudo apt-get remove --auto-remove avahi-daemon
  sudo apt-get purge --auto-remove avahi-daemon
  sudo apt-get autoremove
  sudo apt-get autoclean
  ```
## 5. Reconfigure Apache for port 80
  * Edit `/etc/apache2/sites-enabled/000-default.conf` as follows:
  ```
  <VirtualHost*:8080>
  ```
  change to
  ```
  <VirtualHost*:80>
  ```
  * Edit `/etc/apache2/ports.conf` as follows:
  ```
  Listen 8080
  ```
  change to
  ```
  Listen 80
  ```

## 6. Configure Apache cgi stuff

[CHIP Apache setup information](http://www.chip-community.org/index.php/CGI_program_on_CHIP)
I have summarized the information here from the link with some changes. Note the information is for the CHIP machine but it works for BBB also!  

```
cd /etc/apache2/mods-enabled
sudo ln -s ../mods-available/cgi.load
sudo apachectl -k graceful
```
* Edit `/etc/apache2/etc/apache2/sites-enabled/000-default.conf` as follows :
   * place this after `DocumentRoot` command and before `ErrorLog` command.
```
ScriptAlias "/cgi-bin/" "/usr/lib/cgi-bin/"
AddHandler cgi-script .cgi
<Directory "/usr/lib/cgi-bin/">
  Options +ExecCGI
</Directory>
```

This makes the `/usr/lib/cgi-bin/` directory the place to put cgi code and this code should have an extention of .cgi and the privileges on these codes should be changed to 755 or higher!
Now the `.html` files and the `index.html` file can be placed in this directory `/var/www/html/`.
So basically these two directory's are the places to put stuff to be accessed remotely on the network.

* Restart Apache to make settings current!
```
sudo service apache2 restart
```

Gforth script can be in a simple file that has .cgi extension with a privilage of 755.  The file needs the following in its first line:

```
#! /usr/local/bin/gforth-arm
```

This first line simply tells the system where the gforth engine is located in the system that is to be used with the script.  
Once you make the gforth script and save it you need to change its permissions to be executable as follows:

```
sudo chmod +x name-of-cgi-code
```

You can test and access the files served locally by using any of the commands below.

```
ping localhost
wget localhost/cgi-bin/name-of-cgi-code
wget localhost
```

Each of the above lines will give different information but they should all show the system working!

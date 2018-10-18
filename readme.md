# Steps to use UART on Beaglebone black
1. ## Edit /boot/uEnv.txt
  * `sudo nano /boot/uEnv.txt `
  * add `cape_enable=bone_capemgr.enable_partno=BB-UART1,BB-UART2`
    * note this is turning on /dev/ttyo1 and /dev/ttyo2
    * note this now will turn the uart back on at reboot
  * after you reboot you can test uart with `ls -l /dev/ttyo*` and you should get a list of the devices present.
2. ## Update beaglebone black debian image  
    ```
        cd /opt/scripts/tools/
        sudo ./update_kernel.sh
        sudo apt-get update
        sudo reboot
    ```
3. ## Wifi setup using connmanctl
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
4. ## Remove some services on BBB
  ```
  sudo systemctl disable cloud9.service
  sudo systemctl disable bonescript.service
  sudo systemctl disable bonescript.socket
  sudo systemctl disable bonescript-autorun.service
  ```

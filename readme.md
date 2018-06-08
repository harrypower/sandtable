# Steps to use UART on Beaglebone black
1. Edit /boot/uEnv.txt
  * `sudo nano /boot/uEnv.txt `
  * add `cape_enable=bone_capemgr.enable_partno=BB-UART1,BB-UART2`
    * note this is turning on /dev/ttyo1 and /dev/ttyo2
    * note this now will turn the uart back on at reboot
  * after you reboot you can test uart with `ls -l /dev/ttyo*` and you should get a list of the devices present.
2. Tell debian to use the pins for UART
  * `sudo config-pin p9.24 uart`
  * `sudo config-pin p9.26 uart`
    * this is /dev/ttyo1
  * `sudo config-pin p9.21 uart`
  * `sudo config-pin p9.22 uart`
    * this is /dev/ttyo2
  * You can confirm the pins by doing `config-pin -q p9.22` and you should see `P9_22 Mode: uart` as example.
  * Note you need to tell debian these same commands every time the beaglebone black gets rebooted for the uart pins to be in uart mode!
  

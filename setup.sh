#!/bin/bash

sudo rm /var/www/html/index.html
sudo rm /var/www/html/testing.html
sudo rm /var/www/html/sandtablestyle.css
sudo rm /usr/lib/cgi-bin/cgi-get-test.cgi
sudo rm /usr/lib/cgi-bin/gohome.cgi
sudo rm /run/cgitest.tmp
sudo rm /usr/lib/cgi-bin/directtesting.cgi

sudo cp index.html /var/www/html/index.html
sudo cp testing.html /var/www/html/testing.html
sudo cp sandtablestyle.css /var/www/html/sandtablestyle.css
sudo cp cgi-get-test.cgi /usr/lib/cgi-bin/cgi-get-test.cgi
sudo cp gohome.cgi /usr/lib/cgi-bin/gohome.cgi
sudo cp directtesting.cgi /usr/lib/cgi-bin/directtesting.cgi
sudo touch /run/cgitest.tmp

sudo cp BBB_Gforth_gpio/syscalls386.fs /usr/lib/cgi-bin/syscalls386.fs

sudo chmod 755 /usr/lib/cgi-bin/cgi-get-test.cgi
sudo chmod 755 /usr/lib/cgi-bin/gohome.cgi
sudo chmod 755 /usr/lib/cgi-bin/directtesting.cgi
sudo chmod 666 /run/cgitest.tmp

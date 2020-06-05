#!/bin/bash

sudo rm /var/www/html/index.html
sudo rm /var/www/html/testing.html
sudo rm /var/www/html/sandtablestyle.css
sudo rm /usr/lib/cgi-bin/directtesting.cgi
sudo rm /var/www/html/favicon.ico
sudo rm /var/www/html/lines.html
sudo rm /var/www/html/othercmds.html

sudo cp index.html /var/www/html/index.html
sudo cp testing.html /var/www/html/testing.html
sudo cp sandtablestyle.css /var/www/html/sandtablestyle.css
sudo cp directtesting.cgi /usr/lib/cgi-bin/directtesting.cgi
sudo cp favicon.ico /var/www/html/favicon.ico
sudo cp lines.html /var/www/html/lines.html
sudo cp othercmds.html /var/www/html/othercmds.html

sudo chmod 777 /usr/lib/cgi-bin/directtesting.cgi

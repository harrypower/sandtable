#!/bin/bash

sudo rm /var/www/html/index.html
sudo rm /var/www/html/testing.html
sudo rm /var/www/html/sandtablestyle.css
sudo rm /usr/lib/cgi-bin/directtesting.cgi

sudo cp index.html /var/www/html/index.html
sudo cp testing.html /var/www/html/testing.html
sudo cp sandtablestyle.css /var/www/html/sandtablestyle.css
sudo cp directtesting.cgi /usr/lib/cgi-bin/directtesting.cgi

sudo chmod 755 /usr/lib/cgi-bin/directtesting.cgi

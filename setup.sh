#!/bin/bash

sudo rm /var/www/html/index.html
sudo rm /usr/lib/cgi-bin/cgi-get-test.cgi
sudo rm /run/cgitest.tmp

sudo cp index.html /var/www/html/index.html
sudo cp cgi-get-test.cgi /usr/lib/cgi-bin/cgi-get-test.cgi
sudo touch /run/cgitest.tmp

sudo chmod 755 /usr/lib/cgi-bin/cgi-get-test.cgi
sudo chmod 666 /run/cgitest.tmp
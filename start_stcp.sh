#!/bin/bash

nohup sudo --user=root --group=root /home/debian/sandtable/stcp.fs > /home/debian/sandtable/stcp.data 2>&1 &

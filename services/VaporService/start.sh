#!/bin/bash
mkdir /app/data
chown vaporservice: /app/data -R
chmod 700 /app/data -R

su vaporservice -s /bin/sh -c 'dotnet VaporService.dll --urls=http://0.0.0.0:9000/'

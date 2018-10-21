#!/bin/sh

cat "$2" | tr -d ".,“\":;?!\-\'" | tr -s " " "\n" | sort | uniq -c | sort -n -r | egrep -a "\b$1\b" 

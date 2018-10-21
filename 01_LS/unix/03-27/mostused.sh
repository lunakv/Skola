#!/bin/sh

cat "$1" | tr -d ".,“\":;?!\-\'" | tr -s " " "\n" | sort | uniq -c | sort -n -r | head -n 5 


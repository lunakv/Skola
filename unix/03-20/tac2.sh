#!/bin/sh

cat -n "$1" | sort -n -r -k1,1 | cut -f1 --complement  

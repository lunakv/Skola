#!/bin/sh

getent passwd | cut -f 5 -d : | egrep -i "[[:alnum:]]+ [[:alnum:]]+ [[:alnum:]]+" 

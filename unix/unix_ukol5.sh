#!/bin/sh
#Také k nalezení jako 03-27/ukoly/sedscript.sh

tmp=`date -Iseconds`_sedscript1
if ! mkdir -p $tmp ; then
	echo "Unable to create temporary directory"
	exit 1
fi

sed -r '
s#(.+) -([0-9]*)-> (.+)#\2 s/\1/\3/g#
s#(.+) <-([0-9]*)- (.+)#\2 s/\3/\1/g#
' > $tmp/script.tmp < "$2"

sed -r -f $tmp/script.tmp < "$1"
rm -r $tmp

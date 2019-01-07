#!/bin/sh
#varianta 1
tmp=`date -Iseconds`_"$$"

if ! mkdir $tmp ; then
        echo "Unable to create temporary directory" >&2
        return 1
fi
if [ ! -f "$GROUP" ]; then
        echo "Variable GROUP does not contain a valid file" >&2
        return 1
fi
if [ ! -f "$PASSWD" ]; then
        echo "Variable PASSWD does not contain a valid file" >&2
        return 1
fi
if [ "$#" -eq 0 ]; then
        echo "This script requires an argument" >&2
        return 1
fi

sort -t: -k 1,1 "$GROUP" > $tmp/group                           #seradi $GROUP podle jmena
echo "$1" | join -t: -j 1 "$tmp/group" - > $tmp/match           #vybere skupinu z parametru
cut -d: -f 4 "$tmp/match" | tr "," "\n" | sort > $tmp/usrs      #ulozi jmena uzivatelu ze skupiny

sort -t: -k 1,1 "$PASSWD" | join -t: -o "2.5" -j 1 "$tmp/usrs" - | cut -d, -f1          #najde shody se ziskanymi jmeny
sort -t: -k 4,4 "$PASSWD" | join -t: -1 3 -2 4 -o "2.5" "$tmp/match" - | cut -d, -f1    #najde usrs s vychozi skupinou

rm -r $tmp
#!/bin/sh
rand="$@"

while ! echo $rand | tr " " "\n" | sort -C ; do

        rand=`echo "$@" | tr " " "\n" | sort -R | tr "\n" " "`
        echo "$rand"
done;
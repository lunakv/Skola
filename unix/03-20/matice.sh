#!/bin/sh



while read radek; do
 while [ "$radek" != "" ]; do
 radek="$(cut -f 1 -d " " --complement <<< $radek)"
 echo "$radek"
 done;

done < "$1"


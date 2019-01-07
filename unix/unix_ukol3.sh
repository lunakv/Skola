#!/bin/sh

temp=$(date -Iseconds)_bankovniscript1

if mkdir "$temp"; then

 zustatek="$1"

 if [ "$#" -ge 2]]; then
    in="$2"
 else
    in="-"
 fi


 cut -f 1 -d ";" "$in" > "$temp"/popis.tmp
 cut -f 2 -d ";" "$in" > "$temp"/zmena.tmp
 cut -f 3 -d ";" "$in" > "$temp"/prijemce.tmp

 cd "$temp"

 while read -r radka; do
   zustatek="$((zustatek + radka))"
   echo "$zustatek" >> zustatky.tmp
 done < zmena.tmp

 paste -d ";" zmena.tmp zustatky.tmp prijemce.tmp popis.tmp > out.tmp

 cd ..

 if [ "$#" -ge 3 ]; then
  cat "$temp"/out.tmp > "$3"
 else
  cat "$temp"/out.tmp
 fi

 rm -rf "$temp"

else
 echo "Error: Nebylo mozno vytvorit pomocny adresar" >&2
fi
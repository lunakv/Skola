#!/bin/sh
# Také k nalezení jako 03-20/ukoly/lookup.sh

temp=tmp_$(date -Iseconds)_vyhledavaciSkript
if ! mkdir $temp; then
  echo "error"
  exit 1
fi
delim=" "

while getopts ":i:w:W:p:P:d:" opt; do				#get arguments
	case $opt in 
		i)
			echo "$OPTARG" >> $temp/soubory.tmp
			;;
		w)
			echo "$OPTARG" >> $temp/wordlist.tmp
			;;
		W)
			echo "$OPTARG" >> $temp/ciwordlist.tmp
			;;
		p)
			echo "$OPTARG" >> $temp/wordpart.tmp
			;;
		P)
			echo "$OPTARG" >> $temp/ciwordpart.tmp
			;;
		d)
			delim="$OPTARG"
			;;
		\?)
			echo "Option -$OPTARG is invalid" >&2
			exit 1
			;;
		:)
			echo "Option -$OPTARG requires an argument" >&2
			exit 1
			;;
	esac
done

for soubor in `cat $temp/soubory.tmp`; do	#go through each file
  cisloRadky=0

  while read radka; do				#go through each line of file
	cisloRadky=$((cisloRadky + 1))
	echo "$radka" | tr -s "$delim" "\n" > $temp/radka.tmp	#tr words to lines

	echo > $temp/found.tmp			#make sure file exists & reset on every line
	
	if [ -f $temp/wordlist.tmp ]; then	#check expr file exists
		while read expr; do
			grep -s -n -x "$expr"  $temp/radka.tmp >> $temp/found.tmp
		done < "$temp"/wordlist.tmp
	fi

	if [ -f $temp/ciwordlist.tmp ]; then	#check expr file exists
		while read expr && [ -f $temp/ciwordlist ]; do
			grep -s -n -x -i "$expr" $temp/radka.tmp >> $temp/found.tmp 
		done < $temp/ciwordlist.tmp
	fi
	
	if [ -f $temp/wordpart.tmp ]; then	#check expr file exists
		while read expr; do
			grep -s -n "$expr" $temp/radka.tmp >> $temp/found.tmp 
		done < $temp/wordpart.tmp
	fi

	if [ -f $temp/ciwordpart.tmp ]; then	#check expr file exists
		while read expr; do
			grep -s -n -i "$expr" $temp/radka.tmp >> $temp/found.tmp  
		done < $temp/ciwordpart.tmp
	fi

	for match in `tail -n +2 $temp/found.tmp`; do #omits first dummy line
		echo "$soubor":"$cisloRadky":"$match"
	done
	
  done < "$soubor" 
done

rm -r "$temp"


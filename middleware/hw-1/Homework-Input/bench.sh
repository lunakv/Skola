#!/bin/bash
NODES=1000
MAX_EDGES=$(($NODES * ($NODES - 1) / 2))

DIR=.
while [ $# != 0 ]; do
	if [ "$1" = "rs" ]; then
		SEARCHER="remote-searcher"
	else
		DIR="$1"
	fi
	shift
done
mkdir -p benchmarks/"$DIR"

for i in $(seq 100); do
	EDGES=$(($MAX_EDGES * $i / 100))
	if ./run-client $NODES $EDGES $SEARCHER > benchmarks/"$DIR"/$i
   	then
		echo "$i"
	else
		exit
	fi
done
		

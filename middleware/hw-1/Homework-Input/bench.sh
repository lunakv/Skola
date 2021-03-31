#!/bin/bash
NODES=1000
DIR=.

while [ $# != 0 ]; do
	if [ "$1" = "rs" ]; then
		SEARCHER="remote-searcher"
	elif [ "$1" = "rn" ]; then
		NODEFACTORY="remote-nodes"
	elif [ "$1" = "bn" ]; then
		NODEFACTORY="both-nodes"
	elif [ "$1" = "-n" ]; then
		shift
		NODES="$1"	
	else
		DIR="$1"
	fi
	shift
done

MAX_EDGES=$(($NODES * ($NODES - 1) / 2))
mkdir -p benchmarks/"$DIR"

for i in $(seq 100); do
	EDGES=$(($MAX_EDGES * $i / 100))
	if ./run-client $NODES $EDGES $SEARCHER $NODEFACTORY > benchmarks/"$DIR"/$i
   	then
		echo "$i"
	else
		exit
	fi
done
		

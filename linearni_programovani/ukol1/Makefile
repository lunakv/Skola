ROOT_DIR = $(dir $(realpath $(lastword $(MAKEFILE_LIST))))
CFLAGS = -D 'DIR="$(ROOT_DIR)"'

homework1: hw1 hw2

hw1:
	gcc $(CFLAGS) -o lunakv_1_1 hw_1.c

hw2:
	gcc $(CFLAGS) -o lunakv_1_2 hw_2.c

debug: hw1d hw2d

hw1d: 
	gcc $(CFLAGS) -D 'DEBUG' -o lunakv_1_1 hw_1.c

hw2d:
	gcc $(CFLAGS) -D 'DEBUG' -o lunakv_1_2 hw_2.c




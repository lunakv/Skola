#include <stdio.h>
#include <stdlib.h>
#include "c2_list.h"

void swap(int* a, int* b) {
	int c = *a;
	*a = *b;
	*b = c;
}

void print_list(struct ListElem *l) {
	while (l) {
		printf("%d\n", l->value);
		l = l->next;
	}
}

void print_list2(struct ListElem *l) {
	for(; l; l = l->next)
		printf("%d\n", l->value);
}

void push_list(struct ListElem **l, int val) {
	struct ListElem *prvek = malloc(sizeof(struct ListElem));
	prvek->value = val;
	prvek->next = *l;
	*l = prvek;
}

void create_new_list(struct ListElem **l) {
		*l = NULL;
}

void pop_list(struct ListElem **l) {
	struct ListElem *to_remove = *l;
	*l = (*l)->next;
	free(to_remove);
}

void erase_list(struct ListElem **l) {
	while(*l) 
		pop_list(l);
}

void bubble_sort(struct ListElem *l) {
	for (struct ListElem *i = l; i; i = i->next)
	for (struct ListElem *j = l; j; j = j->next)
		if (j->next && j->value > (j->next->value))
			swap(&j->value, &j->next->value);	
}

#include "c2_list.h"

int main() {
	struct ListElem *l;
	create_new_list(&l);
	push_list(&l, 123);
	push_list(&l, 42);
	push_list(&l, 666);
	bubble_sort(l);
	print_list(l);
	pop_list(&l);
	erase_list(&l);
	return 0;
}


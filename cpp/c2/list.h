struct ListElem {
	int value;
	struct ListElem *next;
};
void swap(int *a, int *b);
void print_list(struct ListElem *l);
void print_list2(struct ListElem *l);
void push_list(struct ListElem **l, int val);
void create_new_list(struct ListElem **l);
void pop_list(struct ListElem **l);
void erase_list(struct ListElem **l);
void bubble_sort(struct ListElem *l);

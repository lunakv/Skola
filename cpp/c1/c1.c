#include <stdio.h>
#include <stdlib.h>

int rect_a(int a, int b) {
	return a*b;
}

int gcd(int a, int b) {
	if (a < b) return gcd(b, a);
	if (a > b) return gcd(a-b, b);
	return a;
}

void swap(int* a, int* b) {
	int c = *a;
	*a = *b;
	*b = c;
}

int gcd2(int a, int b) {
	while (a != b) {
		if (a < b) 
			swap(&a,&b);
		a = a-b;
	}
	return a;
}	

void sort(int *a, int len) {
	int i, j;
	for(i = 0; i < len; i++) 
	for(j = i; j < len; j++)
		if (a[i] > a[j])
			swap(a+i, a+j);
}

int main() {
	printf("%d\n", rect_a(4,5));
	printf("%d\n", gcd2(16,10000000));

	//int a[10];
	int i;
	int *a;
	a = malloc(sizeof(int)*10);
	for(i = 0; i < 10; i++)
		scanf("%d", &a[i]);

	sort(a, 10);

	for(i = 0; i < 10; i++)
		printf("%d ", a[i]);
	printf("\n");
	free(a);
	return 0;
}


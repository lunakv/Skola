.section .text

.global inc2

inc2: 
	mov %edi,%eax
	add $0x7b,%eax
	retq

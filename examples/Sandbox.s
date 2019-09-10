	.file	"Sandbox.cpp"
	.intel_syntax noprefix
	.text
	.section	.rodata
	.type	_ZStL19piecewise_construct, @object
	.size	_ZStL19piecewise_construct, 1
_ZStL19piecewise_construct:
	.zero	1
	.text
	.globl	main
	.type	main, @function
main:
.LFB2406:
	.cfi_startproc
	push	rbp
	.cfi_def_cfa_offset 16
	.cfi_offset 6, -16
	mov	rbp, rsp
	.cfi_def_cfa_register 6
	call	rand@PLT
	mov	ecx, eax
	mov	edx, 1717986919
	mov	eax, ecx
	imul	edx
	sar	edx, 2
	mov	eax, ecx
	sar	eax, 31
	sub	edx, eax
	mov	eax, edx
	sal	eax, 2
	add	eax, edx
	add	eax, eax
	sub	ecx, eax
	mov	edx, ecx
	cmp	edx, 7
	jne	.L2
	call	rand@PLT
	mov	ecx, eax
	mov	edx, 1717986919
	mov	eax, ecx
	imul	edx
	sar	edx, 2
	mov	eax, ecx
	sar	eax, 31
	sub	edx, eax
	mov	eax, edx
	sal	eax, 2
	add	eax, edx
	add	eax, eax
	sub	ecx, eax
	mov	edx, ecx
	cmp	edx, 7
	jne	.L2
	mov	eax, 1
	jmp	.L3
.L2:
	mov	eax, 0
.L3:
	test	al, al
	je	.L4
	mov	eax, 0
	jmp	.L5
.L4:
	mov	eax, 1
.L5:
	pop	rbp
	.cfi_def_cfa 7, 8
	ret
	.cfi_endproc
.LFE2406:
	.size	main, .-main
	.ident	"GCC: (Ubuntu 8.3.0-6ubuntu1) 8.3.0"
	.section	.note.GNU-stack,"",@progbits

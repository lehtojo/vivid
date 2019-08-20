	.file	"Sandbox.cpp"
	.intel_syntax noprefix
	.text
	.section	.text.startup,"ax",@progbits
	.p2align 4,,15
	.globl	main
	.type	main, @function
main:
.LFB2473:
	.cfi_startproc
	push	rbp
	.cfi_def_cfa_offset 16
	.cfi_offset 6, -16
	mov	ebp, 1717986919
	push	rbx
	.cfi_def_cfa_offset 24
	.cfi_offset 3, -24
	sub	rsp, 8
	.cfi_def_cfa_offset 32
	call	rand@PLT
	mov	ecx, eax
	imul	ebp
	mov	ebx, ecx
	mov	eax, edx
	mov	edx, ecx
	sar	edx, 31
	sar	eax, 2
	sub	eax, edx
	lea	eax, [rax+rax*4]
	add	eax, eax
	sub	ebx, eax
	call	rand@PLT
	add	rsp, 8
	.cfi_def_cfa_offset 24
	mov	ecx, eax
	imul	ebp
	mov	eax, ecx
	sar	eax, 31
	sar	edx, 2
	sub	edx, eax
	lea	eax, [rdx+rdx*4]
	add	eax, eax
	sub	ecx, eax
	lea	eax, [rcx+rbx]
	imul	eax, ebx
	pop	rbx
	.cfi_def_cfa_offset 16
	pop	rbp
	.cfi_def_cfa_offset 8
	add	eax, eax
	ret
	.cfi_endproc
.LFE2473:
	.size	main, .-main
	.ident	"GCC: (Ubuntu 8.3.0-6ubuntu1) 8.3.0"
	.section	.note.GNU-stack,"",@progbits

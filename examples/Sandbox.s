	.arch armv5t
	.eabi_attribute 20, 1
	.eabi_attribute 21, 1
	.eabi_attribute 23, 3
	.eabi_attribute 24, 1
	.eabi_attribute 25, 1
	.eabi_attribute 26, 2
	.eabi_attribute 30, 6
	.eabi_attribute 34, 0
	.eabi_attribute 18, 4
	.file	"Sandbox.cpp"
	.text
	.section	.rodata
	.align	2
	.type	_ZStL19piecewise_construct, %object
	.size	_ZStL19piecewise_construct, 1
_ZStL19piecewise_construct:
	.space	1
	.align	2
	.type	_ZStL13allocator_arg, %object
	.size	_ZStL13allocator_arg, 1
_ZStL13allocator_arg:
	.space	1
	.align	2
	.type	_ZStL6ignore, %object
	.size	_ZStL6ignore, 1
_ZStL6ignore:
	.space	1
	.text
	.align	2
	.global	_Z3sumics
	.syntax unified
	.arm
	.fpu softvfp
	.type	_Z3sumics, %function
_Z3sumics:
	.fnstart
.LFB1653:
	@ args = 0, pretend = 0, frame = 8
	@ frame_needed = 1, uses_anonymous_args = 0
	@ link register save eliminated.
	str	fp, [sp, #-4]!
	add	fp, sp, #0
	sub	sp, sp, #12
	str	r0, [fp, #-8]
	mov	r3, r1
	strb	r3, [fp, #-9]
	mov	r3, r2	@ movhi
	strh	r3, [fp, #-12]	@ movhi
	ldrsh	r2, [fp, #-12]
	ldr	r3, [fp, #-8]
	add	r2, r2, r3
	ldrb	r3, [fp, #-9]	@ zero_extendqisi2
	add	r3, r2, r3
	mov	r0, r3
	add	sp, fp, #0
	@ sp needed
	ldr	fp, [sp], #4
	bx	lr
	.cantunwind
	.fnend
	.size	_Z3sumics, .-_Z3sumics
	.align	2
	.global	main
	.syntax unified
	.arm
	.fpu softvfp
	.type	main, %function
main:
	.fnstart
.LFB1654:
	@ args = 0, pretend = 0, frame = 8
	@ frame_needed = 1, uses_anonymous_args = 0
	push	{fp, lr}
	add	fp, sp, #4
	sub	sp, sp, #8
	mov	r3, #7
	strb	r3, [fp, #-5]
	ldrb	r3, [fp, #-5]	@ zero_extendqisi2
	lsl	r3, r3, #16
	asr	r3, r3, #16
	mov	r2, r3
	mov	r1, #5
	mov	r0, #3
	bl	_Z3sumics
	mov	r3, r0
	nop
	mov	r0, r3
	sub	sp, fp, #4
	@ sp needed
	pop	{fp, pc}
	.cantunwind
	.fnend
	.size	main, .-main
	.ident	"GCC: (Ubuntu/Linaro 8.3.0-6ubuntu1) 8.3.0"
	.section	.note.GNU-stack,"",%progbits

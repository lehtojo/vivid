.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V17internal_allocatex_rPh

.global _V23special_multiplicationsxx_rx
_V23special_multiplicationsxx_rx:
imul rcx, 11
mov r8, rdx
imul r8, 17
add rcx, r8
sar rdx, 2
lea rax, [rcx+rdx]
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V23special_multiplicationsxx_rx
mov rax, 1
add rsp, 40
ret

.section .data


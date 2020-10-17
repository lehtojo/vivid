section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V20register_utilizationxxxxxxx_rx
export _V20register_utilizationxxxxxxx_rx
_V20register_utilizationxxxxxxx_rx:
lea rax, [rcx+rcx]
mov r8, rdx
sal r8, 0
imul r8, 7
sub rax, r8
mov r8, rcx
imul r8, rax
imul r8, rdx
sub rcx, r8
mov rdx, [rsp+56]
add rcx, rdx
imul rax, rcx
add rax, rdx
ret

_V4initv_rx:
sub rsp, 56
mov rcx, 1
mov rdx, 1
mov r8, 1
mov r9, 1
mov qword [rsp+32], 1
mov qword [rsp+40], 1
mov qword [rsp+48], 1
call _V20register_utilizationxxxxxxx_rx
mov rax, 1
add rsp, 56
ret
section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V23special_multiplicationsxx_rx
export _V23special_multiplicationsxx_rx
_V23special_multiplicationsxx_rx:
mov rax, rcx
sal rax, 1
mov r8, rdx
imul r8, 17
add rax, r8
lea r8, [rcx*8+rcx]
add rax, r8
sar rdx, 2
add rax, rdx
ret

_V4initv_rx:
sub rsp, 40
mov rcx, 1
mov rdx, 1
call _V23special_multiplicationsxx_rx
mov rax, 1
add rsp, 40
ret
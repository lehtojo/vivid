section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V20register_utilizationxxxxxxx_rx
_V20register_utilizationxxxxxxx_rx:
lea rax, [rdi+rdi]
mov rcx, rsi
sal rcx, 0
imul rcx, 7
sub rax, rcx
mov rcx, rdi
imul rcx, rax
imul rcx, rsi
sub rdi, rcx
mov rcx, [rsp+8]
add rdi, rcx
imul rax, rdi
add rax, rcx
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 1
mov rsi, 1
mov rdx, 1
mov rcx, 1
mov r8, 1
mov r9, 1
mov qword [rsp], 1
call _V20register_utilizationxxxxxxx_rx
mov rax, 1
add rsp, 8
ret
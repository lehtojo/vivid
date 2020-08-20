section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V23special_multiplicationsxx_rx
_V23special_multiplicationsxx_rx:
mov rax, rdi
sal rax, 1
mov rcx, rsi
imul rcx, 17
add rax, rcx
lea rcx, [rdi*8+rdi]
add rax, rcx
sar rsi, 2
add rax, rsi
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 1
mov rsi, 1
call _V23special_multiplicationsxx_rx
mov rax, 1
add rsp, 8
ret
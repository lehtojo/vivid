section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

_V1fxxxxxx_rx:
add rdi, rsi
add rdi, rdx
add rdi, rcx
add rdi, r8
add rdi, r9
mov rax, rdi
ret

global _V1gxx_rx
_V1gxx_rx:
sub rsp, 8
lea rcx, [rdi+1]
mov r8, rdi
sar r8, 1
sal rdi, 2
lea r9, [rsi+1]
mov r10, rsi
sal r10, 1
sar rsi, 2
mov rdx, rdi
mov rdi, r9
mov r9, rsi
mov rsi, rdi
mov rdi, rcx
mov rcx, rsi
mov rsi, r8
mov r8, r10
mov r10, rsi
mov rsi, r10
call _V1fxxxxxx_rx
add rsp, 8
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 1
mov rsi, 1
call _V1gxx_rx
mov rax, 1
add rsp, 8
ret
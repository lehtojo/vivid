section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh
extern _V14large_functionv

global _V12multi_returnxx_rx
_V12multi_returnxx_rx:
push rbx
push rbp
sub rsp, 8
mov rbx, rsi
mov rbp, rdi
call _V14large_functionv
cmp rbp, rbx
jle _V12multi_returnxx_rx_L1
mov rax, 1
add rsp, 8
pop rbp
pop rbx
ret
jmp _V12multi_returnxx_rx_L0
_V12multi_returnxx_rx_L1:
cmp rbp, rbx
jge _V12multi_returnxx_rx_L3
mov rax, -1
add rsp, 8
pop rbp
pop rbx
ret
jmp _V12multi_returnxx_rx_L0
_V12multi_returnxx_rx_L3:
xor rax, rax
add rsp, 8
pop rbp
pop rbx
ret
_V12multi_returnxx_rx_L0:
add rsp, 8
pop rbp
pop rbx
ret

_V4initv_rx:
sub rsp, 8
mov rdi, 10
xor rsi, rsi
call _V12multi_returnxx_rx
mov rax, 1
add rsp, 8
ret
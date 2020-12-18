.section .text
.intel_syntax noprefix
.global main
main:
jmp _V4initv_rx

.extern _V14large_functionv
.extern _V17internal_allocatex_rPh

.global _V12multi_returnxx_rx
_V12multi_returnxx_rx:
push rbx
push rsi
sub rsp, 40
mov rbx, rcx
mov rsi, rdx
call _V14large_functionv
cmp rbx, rsi
jle _V12multi_returnxx_rx_L1
mov rax, 1
add rsp, 40
pop rsi
pop rbx
ret
jmp _V12multi_returnxx_rx_L0
_V12multi_returnxx_rx_L1:
cmp rbx, rsi
jge _V12multi_returnxx_rx_L3
mov rax, -1
add rsp, 40
pop rsi
pop rbx
ret
jmp _V12multi_returnxx_rx_L0
_V12multi_returnxx_rx_L3:
xor rax, rax
add rsp, 40
pop rsi
pop rbx
ret
_V12multi_returnxx_rx_L0:
add rsp, 40
pop rsi
pop rbx
ret

.global _V4initv_rx
_V4initv_rx:
sub rsp, 40
mov rcx, 10
xor rdx, rdx
call _V12multi_returnxx_rx
mov rax, 1
add rsp, 40
ret

.section .data


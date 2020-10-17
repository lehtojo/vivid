section .text
global main
main:
jmp _V4initv_rx

extern _V8allocatex_rPh

global _V12conditionalsxx_rx
export _V12conditionalsxx_rx
_V12conditionalsxx_rx:
cmp rcx, rdx
jl _V12conditionalsxx_rx_L1
mov rax, rcx
ret
jmp _V12conditionalsxx_rx_L0
_V12conditionalsxx_rx_L1:
mov rax, rdx
ret
_V12conditionalsxx_rx_L0:
ret

_V4initv_rx:
sub rsp, 40
mov rax, 1
add rsp, 40
ret
mov rcx, 1
mov rdx, 2
call _V12conditionalsxx_rx
ret
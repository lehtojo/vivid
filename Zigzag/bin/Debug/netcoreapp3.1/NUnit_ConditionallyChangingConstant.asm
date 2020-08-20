section .text
global _start
_start:
call _V4initv_rx
mov rax, 60
xor rdi, rdi
syscall

extern _V8allocatex_rPh

global _V49conditionally_changing_constant_with_if_statementxx_rx
_V49conditionally_changing_constant_with_if_statementxx_rx:
mov rcx, 7
cmp rdi, rsi
jle _V49conditionally_changing_constant_with_if_statementxx_rx_L0
mov rcx, rdi
_V49conditionally_changing_constant_with_if_statementxx_rx_L0:
add rdi, rcx
mov rax, rdi
ret

global _V51conditionally_changing_constant_with_loop_statementxx_rx
_V51conditionally_changing_constant_with_loop_statementxx_rx:
mov rax, 100
cmp rdi, rsi
jge _V51conditionally_changing_constant_with_loop_statementxx_rx_L1
_V51conditionally_changing_constant_with_loop_statementxx_rx_L0:
add rax, 1
add rdi, 1
cmp rdi, rsi
jl _V51conditionally_changing_constant_with_loop_statementxx_rx_L0
_V51conditionally_changing_constant_with_loop_statementxx_rx_L1:
imul rsi, rax
mov rax, rsi
ret

_V4initv_rx:
sub rsp, 8
mov rax, 1
add rsp, 8
ret
mov rdi, 1
mov rsi, 1
call _V49conditionally_changing_constant_with_if_statementxx_rx
mov rdi, 1
mov rsi, 1
call _V51conditionally_changing_constant_with_loop_statementxx_rx
ret